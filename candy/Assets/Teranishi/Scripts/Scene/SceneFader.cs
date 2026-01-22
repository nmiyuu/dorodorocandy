using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

/// 画面フェードの色彩定義
public enum FadeColor { Black, White }

/// シーン間の画面遷移およびフェードイン・アウトの制御クラス
/// 手動配置されたCanvas要素を用い、DontDestroyOnLoadによる全シーン常駐管理を実行
public class SceneFader : MonoBehaviour
{
    // ======================================================================================
    // シングルトン・ステータス
    // ======================================================================================

    public static SceneFader Instance { get; private set; }

    /// フェード処理の実行中フラグ
    public bool IsFading { get; private set; } = false;

    // ======================================================================================
    // 設定項目
    // ======================================================================================

    [Header("UI Components")]
    [SerializeField] private Image fadePanel; // フェード演出用パネル画像

    [Header("Fade Settings")]
    public float fadeDuration = 0.8f; // フェードの所要時間

    [Header("Excluded Scenes")]
    public List<string> excludedScenes = new List<string>(); // フェード処理をスキップするシーンリスト

    private FadeColor lastFadeColor = FadeColor.Black; // 遷移時の色記憶用

    // ======================================================================================
    // ライフサイクル
    // ======================================================================================

    private void Awake()
    {
        /// 重複インスタンスの破棄と常駐設定
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            /// シーン読み込みイベントの購読
            SceneManager.sceneLoaded += OnSceneLoaded;
            InitializeFadePanel();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        /// メモリリーク防止のためのイベント購読解除
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // ======================================================================================
    // 内部セットアップ
    // ======================================================================================

    /// 起動時におけるフェードパネルの初期化
    private void InitializeFadePanel()
    {
        if (fadePanel != null)
        {
            fadePanel.gameObject.SetActive(true);
            fadePanel.color = new Color(0, 0, 0, 0); // 初期透明状態
            fadePanel.raycastTarget = false;         // 背後のUI操作を許可
        }
    }

    /// シーンロード完了直後の自動シーケンス
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (fadePanel == null) return;

        /// 指定された除外シーンでの処理スキップ
        if (excludedScenes.Contains(scene.name))
        {
            fadePanel.gameObject.SetActive(false);
            IsFading = false;

            /// 遷移中フラグの強制解除
            if (SceneDataTransfer.Instance != null) SceneDataTransfer.Instance.EndSceneChange();
            return;
        }

        /// ロード完了時の画面点滅を防止するための不透明度維持
        fadePanel.gameObject.SetActive(true);
        Color c = (lastFadeColor == FadeColor.Black) ? Color.black : Color.white;
        fadePanel.color = new Color(c.r, c.g, c.b, 1f);
        fadePanel.raycastTarget = true;

        /// シーン開始時の明転演出実行
        StartCoroutine(FadeInAfterLoad(lastFadeColor));
    }

    // ======================================================================================
    // 外部インターフェース（シーン遷移）
    // ======================================================================================

    /// 指定シーンへのフェード遷移実行
    public void LoadSceneWithFade(string sceneName, FadeColor color)
    {
        if (IsFading) return;
        lastFadeColor = color;
        StartCoroutine(LoadSceneSequence(sceneName, color));
    }

    // ======================================================================================
    // 演出ロジック（コルーチン）
    // ======================================================================================

    /// ロード後の明転およびプレイヤー操作権限の復旧
    private IEnumerator FadeInAfterLoad(FadeColor color)
    {
        yield return StartCoroutine(FadeIn(color));

        /// 遷移完了による入力ロックの解除
        if (SceneDataTransfer.Instance != null)
        {
            SceneDataTransfer.Instance.EndSceneChange();
        }

        IsFading = false;
    }

    /// 暗転（透明 → 不透明）の補間処理
    public IEnumerator FadeOut(FadeColor color)
    {
        IsFading = true;
        fadePanel.raycastTarget = true;
        Color targetColor = (color == FadeColor.Black) ? Color.black : Color.white;

        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Clamp01(timer / fadeDuration);
            fadePanel.color = new Color(targetColor.r, targetColor.g, targetColor.b, alpha);
            yield return null;
        }
        fadePanel.color = new Color(targetColor.r, targetColor.g, targetColor.b, 1f);
    }

    /// 明転（不透明 → 透明）の補間処理
    public IEnumerator FadeIn(FadeColor color)
    {
        IsFading = true;
        Color targetColor = (color == FadeColor.Black) ? Color.black : Color.white;

        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Clamp01(1f - (timer / fadeDuration));
            fadePanel.color = new Color(targetColor.r, targetColor.g, targetColor.b, alpha);
            yield return null;
        }
        fadePanel.color = new Color(targetColor.r, targetColor.g, targetColor.b, 0f);
        fadePanel.raycastTarget = false;
    }

    /// 操作制限、暗転、物理ロードを統括する遷移シーケンス
    private IEnumerator LoadSceneSequence(string sceneName, FadeColor color)
    {
        /// 入力ロックの開始
        if (SceneDataTransfer.Instance != null)
        {
            SceneDataTransfer.Instance.StartSceneChange();
        }

        yield return StartCoroutine(FadeOut(color));

        /// ロード処理中の画面維持
        fadePanel.color = (color == FadeColor.Black) ? Color.black : Color.white;

        SceneManager.LoadScene(sceneName);
    }
}