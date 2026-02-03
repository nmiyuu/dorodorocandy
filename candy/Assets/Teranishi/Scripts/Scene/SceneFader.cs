using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public enum FadeColor { Black, White }

public class SceneFader : MonoBehaviour
{
    public static SceneFader Instance { get; private set; }
    public bool IsFading { get; private set; } = false;

    [Header("UI Components")]
    [SerializeField] private Image fadePanel;

    [Header("Fade Settings")]
    public float fadeDuration = 0.8f;

    [Header("Excluded Scenes")]
    public List<string> excludedScenes = new List<string>();

    private FadeColor lastFadeColor = FadeColor.Black;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;

            // 【重要】起動時も、まずはパネルを準備（初期は透明でも良いが、念のため）
            InitializeFadePanel();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void InitializeFadePanel()
    {
        if (fadePanel != null)
        {
            fadePanel.gameObject.SetActive(true);
            // 初期状態は、あえて真っ暗から始めるか透明にするかは好みですが、
            // 起動時のロゴなどを見せたい場合は(0,0,0,0)でOK
            fadePanel.color = new Color(0, 0, 0, 0);
            fadePanel.raycastTarget = false;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (fadePanel == null) return;

        if (excludedScenes.Contains(scene.name))
        {
            fadePanel.gameObject.SetActive(false);
            IsFading = false;
            if (SceneDataTransfer.Instance != null) SceneDataTransfer.Instance.EndSceneChange();
            return;
        }

        // 【最強の点滅対策】
        // コルーチンが動く「前」のこの瞬間に、物理的に色を100%不透明にする
        StopAllCoroutines(); // 実行中のフェードアウト等を強制停止

        fadePanel.gameObject.SetActive(true);
        Color c = (lastFadeColor == FadeColor.Black) ? Color.black : Color.white;
        fadePanel.color = new Color(c.r, c.g, c.b, 1f); // 強制的にアルファ1
        fadePanel.raycastTarget = true;

        // 1フレーム待ってから明転を開始することで、ロード直後のガタつきを吸収
        StartCoroutine(WaitAndFadeIn(lastFadeColor));
    }

    private IEnumerator WaitAndFadeIn(FadeColor color)
    {
        // ロード直後の1フレームは処理が重いので、少しだけ待ってからフェード開始
        yield return null;
        yield return StartCoroutine(FadeInAfterLoad(color));
    }

    public void LoadSceneWithFade(string sceneName, FadeColor color)
    {
        if (IsFading) return;
        lastFadeColor = color;
        StartCoroutine(LoadSceneSequence(sceneName, color));
    }

    private IEnumerator FadeInAfterLoad(FadeColor color)
    {
        yield return StartCoroutine(FadeIn(color));
        if (SceneDataTransfer.Instance != null) SceneDataTransfer.Instance.EndSceneChange();
        IsFading = false;
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

    private IEnumerator LoadSceneSequence(string sceneName, FadeColor color)
    {
        if (SceneDataTransfer.Instance != null) SceneDataTransfer.Instance.StartSceneChange();

        yield return StartCoroutine(FadeOut(color));

        // シーン切り替え直前、色を完全に固定
        fadePanel.color = (color == FadeColor.Black) ? Color.black : Color.white;

        SceneManager.LoadScene(sceneName);
    }
}