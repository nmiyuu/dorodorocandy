using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

// フェード色を定義するEnum
public enum FadeColor { Black, White }

// フェードイン/アウトとシーン遷移を制御するシングルトン
public class SceneFader : MonoBehaviour
{
    public static SceneFader Instance { get; private set; }

    [SerializeField]
    private Image fadePanel;

    public float fadeDuration = 0.8f;

    // フェード中は次の遷移をブロックするためのフラグ
    public bool IsFading { get; set; } = false;

    // シーン遷移時に最後に使用されたフェード色を保持
    private FadeColor lastFadeColor = FadeColor.Black;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (fadePanel == null)
            {
                Debug.LogError("FadePanelがアタッチされていません！");
            }

            // 新しいシーンがロードされたときに実行するイベントを登録
            SceneManager.sceneLoaded += OnSceneLoaded;

            // 初期化をAwakeで行う
            InitializeFadePanel();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // 永続化オブジェクトのためStartは再利用できないが、Awakeで初期化済み
    }

    void OnDestroy()
    {
        // オブジェクトが破棄される時にイベントの登録を解除する
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // シーンロード完了時に呼ばれるメソッド
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // シーンがロードされたら、必ずフェードインを開始する
        if (fadePanel != null)
        {
            // 最後に使用された色でフェードインを開始
            StartCoroutine(FadeIn(lastFadeColor));
        }
    }


    // フェードパネルを初期状態（完全に不透明）に戻す
    private void InitializeFadePanel()
    {
        if (fadePanel != null)
        {
            // ゲーム開始時は一旦、完全に不透明（黒）にしておく
            fadePanel.color = new Color(0f, 0f, 0f, 1f);
            fadePanel.gameObject.SetActive(true);
        }
    }

    // 暗転からゲーム画面へ明るくする（ロード完了時）
    public IEnumerator FadeIn(FadeColor color)
    {
        Color targetColor = (color == FadeColor.Black) ? Color.black : Color.white;

        IsFading = true;

        float timer = fadeDuration;
        while (timer > 0f)
        {
            timer -= Time.deltaTime;
            float alpha = timer / fadeDuration;

            // ターゲット色とアルファ値で色を設定
            fadePanel.color = new Color(targetColor.r, targetColor.g, targetColor.b, alpha);
            yield return null;
        }
        fadePanel.color = new Color(targetColor.r, targetColor.g, targetColor.b, 0f);

        // フェード終了フラグを解除することで、次の遷移を許可する
        IsFading = false;
    }

    // ゲーム画面から暗転させる（シーン遷移前）
    public IEnumerator FadeOut(FadeColor color)
    {
        // フェード中フラグを設定（連打防止）
        IsFading = true;

        Color targetColor = (color == FadeColor.Black) ? Color.black : Color.white;

        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float alpha = timer / fadeDuration;

            // ターゲット色とアルファ値で色を設定
            fadePanel.color = new Color(targetColor.r, targetColor.g, targetColor.b, alpha);
            yield return null;
        }
        fadePanel.color = new Color(targetColor.r, targetColor.g, targetColor.b, 1f);

        // LoadSceneSequence()に処理が戻る
    }

    // シーン切り替えとフェードアウト/インを連続で実行する
    public void LoadSceneWithFade(string sceneName, FadeColor color)
    {
        // 連打防止のチェック！
        if (IsFading)
        {
            return;
        }

        // 最後に使用した色を保存
        lastFadeColor = color;

        StartCoroutine(LoadSceneSequence(sceneName, color));
    }

    private IEnumerator LoadSceneSequence(string sceneName, FadeColor color)
    {
        // 1. フェードアウト（画面が暗くなる）
        yield return StartCoroutine(FadeOut(color));

        // 2. シーンをロードする（OnSceneLoadedが呼ばれFadeInが開始される）
        SceneManager.LoadScene(sceneName);

        // シーンロード後は処理をOnSceneLoadedとFadeInに任せる
    }
}