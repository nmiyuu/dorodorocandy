using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using UnityEngine.SceneManagement; // R�L�[���Z�b�g�̂��߂ɕK�v

// �v���C���[�̈ړ��ƏՓ˔����S�����R�A�X�N���v�g�B
// ����: �Ō�̃L�[��D�悷�郍�W�b�N�������Ŏ����߁At_pl�Ƃ̎��s�����ˑ����Ȃ����B
public class t_player : MonoBehaviour
{
    // --- �p�����[�^�ݒ� (Inspector�ݒ�p) ---
    public float moveUnit = 1.0f;        // 1�}�X�i�ދ���
    public float moveSpeed = 5f;         // �ړ��X�s�[�h
    public LayerMask obstacleLayer;      // �Ԃ���Ώۂ̃��C���[�i�ǂƂ��u���b�N�j

    // --- ������ԂƃR���|�[�l���g ---
    [SerializeField]
    private bool isMoving = false;       // �ړ����t���O
    private Vector3 targetPos;           // ���̖ړI�n
    private BoxCollider2D playerCollider;
    private t_pl playerAnimScript;      // �A�j���[�V�����S����t_pl�ւ̎Q��

    // �Ō�ɉ����ꂽ�L�[�Ǝ��Ԃ��L�^���鎫���i�L�[�D�攻��Ɏg���j
    private Dictionary<int, float> lastKeyPressTime = new Dictionary<int, float>();

    // R�L�[���Z�b�g�����̂��߂Ɍ��݂̃V�[������ێ�����
    private string currentSceneName;

    // --- ���J�v���p�e�B (�O���A�g�p) ---
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
        playerCollider = GetComponent<BoxCollider2D>();
        playerAnimScript = GetComponent<t_pl>();

        if (playerCollider == null) Debug.LogError("[t_player] BoxCollider2D���Ȃ�");
        if (playerAnimScript == null) Debug.LogError("[t_player] t_pl���Ȃ�");

        // �����̏������i�����C���f�b�N�X��o�^�j
        lastKeyPressTime.Add(1, 0f); // ��
        lastKeyPressTime.Add(2, 0f); // ��
        lastKeyPressTime.Add(3, 0f); // �E
        lastKeyPressTime.Add(4, 0f); // ��

        // --- �V�[���؂�ւ����̈ʒu���[�h���� (SceneDataTransfer�Ɉˑ�) ---
        if (SceneDataTransfer.Instance != null)
        {
            Vector3 loadPosition = SceneDataTransfer.Instance.playerPositionToLoad;
            if (loadPosition != Vector3.zero)
            {
                transform.position = loadPosition;
                targetPos = loadPosition;
            }
        }
        // ���[�h����Ȃ�������A���݂�Hierarchy��̈ʒu��ڕW�ɂ���
        if (targetPos == Vector3.zero) targetPos = transform.position;

