using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Linq;

public class PlayerController : MonoBehaviour
{
    // --- �v���C���[�ŗL�̒萔�ƃR���|�[�l���g ---
    private Animator _animator;
    private BoxCollider2D playerCollider;
    private const string DirectionParam = "Direction"; // Animator��Int�p�����[�^�[��

    // --- �p�����[�^�ݒ� (Inspector�Őݒ�) ---
    public float moveUnit = 1.0f;        // 1��̈ړ��Ői�ދ����i1�}�X���j
    public float moveSpeed = 5f;         // �ړ��X�s�[�h
    public LayerMask obstacleLayer;      // �Փ˔�����s�����C���[�i�ǁA�u���b�N�Ȃǁj

    // --- ��ԊǗ� ---
    [SerializeField]
    private bool isMoving = false;
    private Vector3 targetPos;

    // ���݂̌��� (1:��, 2:��, 3:�E, 4:��)
    public int CurrentDirectionIndex => lastDirectionIndex;
    private int lastDirectionIndex = 1;

    // �Ō�ɉ����ꂽ�L�[�ƃ^�C���X�^���v���i�[���鎫�� (�Ō�ɉ����ꂽ�L�[�D�惍�W�b�N�p)
    private Dictionary<int, float> lastKeyPressTime = new Dictionary<int, float>();

    // TimeTravelController���A�N�Z�X���邽�߂̌��J�v���p�e�B
    public Vector3 CurrentTargetPosition
    {
        get { return targetPos; }
    }
    public bool IsPlayerMoving
    {
        get { return isMoving; }
    }

    // --- Unity���C�t�T�C�N�� ---

    void Awake()
    {
        // �R���|�[�l���g�̎擾
        _animator = GetComponent<Animator>();
        playerCollider = GetComponent<BoxCollider2D>();

        // �K�{�R���|�[�l���g�̃`�F�b�N
        if (_animator == null) Debug.LogError("Animator�R���|�[�l���g��������܂���B");
        if (playerCollider == null) Debug.LogError("BoxCollider2D��������܂���B");

        // �����̏�����
        lastKeyPressTime.Add(1, 0f); // ��
        lastKeyPressTime.Add(2, 0f); // ��
        lastKeyPressTime.Add(3, 0f); // �E
        lastKeyPressTime.Add(4, 0f); // ��

        // --- �V�[���؂�ւ����̈ʒu�ƌ����̃��[�h���� ---
        if (SceneDataTransfer.Instance != null)
        {
            // 1. �ʒu�̃��[�h
            Vector3 loadPosition = SceneDataTransfer.Instance.playerPositionToLoad;
            if (loadPosition != Vector3.zero)
            {
                transform.position = loadPosition;
                targetPos = loadPosition;
            }
            else
            {
                targetPos = transform.position;
            }

            // 2. �����̃��[�h
            int loadDirection = SceneDataTransfer.Instance.playerDirectionIndexToLoad;
            if (loadDirection != 0)
            {
                lastDirectionIndex = loadDirection;
                UpdateAnimator(lastDirectionIndex);
            }
            else
            {
                UpdateAnimator(lastDirectionIndex); // �f�t�H���g�����ŏ�����
            }
        }
        else
        {
            targetPos = transform.position;
            UpdateAnimator(lastDirectionIndex); // �f�t�H���g�����ŏ�����
        }
    }

    void Update()
    {
        if (Keyboard.current == null || _animator == null) return;

        // 1. �A�j���[�V���������̍X�V (�L�[��������Ă���ԁA�ŐV�̃L�[��ǐ�)
        UpdateDirection();

        // 2. �ړ����͐V�������͂��󂯕t���Ȃ�
        if (isMoving) return;

        // 3. �ړ��g���K�[�̔��� (�������h�~�̂��� wasPressedThisFrame ���g�p)
        bool keyWasPressed = Keyboard.current.rightArrowKey.wasPressedThisFrame ||
                             Keyboard.current.leftArrowKey.wasPressedThisFrame ||
                             Keyboard.current.upArrowKey.wasPressedThisFrame ||
                             Keyboard.current.downArrowKey.wasPressedThisFrame;

        // �L�[�������ꂽ�u�Ԃł͂Ȃ��ꍇ�A�ړ��͊J�n���Ȃ�
        if (!keyWasPressed) return;

        // 4. �A�j���[�V���������Ɋ�Â��Ĉړ�����������
        Vector3 dir = ConvertDirectionIndexToVector(lastDirectionIndex);

        if (dir != Vector3.zero)
        {
            TryMove(dir);
        }
    }

