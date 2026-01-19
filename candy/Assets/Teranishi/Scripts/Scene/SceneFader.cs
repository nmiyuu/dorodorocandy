using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public enum FadeColor { Black, White }

public class SceneFader : MonoBehaviour
{
    public static SceneFader Instance { get; private set; }

    [SerializeField] private Image fadePanel;
    public float fadeDuration = 0.8f;

    // フェード中フラグ（外部のスクリプトがこれを参照して動かなくなっている場合もあります）
    public bool IsFading { get; private set; } = false;

    [Header("除外設定")]
    public List<string> excludedScenes = new List<string>();

    private FadeColor lastFadeColor = FadeColor.Black;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
            InitializeFadePanel();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void InitializeFadePanel()
    {
        if (fadePanel != null)
        {
            fadePanel.gameObject.SetActive(true);
            fadePanel.color = new Color(0, 0, 0, 0);
            fadePanel.raycastTarget = false;
        }
    }

    // シーンがロードされた瞬間に呼ばれる
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (fadePanel == null) return;

        // 除外シーンの場合
        if (excludedScenes.Contains(scene.name))
        {
            fadePanel.gameObject.SetActive(false);
            IsFading = false;
            // 除外シーンでも操作ロックは解除する
            if (SceneDataTransfer.Instance != null) SceneDataTransfer.Instance.EndSceneChange();
            return;
        }

        // 【点滅防止】即座にパネルを不透明にする
        fadePanel.gameObject.SetActive(true);
        Color c = (lastFadeColor == FadeColor.Black) ? Color.black : Color.white;
        fadePanel.color = new Color(c.r, c.g, c.b, 1f);
        fadePanel.raycastTarget = true;

        // フェードインと操作解除のコルーチンを開始
        StartCoroutine(FadeInAfterLoad(lastFadeColor));
    }

    private IEnumerator FadeInAfterLoad(FadeColor color)
    {
        // 1. フェードイン実行
        yield return StartCoroutine(FadeIn(color));

        // 2. 【重要】操作を有効化する命令を出す
        if (SceneDataTransfer.Instance != null)
        {
            SceneDataTransfer.Instance.EndSceneChange();
        }

        // 3. フラグを解除
        IsFading = false;
    }

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

    public void LoadSceneWithFade(string sceneName, FadeColor color)
    {
        if (IsFading) return;
        lastFadeColor = color;
        StartCoroutine(LoadSceneSequence(sceneName, color));
    }

    private IEnumerator LoadSceneSequence(string sceneName, FadeColor color)
    {
        // 1. 【重要】シーン切り替え開始（操作ロック）
        if (SceneDataTransfer.Instance != null)
        {
            SceneDataTransfer.Instance.StartSceneChange();
        }

        // 2. フェードアウト
        yield return StartCoroutine(FadeOut(color));

        // 3. ロード中も真っ暗を維持
        fadePanel.color = (color == FadeColor.Black) ? Color.black : Color.white;

        // 4. シーンロード
        SceneManager.LoadScene(sceneName);
    }
}