        // ���݂̃V�[������Awake�Ŏ擾���Ă���
        currentSceneName = SceneManager.GetActiveScene().name;
    }

    // --- ���C������ ---

    void Update()
    {
        // R�L�[���S���Z�b�g����
        // R�L�[�������ꂽ��AFullSceneReset�����s����
        if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
        {
            FullSceneReset();
            return; // ���Z�b�g�����炻�̃t���[���̑��̏����͂��Ȃ�
        }

        if (playerAnimScript == null || Keyboard.current == null) return;

        // �A�j���[�V�����������ŐV�ɍX�V����
        // �ړ�����̑O�ɁA���̃t���[���łǂ̃L�[���Ō�ɉ����ꂽ���v�Z���At_pl�ɋ����ē���������
        int newDirectionIndex = CalculateNewDirection();
        playerAnimScript.SetDirectionFromExternal(newDirectionIndex);

        // �ړ����͓��͂��󂯕t���Ȃ�
        if (isMoving) return;

        // �ړ��g���K�[�̔��� (�������h�~�̂���wasPressedThisFrame���g��)
        bool keyWasPressed = Keyboard.current.upArrowKey.wasPressedThisFrame ||
                             Keyboard.current.downArrowKey.wasPressedThisFrame ||
                             Keyboard.current.leftArrowKey.wasPressedThisFrame ||
                             Keyboard.current.rightArrowKey.wasPressedThisFrame;

        if (!keyWasPressed) return;

        // �v�Z�����ŐV�̌����inewDirectionIndex�j�ňړ�����������
        Vector3 dir = ConvertDirectionIndexToVector(newDirectionIndex);

        // �Փ˔���ƈړ��̎��s
        if (dir != Vector3.zero)
        {
            Vector2 origin = (Vector2)transform.position + playerCollider.offset;
            Vector2 size = playerCollider.size;
            float angle = 0f;
            float checkDistance = moveUnit * 1.01f; // ���傢���߂Ƀ`�F�b�N

            RaycastHit2D hit = Physics2D.BoxCast(origin, size, angle, dir, checkDistance, obstacleLayer);

            if (hit.collider == null)
            {
                // �����Ȃ��Ȃ�ړ�����
                targetPos = transform.position + dir * moveUnit;
                StartCoroutine(MoveToPosition(targetPos));
            }
            else
            {
                // �����ɂԂ�������A���ꂪ�u���b�N���`�F�b�N����
                GameObject hitObject = hit.collider.gameObject;
                int moveBlockLayerIndex = LayerMask.NameToLayer("MoveBlock");

                if (hitObject.layer == moveBlockLayerIndex)
                {
                    MoveBlock blockToPush = hitObject.GetComponent<MoveBlock>();
                    if (blockToPush != null && blockToPush.TryMove(dir))
                    {
                        // �u���b�N�𓮂���������A�������ړ�����
                        targetPos = transform.position + dir * moveUnit;
                        StartCoroutine(MoveToPosition(targetPos));
                    }
                    // �u���b�N���������Ȃ�������A�������~�܂�
                }
                // �ǂƂ���������A���������~�܂�
            }
        }
    }

    // --- �v���C�x�[�g���\�b�h (���W�b�N��������) ---

    
    // ��������Ă���L�[�̒��ŁA��ԍŌ�ɉ����ꂽ�L�[�̌����C���f�b�N�X���v�Z���ĕԂ��B
  
    private int CalculateNewDirection()
    {
        var keyboard = Keyboard.current;
        List<int> pressedDirections = new List<int>();

        // ������Ă���L�[�̃^�C���X�^���v���X�V����
        if (keyboard.downArrowKey.isPressed) { lastKeyPressTime[1] = Time.time; pressedDirections.Add(1); }
        if (keyboard.upArrowKey.isPressed) { lastKeyPressTime[2] = Time.time; pressedDirections.Add(2); }
        if (keyboard.rightArrowKey.isPressed) { lastKeyPressTime[3] = Time.time; pressedDirections.Add(3); }
        if (keyboard.leftArrowKey.isPressed) { lastKeyPressTime[4] = Time.time; pressedDirections.Add(4); }

        if (pressedDirections.Count == 0)
        {
            // �L�[������������ĂȂ��Ȃ�At_pl�������Ă鍡�̌��������̂܂ܕԂ�
            return playerAnimScript.CurrentDirectionIndex;
        }

        // �Ō�ɉ����ꂽ�L�[����肷�郍�W�b�N
        int preferredIndex = 0;
        float latestTime = -1f;

        foreach (int index in pressedDirections)
        {
            // Time.time���傫�������ŐV�i�Ō�ɉ����ꂽ�L�[�j
            if (lastKeyPressTime[index] > latestTime)
            {
                latestTime = lastKeyPressTime[index];
                preferredIndex = index;
            }
        }
        return preferredIndex;
    }

  
    // �����̃C���f�b�N�X�i1�`4�j��Unity��Vector3�i�����x�N�g���j�ɕϊ�����֐��B
   
    private Vector3 ConvertDirectionIndexToVector(int index)
    {
        switch (index)
        {
            case 1: return Vector3.down;
            case 2: return Vector3.up;
            case 3: return Vector3.right;
            case 4: return Vector3.left;
            default: return Vector3.zero; // 0�Ȃ�ړ��Ȃ�
        }
    }

 
    // R�L�[�ŌĂ΂�銮�S���Z�b�g�@�\�B
    // �V���O���g�����̃f�[�^���N���A���A���݂̃V�[�����ă��[�h����B

    private void FullSceneReset()
    {
        // 1. �V���O���g���̃f�[�^�i�ߋ�/�����̏�ԁj�����Z�b�g����
        if (SceneDataTransfer.Instance != null)
        {
            // SceneDataTransfer.cs��FullReset�֐����Ăяo���ăf�[�^���N���A
            SceneDataTransfer.Instance.FullReset();
        }

        // 2. ���݂̃V�[�����ă��[�h����
        SceneManager.LoadScene(currentSceneName);

        Debug.Log($"�V�[�� '{currentSceneName}' ��R�L�[�Ŋ��S���Z�b�g����");
    }

    // --- �R���[�`�� ---

    IEnumerator MoveToPosition(Vector3 target)
    {
        isMoving = true;

        // �ړI�n�ɂقڒ����܂ňړ��𑱂���
        while ((transform.position - target).sqrMagnitude > 0.001f)
        {
            transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
            yield return null; // 1�t���[���҂�
        }

        // �Ō�ɖړI�n�Ƀs�^�b�ƍ��킹��
        transform.position = target;
        isMoving = false;
    }
}