    // --- �v���C�x�[�g���\�b�h�F�����̍X�V�ƃA�j���[�V�������� ---

    private void UpdateDirection()
    {
        var keyboard = Keyboard.current;
        List<int> pressedDirections = new List<int>();

        // ������Ă���L�[�̃^�C���X�^���v���X�V
        if (keyboard.downArrowKey.isPressed) { lastKeyPressTime[1] = Time.time; pressedDirections.Add(1); }
        if (keyboard.upArrowKey.isPressed) { lastKeyPressTime[2] = Time.time; pressedDirections.Add(2); }
        if (keyboard.rightArrowKey.isPressed) { lastKeyPressTime[3] = Time.time; pressedDirections.Add(3); }
        if (keyboard.leftArrowKey.isPressed) { lastKeyPressTime[4] = Time.time; pressedDirections.Add(4); }

        // �Ō�ɉ����ꂽ�L�[����肵�A�������X�V
        if (pressedDirections.Count > 0)
        {
            int preferredIndex = 0;
            float latestTime = -1f;

            foreach (int index in pressedDirections)
            {
                if (lastKeyPressTime[index] > latestTime)
                {
                    latestTime = lastKeyPressTime[index];
                    preferredIndex = index;
                }
            }

            SetDirection(preferredIndex);
        }

        // ��Ɍ��݂̌�����Animator���X�V
        UpdateAnimator(lastDirectionIndex);
    }

    private void SetDirection(int newIndex)
    {
        if (newIndex != 0)
        {
            lastDirectionIndex = newIndex;
        }
    }

    private void UpdateAnimator(int directionIndex)
    {
        if (_animator != null)
        {
            _animator.SetInteger(DirectionParam, directionIndex);
        }
    }

    // �����̃C���f�b�N�X��Vector3�ɕϊ�����w���p�[�֐�
    private Vector3 ConvertDirectionIndexToVector(int index)
    {
        switch (index)
        {
            case 1: return Vector3.down;
            case 2: return Vector3.up;
            case 3: return Vector3.right;
            case 4: return Vector3.left;
            default: return Vector3.zero;
        }
    }

    // --- �v���C�x�[�g���\�b�h�F�ړ��ƏՓ˔��� ---

    private void TryMove(Vector3 dir)
    {
        // BoxCast�̔���ɕK�v�ȏ���Collider����擾
        Vector2 origin = (Vector2)transform.position + playerCollider.offset;
        Vector2 size = playerCollider.size;
        float angle = 0f;
        float checkDistance = moveUnit * 1.01f;

        RaycastHit2D hit = Physics2D.BoxCast(origin, size, angle, dir, checkDistance, obstacleLayer);

        // �Փ˔���ƃu���b�N��������
        if (hit.collider == null)
        {
            // �Փ˂Ȃ�: �ړ������s
            targetPos = transform.position + dir * moveUnit;
            StartCoroutine(MoveToPosition(targetPos));
        }
        else
        {
            // �Փ˂���: �u���b�N�����̃`�F�b�N
            GameObject hitObject = hit.collider.gameObject;
            int moveBlockLayerIndex = LayerMask.NameToLayer("MoveBlock");

            if (hitObject.layer == moveBlockLayerIndex)
            {
                MoveBlock blockToPush = hitObject.GetComponent<MoveBlock>();

                if (blockToPush != null)
                {
                    if (blockToPush.TryMove(dir))
                    {
                        // �u���b�N�̈ړ�������������A�v���C���[���ړ����J�n����
                        targetPos = transform.position + dir * moveUnit;
                        StartCoroutine(MoveToPosition(targetPos));
                    }
                }
            }
        }
    }

    // �v���C���[���^�[�Q�b�g�̈ʒu�܂Ŋ��炩�Ɉړ�������R���[�`��
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

    // �V�[���؂�ւ����̌��������p�iTimeTravelController����Ă΂��j
    public void LoadDirectionIndex(int index)
    {
        if (index != 0)
        {
            lastDirectionIndex = index;
            UpdateAnimator(lastDirectionIndex);
        }
    }
}