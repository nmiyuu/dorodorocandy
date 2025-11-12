using UnityEngine;

public class ButtonSoundPlayer : MonoBehaviour
{
    public AudioClip clickSound;      // 効果音ファイル
    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    // ボタンから呼び出すメソッド
    public void PlayClickSound()
    {
        if (clickSound != null)
        {
            audioSource.PlayOneShot(clickSound);
        }
    }
}
