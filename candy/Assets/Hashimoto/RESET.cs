using UnityEngine;

public class RESERS : MonoBehaviour
{
    [SerializeField] private Vector3 fixedStartPosition = new Vector3(0,-1,0);
    [SerializeField] private Vector3 fixedStartRotation = Vector3.zero;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // R�L�[����������Œ�ʒu�Ƀ��Z�b�g
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetPlayer();
        }
    }

    private void ResetPlayer()
    {
        transform.position = fixedStartPosition;
        transform.rotation = Quaternion.Euler(fixedStartRotation);

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        Debug.Log("�L�������Œ肳�ꂽ�����ʒu�Ƀ��Z�b�g���܂����I");
    }
}
