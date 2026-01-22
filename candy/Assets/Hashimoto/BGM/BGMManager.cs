using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class BGMManager : MonoBehaviour
{
    // ===== シングルトン用 =====
    // どこからでも参照できる BGMManager の実体
    public static BGMManager instance;

    // ===== Inspector で設定 =====
    // ここに書いたシーン名では BGM を流さない
    [Header("BGMを流さないシーン名")]
    public List<string> noBGMScenes = new List<string>();

    // ===== 内部用 =====
    // この GameObject に付いている AudioSource
    AudioSource audioSource;

    void Awake()
    {
        // すでに BGMManager が存在していたら
        // 新しく生成された自分は削除する（二重防止）
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        // 自分を唯一の BGMManager として登録
        instance = this;

        // シーンが切り替わっても消えないようにする
        DontDestroyOnLoad(gameObject);

        // AudioSource コンポーネントを取得
        audioSource = GetComponent<AudioSource>();

        // BGM の音量（0.0 ～ 1.0）
        audioSource.volume = 1.0f;

        // シーンがロードされたときに
        // OnSceneLoaded を呼ぶよう登録
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    // シーンが切り替わった直後に自動で呼ばれる
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 「BGMを流さないシーン」に
        // 今のシーン名が含まれているか？
        if (noBGMScenes.Contains(scene.name))
        {
            // 含まれていたら BGM を停止
            audioSource.Stop();
        }
        else
        {
            // 含まれていなければ BGM を再生
            PlayBGM();
        }
    }

    // ===== BGM 再生 =====
    // すでに再生中でなければ再生する
    public void PlayBGM()
    {
        if (!audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }

    // ===== BGM 停止 =====
    public void StopBGM()
    {
        audioSource.Stop();
    }
}
