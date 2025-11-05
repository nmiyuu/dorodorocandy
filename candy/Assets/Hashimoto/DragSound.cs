using UnityEngine;

public class DragSound : MonoBehaviour
{
    public AudioSource audioSource;
    public Rigidbody rb;
    public float minSpeed = 0.1f; // ������n�߂�Œᑬ�x

    void Update()
    {
        // ���̂�������x�̑��x�œ����Ă��邩�`�F�b�N
        if (rb.linearVelocity.magnitude > minSpeed)
        {
            // �����Ă��āA�܂��Đ����Ă��Ȃ���΍Đ�
            if (!audioSource.isPlaying)
                audioSource.Play();
        }
        else
        {
            // �قƂ�Ǔ����Ă��Ȃ���Β�~
            if (audioSource.isPlaying)
                audioSource.Stop();
        }
    }
}
