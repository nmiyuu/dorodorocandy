using UnityEngine;
using System.Collections;

/// <summary>
/// �v���C���[�̈ړ��ƏՓ˔�����Ǘ�����R�A�X�N���v�g���B
/// ���̏����o�[�W�����ł́A�L�[���͂ɉ����đ����Ɉړ����J�n�i�������L���j����B
/// </summary>
public class t_player : MonoBehaviour
{
    // --- �p�����[�^�ݒ� (Inspector�ݒ�p) ---
    public float moveUnit = 1.0f;        // 1��̈ړ��Ői�ދ����i�O���b�h�P�ʁj
    public float moveSpeed = 5f;         // 1�}�X�ړ��ɂ����鑬�x
    public LayerMask obstacleLayer;      // �Փ˔���Ώۂ̃��C���[�iWall, Block�Ȃǁj

    // --- ������ԂƃR���|�[�l���g ---
    [SerializeField]
    private bool isMoving = false;       // ���A�ړ������ǂ����̃t���O
    private Vector3 targetPos;           // ���ɖڎw���ړ��ڕW���W
    private BoxCollider2D playerCollider;

    // t_pl.cs�ւ̎Q�Ƃ́A���̎��_�ł͂܂��擾���Ă��Ȃ��B

    // --- ���J�v���p�e�B (�O���A�g�p) ---
    // TimeTravelController�Ȃǂ����̈ʒu��ǂݎ��
    public Vector3 CurrentTargetPosition
    {
        get { return targetPos; }
    }

    public bool IsPlayerMoving
    {
        get { return isMoving; }
    }

    // --- Unity���C�t�T�C�N�� ---

    /// <summary>
    /// �����������B�R���|�[�l���g�̎擾�ƃV�[�����[�h���̈ʒu����������B
    /// </summary>
    void Awake()
    {
        playerCollider = GetComponent<BoxCollider2D>();

        if (playerCollider == null)
        {
            Debug.LogError("[t_player] �K�{�R���|�[�l���gBoxCollider2D��������Ȃ��B", this);
            return;
        }

        // �V�[���؂�ւ����̈ʒu���[�h���� (SceneDataTransfer�Ɉˑ�)
        if (SceneDataTransfer.Instance != null)
        {
            Vector3 loadPosition = SceneDataTransfer.Instance.playerPositionToLoad;

            if (loadPosition != Vector3.zero)
            {
                transform.position = loadPosition;
                targetPos = loadPosition;
                return;
            }
        }

        // ���[�h�f�[�^���Ȃ��ꍇ�́A���݂̈ʒu��ڕW�ɐݒ�
        targetPos = transform.position;
    }

    void Update()
    {
        // �ړ����͐V�������͂��󂯕t�����A�ړ�������D�悷��
        if (isMoving) return;

        Vector3 dir = Vector3.zero;

        // --- ���͎�t�ƈړ������̌��� (�������h�~�̂��� GetKeyDown ���g�p) ---
        // GetKeyDown�̓L�[�������ꂽ�t���[���ł̂�true��Ԃ����߁A�������ɂ��A���ړ���h���B

        bool keyWasPressed = false; // �L�[�������ꂽ�u�Ԃ������������`�F�b�N����t���O

        // �������́i�㉺�j���Ƀ`�F�b�N
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            dir = Vector3.up;
            keyWasPressed = true;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            dir = Vector3.down;
            keyWasPressed = true;
        }

        // �������́i���E�j���`�F�b�N���A�������͂��Ȃ������ꍇ�ɏ������s��
        // �i�΂߈ړ���h�����߁A�����ł�else if���g�p�����A��ɉ����ꂽ�������͂�D�悷��j
        if (dir == Vector3.zero) // ���������̃L�[��������Ă��Ȃ������ꍇ�̂݁A�����������`�F�b�N
        {
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                dir = Vector3.right;
                keyWasPressed = true;
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                dir = Vector3.left;
                keyWasPressed = true;
            }
        }
        // ������: ���̃��W�b�N�́u�㉺�L�[�̓��͂����E�L�[�����D�悷��v�Ƃ����Â��d�l�̂܂܂��B

        // �ǂ��炩�̃L�[�������ꂽ�u�ԁA���ړ����������肵���ꍇ�̂݁A�Փ˔���ƈړ����J�n����
        if (keyWasPressed && dir != Vector3.zero)
        {
            // --- BoxCast�ɂ��ǂ̎��O�`�F�b�N ---

            // BoxCast�̔���ɕK�v�ȏ���Collider����擾
            Vector2 origin = (Vector2)transform.position + playerCollider.offset;
            Vector2 size = playerCollider.size;
            float angle = 0f;

            // �`�F�b�N����: �ړ����� (moveUnit) ���킸���ɒ����ݒ�
            float checkDistance = moveUnit * 1.01f;

            // Physics2D.BoxCast�����s
            RaycastHit2D hit = Physics2D.BoxCast(origin, size, angle, dir, checkDistance, obstacleLayer);

            // --- �ړ��܂��̓u���b�N�����̎��s ---
            if (hit.collider == null)
            {
                // �Փ˂Ȃ�: �ړ������s
                targetPos = transform.position + dir * moveUnit;
                StartCoroutine(MoveToPosition(targetPos));
            }
            else
            {
                // �����ɏՓ˂����ꍇ
                GameObject hitObject = hit.collider.gameObject;

                // MoveBlock �̃��C���[�ԍ����擾�iMoveBlock���C���[������O��j
                int moveBlockLayerIndex = LayerMask.NameToLayer("MoveBlock");

                // �Փ˂����I�u�W�F�N�g�̃��C���[�� "MoveBlock" �ƈ�v���邩�m�F
                if (hitObject.layer == moveBlockLayerIndex)
                {
                    // �Փ˂����̂������u���b�N�̏ꍇ�AMoveBlock�R���|�[�l���g��T��
                    MoveBlock blockToPush = hitObject.GetComponent<MoveBlock>();

                    if (blockToPush != null)
                    {
                        // �u���b�N���ړ������郁�\�b�h���Ăяo��
                        if (blockToPush.TryMove(dir))
                        {
                            // �u���b�N�̈ړ�������������A�v���C���[���ړ����J�n����
                            targetPos = transform.position + dir * moveUnit;
                            StartCoroutine(MoveToPosition(targetPos));
                        }
                        // �u���b�N�̈ړ������s�����ꍇ�́A�v���C���[����~����B
                    }
                }
                // �ǂȂǂ̓������Ȃ��I�u�W�F�N�g�̏ꍇ�A�v���C���[�͒�~����B
            }
        }
    }

    /// <summary>
    /// �v���C���[���^�[�Q�b�g�ʒu�܂Ŋ��炩�Ɉړ�������R���[�`���B
    /// �ړ������܂�isMoving�t���O��true�ɕۂB
    /// </summary>
    IEnumerator MoveToPosition(Vector3 target)
    {
        isMoving = true;

        while ((transform.position - target).sqrMagnitude > 0.001f)
        {
            transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
            yield return null; // 1�t���[���ҋ@
        }

        // �덷�C��: �ŏI�ʒu���^�[�Q�b�g�ɌŒ�
        transform.position = target;
        isMoving = false;
    }
}