using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class OpeningManager : MonoBehaviour
{
    // --- データ構造 ---
    [System.Serializable]
    public struct OpeningSlide
    {
        public Sprite image;
       // [Range(1f, 10f)]
        public float duration;

        [Header("エフェクト")]
        public AudioClip seClip;
        [Tooltip("このスライドへの切り替え時に使うワイプ（左から徐々に表示）の時間。")]
        public float wipeDuration; // ワイプにかかる時間
    }

    // --- インスペクター設定 ---

    [Header("スライド設定")]
    public List<OpeningSlide> slides;

    [Header("遷移設定")]
    public string nextSceneName = "title"; // 遷移先シーン名

    [Header("UI要素参照")]
    [Tooltip("現在表示中の画像を担うUI.Imageコンポーネント。")]
    public Image displayImage; // 現在表示中の画像

    [Tooltip("新しい画像をマスクで徐々に表示するための親パネル（Maskコンポーネント付き）。RectTransform型で参照します。")]
    public RectTransform wipeMaskPanel; // Maskコンポーネントを持つGameObjectのRectTransform

    [Tooltip("マスクされて徐々に表示される側の画像UI.Imageコンポーネント。")]
    public Image wipeImage; // マスクされて表示される次の画像

    // --- 内部変数 ---
    private AudioSource audioSource;
    private Coroutine openingCoroutine;
    private float canvasWidth; // ワイプ計算に使用するキャンバスの幅
    private bool isSkipping = false; // スキップ中かどうかのフラグ

    void Awake()
    {
        // AudioSourceの取得/追加
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // キャンバスの幅を取得 (Awake時に実行し、Start/Updateで頻繁にGetComponentしないようにする)
        if (displayImage != null && displayImage.canvas != null)
        {
            // CanvasのRectTransformから幅を取得
            canvasWidth = displayImage.canvas.GetComponent<RectTransform>().rect.width;
        }
        else
        {
            Debug.LogError("Canvasの幅を取得できません。Display ImageまたはそのCanvasが正しく設定されていません。幅の計算はデフォルト値(1600)を使用します。");
            canvasWidth = 1600f;
        }

        // 初期設定：displayImageには最初のスライドを設定
        if (slides.Count > 0 && slides[0].image != null)
        {
            displayImage.sprite = slides[0].image;
        }

        // DisplayImageのRectTransformのオフセットを強制ゼロにリセット
        if (displayImage != null && displayImage.rectTransform != null)
        {
            displayImage.rectTransform.offsetMin = Vector2.zero;
            displayImage.rectTransform.offsetMax = Vector2.zero;
        }

        // ワイプパネルを初期状態で完全に隠す (Right Offset = canvasWidth)
        SetWipeMaskWidth(0f);
        if (wipeMaskPanel != null)
        {
            // Awake時点ではパネルを非アクティブにしておく (UIの初期描画負荷を減らすため)
            wipeMaskPanel.gameObject.SetActive(false);
        }
    }

    void Start()
    {
        // UI参照のチェック
        if (displayImage == null || wipeMaskPanel == null || wipeImage == null)
        {
            Debug.LogError("UI要素の参照が不足しています。オープニングを開始できません。");
            return;
        }

        // 最初のスライド表示からコルーチンを開始
        openingCoroutine = StartCoroutine(PlayOpeningSlides());
    }

    // --- スキップ機能 ---
    void Update()
    {
        // 任意のキーが押されたか、またはマウスがクリックされたらスキップ
        if (Input.anyKeyDown || Input.GetMouseButtonDown(0))
        {
            if (!isSkipping && openingCoroutine != null)
            {
                SkipOpening();
            }
        }
    }

    private void SkipOpening()
    {
        isSkipping = true;

        // 実行中のコルーチンを停止
        if (openingCoroutine != null)
        {
            StopCoroutine(openingCoroutine);
            openingCoroutine = null;
        }

        // タイトルシーンへ遷移
        LoadNextScene();
    }

    // --- メインロジック ---

    private IEnumerator PlayOpeningSlides()
    {
        // 最初のスライドはワイプなしで表示済みとして扱う
        if (slides.Count > 0 && slides[0].seClip != null)
        {
            audioSource.PlayOneShot(slides[0].seClip);
        }
        // 最初のスライドの表示時間
        yield return new WaitForSeconds(slides[0].duration);

        // 2枚目以降のスライドをワイプで表示
        for (int i = 1; i < slides.Count; i++) // i=1から開始
        {
            // スキップ中であれば即座に終了
            if (isSkipping) yield break;

            OpeningSlide currentSlide = slides[i];

            if (currentSlide.image != null)
            {
                // 1. 新しい画像をwipeImageに設定し、パネルをアクティブにする
                wipeImage.sprite = currentSlide.image;
                // ワイプ開始前に必ず幅を0に戻し、完全に隠す 
                SetWipeMaskWidth(0f);
                wipeMaskPanel.gameObject.SetActive(true); // パネルをアクティブ化

                // 2. SEの再生
                if (currentSlide.seClip != null)
                {
                    audioSource.PlayOneShot(currentSlide.seClip);
                }

                // 3. ワイプアニメーションを実行
                yield return StartCoroutine(WipeAnimation(currentSlide.wipeDuration));

                // スキップ中であればワイプアニメーション後に即座に終了
                if (isSkipping) yield break;

                // 4. ワイプ完了後の描画処理（残像対策 - 座標変化を遅延させる）

                // (A) Display Imageを新しいスライドに更新 (奥の画像を更新)
                displayImage.sprite = currentSlide.image;

                // (B) 描画を強制的に確定
                Canvas.ForceUpdateCanvases();

                // (C) スプライトをクリア (残像対策)
                ResetWipePanel();

                // (D) ワイプパネルを完全に画面外へ移動させる
                SetWipeMaskWidth(0f); // 右端に移動 (隠れた状態に戻す)

                // (E) SetWipeMaskWidthによる座標変化を処理するため2フレーム待機
                yield return null;
                yield return null;

                // (F) パネルを非アクティブ化 (完全にHierarchyから要素を削除)
                wipeMaskPanel.gameObject.SetActive(false);

                // (G) 描画システムがリセットを処理し終えるまで1フレーム待機
                yield return null;

                // 5. 指定された時間だけ待機
                yield return new WaitForSeconds(currentSlide.duration);
            }
        }

        // スキップ中でなければ、すべて完了後に次のシーンへ遷移
        if (!isSkipping)
        {
            LoadNextScene();
        }
    }

    // ワイプアニメーションを実行するコルーチン (マスクの幅を広げる)
    private IEnumerator WipeAnimation(float duration)
    {
        if (duration <= 0.01f)
        {
            SetWipeMaskWidth(1f); // 即座に完全に表示 (Left Offset = 0)
            Canvas.ForceUpdateCanvases(); // duration=0の場合でも強制的に描画を確定させる
            yield return null; // 描画確定後に1フレーム待機
            yield break;
        }

        float time = 0;
        // Time.unscaledDeltaTimeを使用し、実行時間を安定化させる
        while (time < duration)
        {
            // スキップ中であればアニメーションを中断
            if (isSkipping) yield break;

            time += Time.unscaledDeltaTime;
            float progress = time / duration; // 0から1へ
            SetWipeMaskWidth(progress); // マスクの幅を更新 (Left Offset: canvasWidth -> 0)
            yield return null; // 次のフレームまで待機
        }
        SetWipeMaskWidth(1f); // 終了時にターゲット値 (Left Offset = 0) を保証
        Canvas.ForceUpdateCanvases(); // アニメーション終了時にも強制描画を確定させる
    }

    // wipeMaskPanelの幅を制御するヘルパー関数
    private void SetWipeMaskWidth(float progress)
    {
        if (wipeMaskPanel == null) return;

        // 右から左へ広がるワイプを実現
        // progress 0 (隠れる - 右端に幅0) のとき Left Offset = canvasWidth (1600)
        // progress 1 (表示完了 - 画面全体) のとき Left Offset = 0

        float targetLeftOffset = Mathf.Lerp(canvasWidth, 0f, progress); // 1600 -> 0

        // 左端のオフセットを設定 (アニメーションさせる側)
        // offsetMin.x が 1600 から 0 に減ることで、マスクは右から左へ広がる
        wipeMaskPanel.offsetMin = new Vector2(targetLeftOffset, wipeMaskPanel.offsetMin.y);

        // 右端は固定 (マージン 0)
        wipeMaskPanel.offsetMax = new Vector2(0f, wipeMaskPanel.offsetMax.y);
    }

    /// <summary>
    /// ワイプパネルを非表示後の初期状態にリセットします。（スプライトをクリア）
    /// </summary>
    private void ResetWipePanel()
    {
        if (wipeMaskPanel != null)
        {
            // ワイプイメージのスプライトをクリア (残像対策) 
            if (wipeImage != null)
            {
                wipeImage.sprite = null;
            }
        }
    }

    private void LoadNextScene()
    {
        if (string.IsNullOrEmpty(nextSceneName))
        {
            Debug.LogError("遷移先シーン名が設定されていません！処理を中断します。");
            return;
        }
        string logMessage = isSkipping ?
            $"キー操作によりシーン '{nextSceneName}' へスキップ遷移します。" :
            $"シーン '{nextSceneName}' へ遷移します。";

        Debug.Log(logMessage);
        SceneManager.LoadScene(nextSceneName);
    }
}