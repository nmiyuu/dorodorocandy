using UnityEngine;
using System.Collections;

public class t_player : MonoBehaviour
{
    // --- �p�����[�^�ݒ� (Inspector�Őݒ�) ---
    public float moveUnit = 1.0f;       // 1��̈ړ��Ői�ދ����i1�}�X���j
    public float moveSpeed = 5f;        // �ړ��X�s�[�h
    public LayerMask obstacleLayer;     // �Փ˔�����s�����C���[�i�ǁA�u���b�N�Ȃǁj

    // --- ������� ---
    [SerializeField]
    private bool isMoving = false;
    private Vector3 targetPos;
    private BoxCollider2D playerCollider;
    // TimeTravelController���A�N�Z�X���邽�߂̌��J�v���p�e�B
    public Vector3 CurrentTargetPosition
    {
        get { return targetPos; }
    }

    // �v���C���[���ړ������ǂ������O������m�F���邽�߂̃v���p�e�B
    public bool IsPlayerMoving
    {
        get { return isMoving; }
    }

    // Start()�����������s�����Awake()�ŏ������ƈʒu���[�h���s��
    void Awake()
    {
        // �K�{�R���|�[�l���g�̎擾
        playerCollider = GetComponent<BoxCollider2D>();
        if (playerCollider == null)
        {
            Debug.LogError("Player�I�u�W�F�N�g��BoxCollider2D��������܂���I");
            return;
        }

        // --- �V�[���؂�ւ����̈ʒu���[�h���� ---
        if (SceneDataTransfer.Instance != null)
        {
            Vector3 loadPosition = SceneDataTransfer.Instance.playerPositionToLoad;

            // Vector3.zero �ȊO�i�f�[�^���ۑ�����Ă����ꍇ�j�݈̂ʒu�����[�h����
            if (loadPosition != Vector3.zero)
            {
                transform.position = loadPosition;
                targetPos = loadPosition;
                return;
            }
        }

        // ���[�h����Ȃ������ꍇ�i����N�����Ȃǁj�́A���݂�Hierarchy��̈ʒu���^�[�Q�b�g�ɐݒ�
        targetPos = transform.position;
    }

    void Start()
    {
        // Awake()�łقƂ�ǂ̏��������I���Ă��邽�߁AStart()�͋�Ŗ�肠��܂���B
    }

    void Update()
    {
        // �ړ����͐V�������͂��󂯕t���Ȃ�
        if (isMoving) return;

        Vector3 dir = Vector3.zero;

        // --- 1. ���͌��m ---
        // �����ꂽ�L�[�ɉ����Ĉړ�����������i�΂߈ړ��Ȃ��j
        if (Input.GetKeyDown(KeyCode.RightArrow)) dir = Vector3.right;
        else if (Input.GetKeyDown(KeyCode.LeftArrow)) dir = Vector3.left;
        else if (Input.GetKeyDown(KeyCode.UpArrow)) dir = Vector3.up;
        else if (Input.GetKeyDown(KeyCode.DownArrow)) dir = Vector3.down;

        // �������͂��������ꍇ�̂ݏ���
        if (dir != Vector3.zero)
        {
            // --- 2. BoxCast�ɂ��ǂ̎��O�`�F�b�N ---

            // BoxCast�̔���ɕK�v�ȏ���Collider����擾
            Vector2 origin = (Vector2)transform.position + playerCollider.offset;
            Vector2 size = playerCollider.size;
            float angle = 0f;

            // �`�F�b�N����: �ړ����� (moveUnit) ���킸���ɒ����ݒ�
            float checkDistance = moveUnit * 1.01f;

            // Physics2D.BoxCast�����s: �v���C���[�̈ړ���ɉ����i�ǂ܂��̓u���b�N�j�����邩���m�F
            RaycastHit2D hit = Physics2D.BoxCast(origin, size, angle, dir, checkDistance, obstacleLayer);

            
            //�Փ˔���ƃu���b�N��������

            // �Փ˂��Ȃ������ꍇ (�ړ���ɉ����Ȃ�)
            if (hit.collider == null)
            {
                // �ړ������s
                targetPos = transform.position + dir * moveUnit;
                StartCoroutine(MoveToPosition(targetPos));
            }
            else
            {
                // �����ɏՓ˂����ꍇ
                GameObject hitObject = hit.collider.gameObject;

                // MoveBlock �̃��C���[�ԍ����擾
                int moveBlockLayerIndex = LayerMask.NameToLayer("MoveBlock");

                // �Փ˂����I�u�W�F�N�g�̃��C���[�� "MoveBlock" �ƈ�v���邩�m�F
                if (hitObject.layer == moveBlockLayerIndex)
                {
                    // �Փ˂����̂������u���b�N�̏ꍇ�AMoveBlock�R���|�[�l���g��T��
                    // �iMoveBlock.cs�Ƃ������O�̃X�N���v�g������O��j
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
                        // �u���b�N�̈ړ������s�����ꍇ�́A�v���C���[����~�B
                    }
                }
                // ���������̂�MoveBlock�ł͂Ȃ��i�����Ȃ��ǂȂǁj�ꍇ�́A�v���C���[�͒�~�B
            }
        }
    }

    // �v���C���[���^�[�Q�b�g�̈ʒu�܂Ŋ��炩�Ɉړ�������R���[�`��
    IEnumerator MoveToPosition(Vector3 target)
    {
        isMoving = true;

        // �^�[�Q�b�g�ɋ߂Â��܂Ń��[�v
        while ((transform.position - target).sqrMagnitude > 0.001f)
        {
            // �^�[�Q�b�g�Ɍ����Ĉړ� (Time.deltaTime���g�p)
            transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
            yield return null; // 1�t���[���ҋ@
        }

        // �Ō�̎d�グ: �덷���Ȃ������߁A���m�ȃ^�[�Q�b�g�ʒu�ɌŒ肷��
        transform.position = target;
        isMoving = false;
    }
}