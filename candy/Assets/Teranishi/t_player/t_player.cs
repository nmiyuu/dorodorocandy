using UnityEngine;
using System.Collections;

public class t_player : MonoBehaviour
{
    // --- �p�����[�^�ݒ� (Inspector�Őݒ�) ---
    public float moveUnit = 1.0f;       // 1��̈ړ��Ői�ދ����i1�}�X���j
    public float moveSpeed = 5f;        // �ړ��X�s�[�h
    public LayerMask obstacleLayer;     // �ǁE��Q���̃��C���[ (Tilemap�Ɠ������C���[�Ƀ`�F�b�N������)

    // --- ������� ---
    private bool isMoving = false;
    private Vector3 targetPos;
    private BoxCollider2D playerCollider;

    void Start()
    {
        // �K�{�R���|�[�l���g�̎擾
        playerCollider = GetComponent<BoxCollider2D>();
        if (playerCollider == null)
        {
            Debug.LogError("Player�I�u�W�F�N�g��BoxCollider2D��������܂���I");
            return;
        }

        // �����ʒu���^�[�Q�b�g�ʒu�ɐݒ�
        targetPos = transform.position;
    }

    void Update()
    {
        // �ړ����͐V�������͂��󂯕t���Ȃ�
        if (isMoving) return;

        Vector3 dir = Vector3.zero;

        // --- 1. ���͌��m ---
        // �����ꂽ�L�[�ɉ����Ĉړ�����������
        if (Input.GetKeyDown(KeyCode.RightArrow)) dir = Vector3.right;
        else if (Input.GetKeyDown(KeyCode.LeftArrow)) dir = Vector3.left;
        else if (Input.GetKeyDown(KeyCode.UpArrow)) dir = Vector3.up;
        else if (Input.GetKeyDown(KeyCode.DownArrow)) dir = Vector3.down;

        // �������͂��������ꍇ�̂ݏ���
        if (dir != Vector3.zero)
        {
            // --- 2. BoxCast�ɂ��ǂ̎��O�`�F�b�N ---

            // BoxCast�̔���ɕK�v�ȏ���Collider����擾
            // �n�_�́u���݂̈ʒu + Collider�̃I�t�Z�b�g�v
            Vector2 origin = (Vector2)transform.position + playerCollider.offset;
            // �T�C�Y��Collider�̃T�C�Y
            Vector2 size = playerCollider.size;
            float angle = 0f;

            // �`�F�b�N����: �ړ����� (moveUnit) ���킸���ɒ����ݒ肷��
            // �덷��^�C���̋��E���ɂ�邷�蔲����h�����߂̕K�{�̒���
            float checkDistance = moveUnit * 1.01f; // 1.01�{�̗]�T����������

            // Physics2D.BoxCast�����s:
            // �v���C���[��Collider�Ɠ����`��E�T�C�Y�ŁA�ړ���܂ŏ�Q�����Ȃ������m�F����
            RaycastHit2D hit = Physics2D.BoxCast(origin, size, angle, dir, checkDistance, obstacleLayer);

            // hit.collider �� null�i�����Փ˂��Ȃ������j�̏ꍇ�݈̂ړ������s
            if (hit.collider == null)
            {
                // �ǂ��Ȃ���ΖړI�n���X�V���A�R���[�`���Ŋ��炩�Ɉړ����J�n
                targetPos = transform.position + dir * moveUnit;
                StartCoroutine(MoveToPosition(targetPos));
            }
            // else �̏ꍇ�͕ǂ����邽�߁A�������I���i�ړ������ɂ��̏�ɗ��܂� = �ǂɓ������Ď~�܂�j
        }
    }

    // �v���C���[���^�[�Q�b�g�̈ʒu�܂Ŋ��炩�Ɉړ�������R���[�`��
    IEnumerator MoveToPosition(Vector3 target)
    {
        isMoving = true;

        // �^�[�Q�b�g�ɋ߂Â��܂Łi�����̓�悪0.001f���傫���ԁj���[�v
        while ((transform.position - target).sqrMagnitude > 0.001f)
        {
            // �^�[�Q�b�g�Ɍ����Ĉړ� (�t���[�����[�g�Ɉˑ����Ȃ��悤��Time.deltaTime���g�p)
            transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
            yield return null; // 1�t���[���ҋ@
        }

        // �Ō�̎d�グ: �덷���Ȃ������߁A���m�ȃ^�[�Q�b�g�ʒu�ɌŒ肷��
        transform.position = target;
        isMoving = false;
    }
}