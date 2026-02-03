using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

/// 左右キーによるページ送りと双方向ワイプ演出、操作ガイドの表示制御を行うクラス
public class OpeningManager : MonoBehaviour
{
    // ======================================================================================
    // データ構造
    // ======================================================================================

    [System.Serializable]
    public struct OpeningSlide
    {
        public Sprite image;
        public AudioClip seClip;
        [Tooltip("ページ切り替え時のワイプ演出時間")]
        public float wipeDuration;
    }

    // ======================================================================================
    // 設定項目
    // ======================================================================================

    [Header("スライド設定")]
    [SerializeField] private List<OpeningSlide> slides;
    [SerializeField] private string nextSceneName = "title";

    [Header("UI要素参照")]
    [SerializeField] private Image displayImage;     // 背面：現在確定している画像
    [SerializeField] private RectTransform wipeMaskPanel; // 前面：ワイプ演出用マスクパネル
    [SerializeField] private Image wipeImage;        // 前面：ワイプ演出用画像
    [SerializeField] private CanvasGroup guideUI;    // 操作ガイド（1枚目のみ表示）

    // ======================================================================================
    // 内部変数
    // ======================================================================================

    private AudioSource audioSource;
    private float canvasWidth;
    private int currentIndex = 0;
    private bool isPageChanging = false;

    // ======================================================================================
    // ライフサイクル
    // ======================================================================================

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();

        if (displayImage != null && displayImage.canvas != null)
        {
            canvasWidth = displayImage.canvas.GetComponent<RectTransform>().rect.width;
        }
        else
        {
            canvasWidth = 1600f;
        }

        InitializeUI();
    }

    private void Start()
    {
        if (slides.Count > 0)
        {
            ShowSlideImmediate(0);
        }
    }

    private void Update()
    {
        HandleInput();
    }

    // ======================================================================================
    // 入力制御
    // ======================================================================================

    private void HandleInput()
    {
        if (isPageChanging) return;

        // 右キー：次のページへ
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            NextPage();
        }
        // 左キー：前のページへ
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            PrevPage();
        }
        // エンターキー：タイトルシーンへ（フェード付き）
        else if (Input.GetKeyDown(KeyCode.Return))
        {
            LoadNextScene();
        }
    }

    // ======================================================================================
    // ページ遷移ロジック
    // ======================================================================================

    private void NextPage()
    {
        if (currentIndex + 1 < slides.Count)
        {
            currentIndex++;
            StartCoroutine(TransitionSequence(currentIndex, true));
        }
        else
        {
            LoadNextScene();
        }
    }

    private void PrevPage()
    {
        if (currentIndex - 1 >= 0)
        {
            currentIndex--;
            StartCoroutine(TransitionSequence(currentIndex, false));
        }
    }

    private IEnumerator TransitionSequence(int index, bool isForward)
    {
        isPageChanging = true;

        // 操作ガイド表示制御
        if (guideUI != null) guideUI.gameObject.SetActive(index == 0);

        OpeningSlide targetSlide = slides[index];

        if (isForward)
        {
            wipeImage.sprite = targetSlide.image;
            if (targetSlide.seClip != null) audioSource.PlayOneShot(targetSlide.seClip);
            yield return StartCoroutine(WipeAnimation(targetSlide.wipeDuration, 0f, 1f));
            displayImage.sprite = targetSlide.image;
        }
        else
        {
            wipeImage.sprite = slides[index + 1].image;
            displayImage.sprite = targetSlide.image;
            SetWipeMaskWidth(1f);
            wipeMaskPanel.gameObject.SetActive(true);
            if (targetSlide.seClip != null) audioSource.PlayOneShot(targetSlide.seClip);
            yield return StartCoroutine(WipeAnimation(targetSlide.wipeDuration, 1f, 0f));
        }

        Canvas.ForceUpdateCanvases();
        wipeMaskPanel.gameObject.SetActive(false);
        isPageChanging = false;
    }

    private void ShowSlideImmediate(int index)
    {
        currentIndex = index;
        displayImage.sprite = slides[index].image;
        if (guideUI != null) guideUI.gameObject.SetActive(index == 0);
        wipeMaskPanel.gameObject.SetActive(false);
        isPageChanging = false;
    }

    // ======================================================================================
    // UI演出・計算
    // ======================================================================================

    private void SetWipeMaskWidth(float progress)
    {
        if (wipeMaskPanel == null) return;
        float targetLeftOffset = Mathf.Lerp(canvasWidth, 0f, progress);
        wipeMaskPanel.offsetMin = new Vector2(targetLeftOffset, wipeMaskPanel.offsetMin.y);
        wipeMaskPanel.offsetMax = new Vector2(0f, wipeMaskPanel.offsetMax.y);
    }

    /// 改良版：後半に加速するワイプ演出
    private IEnumerator WipeAnimation(float duration, float start, float end)
    {
        wipeMaskPanel.gameObject.SetActive(true);
        float time = 0;
        while (time < duration)
        {
            time += Time.unscaledDeltaTime;

            // 0.0 ~ 1.0 の進捗率
            float t = Mathf.Clamp01(time / duration);

            // 【イージングの追加：Cubic In】
            // 後半に向けて加速させるため、t を 3乗します
            float easedT = t * t * t;

            float progress = Mathf.Lerp(start, end, easedT);
            SetWipeMaskWidth(progress);
            yield return null;
        }
        SetWipeMaskWidth(end);
    }

    private void InitializeUI()
    {
        if (displayImage != null && displayImage.rectTransform != null)
        {
            displayImage.rectTransform.offsetMin = Vector2.zero;
            displayImage.rectTransform.offsetMax = Vector2.zero;
        }
        SetWipeMaskWidth(0f);
        wipeMaskPanel.gameObject.SetActive(false);
    }

    private void LoadNextScene()
    {
        if (string.IsNullOrEmpty(nextSceneName)) return;

        // SceneFader があればフェード付きでタイトルへ
        if (SceneFader.Instance != null)
        {
            SceneFader.Instance.LoadSceneWithFade(nextSceneName, FadeColor.Black);
        }
        else
        {
            SceneManager.LoadScene(nextSceneName);
        }
    }
}