using UnityEngine;
using UnityEngine.InputSystem;

public class t_pl : MonoBehaviour
{
    // ��SpriteRenderer��Sprite��public�t�B�[���h�͕s�v�ł��i�蓮�ō폜�����j
    // �������A����t_pl�̃C���X�y�N�^�[��SpriteRenderer���c���Ă��Ă��A���̃R�[�h�̓G���[�ɂȂ�܂���B

    private Animator animator; // ��Animator�ւ̎Q��
    public Vector2 lastDirection = Vector2.down; // �Ō�Ɍ��Ă�������

    void Awake()
    {
        // Animator�R���|�[�l���g���擾
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError("Player�I�u�W�F�N�g�� Animator �R���|�[�l���g��������܂���B");
        }
    }

    void Start()
    {
        // Start�ł�Animator�Ɍ��݂̌�����ݒ肷��
        // �����f�[�^������ꍇ��TimeTravelController�����LoadDirection�ɔC���邽�߁A�����ł̓f�t�H���g�ݒ�̂�
        if (SceneDataTransfer.Instance == null || SceneDataTransfer.Instance.playerDirectionToLoad == Vector2.zero)
        {
            UpdateAnimator(lastDirection);
        }
    }

    void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        Vector2 inputDir = Vector2.zero;

        // �L�[��������Ă��邩���`�F�b�N���A���͕���������
        // wasPressedThisFrame (�u��) �� isPressed (�������ςȂ�) �̗����ɑΉ�
        if (keyboard.upArrowKey.isPressed)
        {
            inputDir = Vector2.up;
        }
        else if (keyboard.downArrowKey.isPressed)
        {
            inputDir = Vector2.down;
        }
        else if (keyboard.leftArrowKey.isPressed)
        {
            inputDir = Vector2.left;
        }
        else if (keyboard.rightArrowKey.isPressed)
        {
            inputDir = Vector2.right;
        }

        if (inputDir != Vector2.zero)
        {
            SetDirection(inputDir);
        }
        else
        {
            // �L�[���͂��Ȃ��ꍇ�ł��AAnimator���X�V���A�Ō�̌������ێ�������
            UpdateAnimator(lastDirection);
        }
    }

    void SetDirection(Vector2 dir)
    {
        lastDirection = dir;
        UpdateAnimator(dir);
    }

    // Animator�̃p�����[�^�[���X�V���郁�\�b�h
    void UpdateAnimator(Vector2 dir)
    {
        if (animator == null) return;

        // Animator��MoveX��MoveY�Ɍ��݂̌����̐�����ݒ肵�A�摜��؂�ւ���
        // Animator Controller�Œ�`�����p�����[�^�[��("MoveX", "MoveY")�ƈ�v������K�v������܂�
        animator.SetFloat("MoveX", dir.x);
        animator.SetFloat("MoveY", dir.y);
    }

    // �O����������𕜌����邽�߂�Public���\�b�h (TimeTravelController����Ă΂��)
    public void LoadDirection(Vector2 direction)
    {
        // ������ݒ肵�A������Animator�ɔ��f������
        SetDirection(direction);
    }

    // ���݂̌������擾����Public�v���p�e�B
    public Vector2 CurrentDirection
    {
        get { return lastDirection; }
    }
}