using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MoveSound : MonoBehaviour
{
    public AudioClip moveSound;
    private AudioSource audioSource;
    private Vector3 lastPosition;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.clip = moveSound;
        audioSource.loop = true; // 移動中はループ再生
        lastPosition = transform.position;
    }

    void Update()
    {
        // 移動しているか判定
        if (transform.position != lastPosition)
        {
            if (!audioSource.isPlaying)
            {
                audioSource.Play(); // 移動開始で再生
            }
        }
        else
        {
            if (audioSource.isPlaying)
            {
                audioSource.Stop(); // 移動が止まったら停止
            }
        }

        lastPosition = transform.position;
    }
}