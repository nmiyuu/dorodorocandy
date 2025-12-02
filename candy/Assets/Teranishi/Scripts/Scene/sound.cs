using UnityEngine;

public class SoundManager : MonoBehaviour
{
    // --- シングルトンパターン ---
    public static SoundManager Instance { get; private set; }

    [Header("オーディオソース設定")]
    [Tooltip("SEを再生するためのAudioSource")]
    [SerializeField] private AudioSource seAudioSource;

    [Header("オーディオクリップ (インスペクターで設定)")]
    [Tooltip("シーン切り替え時に鳴らすSE")]
    public AudioClip sceneTransitionSE;


    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (seAudioSource == null)
        {
            // AudioSourceが設定されていない場合、自動的に追加する
            seAudioSource = gameObject.AddComponent<AudioSource>();
            seAudioSource.playOnAwake = false; // 自動再生は無効
            Debug.LogWarning("[SoundManager] AudioSourceが設定されていなかったため、自動で追加しました。");
        }
    }

    /// <summary>
    /// 指定された AudioClip を再生する
    /// </summary>
    /// <param name="clip">再生するオーディオクリップ</param>
    public void PlaySE(AudioClip clip)
    {
        if (seAudioSource != null && clip != null)
        {
            seAudioSource.PlayOneShot(clip);
        }
        else if (clip == null)
        {
            Debug.LogWarning("[SoundManager] 再生しようとした AudioClip が null です。");
        }
    }

    /// <summary>
    /// シーン遷移SEを再生する
    /// </summary>
    public void PlaySceneTransitionSE()
    {
        PlaySE(sceneTransitionSE);
    }
}