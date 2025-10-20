using UnityEngine;
using System.Collections; // IEnumerator�̂��߂ɕK�v�ł�

public class t_player : MonoBehaviour
{
    public float moveUnit = 1.0f;      // 1��̈ړ��Ői�ދ����i1�}�X���j
    public float moveSpeed = 5f;       // �ړ��X�s�[�h
    public LayerMask Obstacle;         // �ǁE��Q���̃��C���[

    private bool isMoving = false;
    private Vector3 targetPos;

    // BoxCollider2D��Start�Ŏ擾���A�J��Ԃ�GetComponent�������
    private BoxCollider2D playerCollider;

    void Start()
    {
        // Start�ň�x����BoxCollider���擾
        playerCollider = GetComponent<BoxCollider2D>();
        if (playerCollider == null)
        {
            Debug.LogError("Player�I�u�W�F�N�g��BoxCollider2D��������܂���I");
        }

        targetPos = transform.position;
    }

    void Update()
    {
        if (isMoving) return;

        Vector3 dir = Vector3.zero;

        // �����L�[���͂����m
        if (Input.GetKeyDown(KeyCode.RightArrow)) dir = Vector3.right;
        else if (Input.GetKeyDown(KeyCode.LeftArrow)) dir = Vector3.left;
        else if (Input.GetKeyDown(KeyCode.UpArrow)) dir = Vector3.up;
        else if (Input.GetKeyDown(KeyCode.DownArrow)) dir = Vector3.down;

        if (dir != Vector3.zero)
        {
            // BoxCast�̔���ɕK�v�ȏ����擾
            Vector2 origin = (Vector2)transform.position + playerCollider.offset; // �n�_���I�t�Z�b�g������
            Vector2 size = playerCollider.size;
            float angle = 0f; // �v���C���[�̉�]���Ȃ��O��

            // BoxCast���g���A�ǂ����邩�`�F�b�N
            // BoxCastAll���g���ƁA�����̓����蔻��i�^�C���}�b�v�̕����̃^�C���j�����m�Ɍ��o�ł��܂�
            // ������ moveUnit - ���e�덷(epsilon) �ɐݒ肷�邱�ƂŁA�s�b�^���אڂ����ǂɂԂ���̂�h���܂�
            float distance = moveUnit;

            // BoxCast���g���ĕǔ���
            // BoxCast�� true ��Ԃ����ꍇ�i�����ɓ��������ꍇ�j�͈ړ����Ȃ�
            if (!Physics2D.BoxCast(origin, size, angle, dir, distance, Obstacle))
            {
                // �ǂ��Ȃ���ΖړI�n���X�V���Ĉړ����J�n
                targetPos = transform.position + dir * moveUnit;
                StartCoroutine(MoveToPosition(targetPos));
            }
        }
    }

    // �v���C���[���^�[�Q�b�g�̈ʒu�܂Ŋ��炩�Ɉړ������鏈���iIEnumerator��System.Collections�ɂ���܂��j
    IEnumerator MoveToPosition(Vector3 target)
    {
        isMoving = true;

        while ((transform.position - target).sqrMagnitude > 0.001f)
        {
            transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = target;
        isMoving = false;
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("a");
    }
}