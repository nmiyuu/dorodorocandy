using UnityEngine;
using System.Collections; // ���C���_1: �R���[�`��(IEnumerator)���g�����߂ɕK�v

public class MoveBlock : MonoBehaviour
{
    public float moveUnit = 1.0f; // �v���C���[�� moveUnit �ƈ�v������
    public float moveSpeed = 5f;
    public LayerMask pushBlockerLayer; // �u���b�N�̈ړ���W�����Q���i�����Ȃ��ǂ�ʂ̃u���b�N�j

    private bool isMoving = false;
    private Vector3 targetPos;
    private BoxCollider2D blockCollider;

    void Start()
    {
        blockCollider = GetComponent<BoxCollider2D>();
        // Collider��������Ȃ��ꍇ�̃G���[�`�F�b�N
        if (blockCollider == null)
        {
            Debug.LogError("MoveBlock�I�u�W�F�N�g��BoxCollider2D��������܂���I");
        }
        targetPos = transform.position;
    }

    // �v���C���[����Ă΂��ړ������݂郁�\�b�h
    public bool TryMove(Vector3 direction)
    {
        // �����̊ȗ����̂��߁ATryMove���ł̂�Null�`�F�b�N�����s
        if (isMoving || blockCollider == null) return false;

        // ... (BoxCast�̌v�Z�͏ȗ�) ...
        Vector2 origin = (Vector2)transform.position + blockCollider.offset;
        Vector2 size = blockCollider.size;
        float angle = 0f;
        float checkDistance = moveUnit * 1.01f;

        RaycastHit2D hit = Physics2D.BoxCast(origin, size, angle, direction, checkDistance, pushBlockerLayer);

        if (hit.collider == null)
        {
            // �ړ��悪�󂢂Ă��邽�߁A�ړ����J�n
            targetPos = transform.position + direction * moveUnit;

            // StartCoroutine�̌Ăяo��
            StartCoroutine(MoveToPosition(targetPos));
            return true; // �ړ�����
        }
        else
        {
            // �ړ���ɕǂ�ʂ̃u���b�N�����邽�߁A�ړ����s
            return false;
        }
    }

    // �u���b�N���^�[�Q�b�g�̈ʒu�܂Ŋ��炩�Ɉړ�������R���[�`��
    // ���C���_2: System.Collections. ���O���AIEnumerator�݂̂ɂ���
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
}