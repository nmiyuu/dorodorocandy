using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MoveSound : MonoBehaviour
{
    public AudioClip moveSound; // インスペクタでmp3を指定
    private AudioSource audioSource;
    private Vector3 lastPosition;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        lastPosition = transform.position;
    }

    void Update()
    {
        // 位置が変わったらSE再生
        if (transform.position != lastPosition)
        {
            if (!audioSource.isPlaying)
            {
                audioSource.PlayOneShot(moveSound);
            }
            lastPosition = transform.position;
        }
    }
}