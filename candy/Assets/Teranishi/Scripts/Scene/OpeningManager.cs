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
        [Range(1f, 10f)]
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
    public string nextSceneName = "Title";

    [Header("UI要素参照")]
    [Tooltip("現在表示中の画像を担うUI.Imageコンポーネント。")]
    public Image displayImage; // 現在表示中の画像

    [Tooltip("新しい画像をマスクで徐々に表示するための親パネル（Maskコンポーネント付き）。")]
    public RectTransform wipeMaskPanel; // Maskコンポーネントを持つGameObjectのRectTransform

    [Tooltip("マスクされて徐々に表示される側の画像UI.Imageコンポーネント。")]
    public Image wipeImage; // マスクされて表示される次の画像

    // --- オーディオ要素 ---
    private AudioSource audioSource;

    private Coroutine openingCoroutine;
    private float canvasWidth; // ワイプ計算に使用するキャンバスの幅

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // キャンバスの幅を取得
        if (displayImage != null && displayImage.canvas != null)
        {
            canvasWidth = displayImage.canvas.GetComponent<RectTransform>().rect.width;
        }
        else
        {
            Debug.LogError("Canvasの幅を取得できません。Display ImageまたはそのCanvasが正しく設定されていません。");
            return;
        }

        // 初期設定：displayImageには最初のスライドを設定
        if (slides.Count > 0 && slides[0].image != null)
        {
            displayImage.sprite = slides[0].image;
        }
        else
        {
            Debug.LogWarning("最初のスライド画像が設定されていません。");
        }

        // wipeMaskPanelとwipeImageは初期状態で非表示/マスクしておく
        if (wipeMaskPanel != null)
        {
            SetWipeMaskWidth(0f); // マスクの幅を0にして完全に隠す
            wipeMaskPanel.gameObject.SetActive(false); // パネル自体も非アクティブにする
        }
    }

    void Start()
    {
        if (displayImage == null || wipeMaskPanel == null || wipeImage == null)
        {
            Debug.LogError("UI要素の参照が不足しています。オープニングを開始できません。");
            return;
        }

        // スキップボタンの処理など、他のロジックは省略
        // ...

        // 最初のスライド表示からコルーチンを開始
        openingCoroutine = StartCoroutine(PlayOpeningSlides());
    }

    // --- メインロジック ---

    private IEnumerator PlayOpeningSlides()
    {
        // 最初のスライドはワイプなしで表示済みとして扱う
        Debug.Log("最初のスライドを表示中 (ワイプなし)。");
        if (slides.Count > 0 && slides[0].seClip != null)
        {
            audioSource.PlayOneShot(slides[0].seClip);
        }
        yield return new WaitForSeconds(slides[0].duration); // 最初のスライドの表示時間

        // 2枚目以降のスライドをワイプで表示
        for (int i = 1; i < slides.Count; i++) // i=1から開始
        {
            OpeningSlide currentSlide = slides[i];

            if (currentSlide.image != null)
            {
                // 1. wipeMaskPanelをアクティブにし、新しい画像をwipeImageに設定
                wipeMaskPanel.gameObject.SetActive(true);
                wipeImage.sprite = currentSlide.image;

                // 2. SEの再生
                if (currentSlide.seClip != null)
                {
                    audioSource.PlayOneShot(currentSlide.seClip);
                }

                Debug.Log($"スライド {i + 1} をワイプで表示中: {currentSlide.duration} 秒, ワイプ時間: {currentSlide.wipeDuration} 秒");

                // 3. ワイプアニメーション (左から右へマスクを広げる)
                yield return StartCoroutine(WipeAnimation(currentSlide.wipeDuration));

                // ワイプが完了したら、displayImageを更新し、wipeMaskPanelをリセット（非アクティブ＆幅0に）
                displayImage.sprite = currentSlide.image;
                SetWipeMaskWidth(0f); // マスクの幅を0に戻す
                wipeMaskPanel.gameObject.SetActive(false); // パネルを非アクティブにする

                // 4. 指定された時間だけ待機
                yield return new WaitForSeconds(currentSlide.duration);
            }
            else
            {
                Debug.LogWarning($"スライド {i + 1} の画像が設定されていません。次のスライドへスキップします。");
            }
        }

        // すべてのスライドが終了したら、次のシーンへ遷移
        LoadNextScene();
    }

    // ワイプアニメーションを実行するコルーチン (マスクの幅を広げる)
    private IEnumerator WipeAnimation(float duration)
    {
        if (duration <= 0.01f)
        {
            SetWipeMaskWidth(1f); // 即座に完全に表示
            yield break;
        }

        float time = 0;
        while (time < duration)
        {
            time += Time.deltaTime;
            float progress = time / duration; // 0から1へ
            SetWipeMaskWidth(progress); // マスクの幅を更新
            yield return null;
        }
        SetWipeMaskWidth(1f); // 終了時にターゲット値を保証
    }

    // wipeMaskPanelの幅を制御するヘルパー関数 (progress: 0.0f = 幅0, 1.0f = 画面幅いっぱい)
    private void SetWipeMaskWidth(float progress)
    {
        if (wipeMaskPanel == null) return;

        // anchorMin.xとanchorMax.xを左端(0)に固定し、右端をprogressで動かす
        // 左に揃えたいのでLeft (RectTransform.offsetMin.x) を 0 に固定し、
        // Right (RectTransform.offsetMax.x) を動かす。
        // アンカーをLeft Stretch (min.x=0, max.x=0, stretch y) に設定していると仮定する。

        float targetRightOffset = Mathf.Lerp(canvasWidth, 0f, progress); // progress 0でRight Offset=canvasWidth (隠れる), progress 1でRight Offset=0 (表示)

        // RectTransformを操作して幅を変える (アンカーがLeft Stretchの前提)
        wipeMaskPanel.offsetMin = new Vector2(0f, wipeMaskPanel.offsetMin.y); // Leftを0に固定
        wipeMaskPanel.offsetMax = new Vector2(-targetRightOffset, wipeMaskPanel.offsetMax.y); // Rightを動かす
    }

    // ... (SkipToNextScene, LoadNextScene の定義は省略/既存のものを利用) ...
    public void SkipToNextScene()
    {
        if (openingCoroutine != null)
        {
            StopCoroutine(openingCoroutine);
            openingCoroutine = null;
        }
        LoadNextScene();
    }

    private void LoadNextScene()
    {
        if (string.IsNullOrEmpty(nextSceneName))
        {
            Debug.LogError("遷移先シーン名が設定されていません！処理を中断します。");
            return;
        }
        Debug.Log($"オープニング終了。シーン '{nextSceneName}' へ遷移します。");
        SceneManager.LoadScene(nextSceneName);
    }
}