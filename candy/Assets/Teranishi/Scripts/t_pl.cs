using UnityEngine;
using UnityEngine.InputSystem;

public class t_pl : MonoBehaviour
{
    private Animator animator;
    // lastDirection��int�i�C���f�b�N�X�j�ŕێ�
    public int lastDirectionIndex = 1; // �f�t�H���g��1�i���j

    void Awake()
    {
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError("Player�I�u�W�F�N�g�� Animator �R���|�[�l���g��������܂���B");
        }
    }

    void Start()
    {
        // SceneDataTransfer�����݂��A���ۑ����ꂽ�����̃C���f�b�N�X������ꍇ�i0�ł͂Ȃ��ꍇ�j
        if (SceneDataTransfer.Instance != null && SceneDataTransfer.Instance.playerDirectionIndexToLoad != 0)
        {
            // ������ �����f�[�^�𑦍��Ƀ��[�h���A�A�j���[�^�[���X�V ������

            // 1. lastDirectionIndex�����[�h�f�[�^�ŏ㏑��
            lastDirectionIndex = SceneDataTransfer.Instance.playerDirectionIndexToLoad;

            // 2. Animator���X�V�i����ɂ��A�����ڂ������ɕς��j
            UpdateAnimator(lastDirectionIndex);

            // �⑫: TimeTravelController�̕���������҂K�v���Ȃ��Ȃ�܂����A
            // TimeTravelController���̕��������͔O�̂��߂��̂܂܎c���Ă����܂��B
        }
        else
        {
            // SceneDataTransfer���Ȃ��A�܂��̓��[�h�f�[�^���Ȃ��ꍇ�́A
            // �f�t�H���g�� lastDirectionIndex (1:��) ��Animator��������
            UpdateAnimator(lastDirectionIndex);
        }
    }

    void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        int newIndex = 0;

        // t_player�X�N���v�g�̎Q�Ƃ��L�����m�F�i�O�̂��߁j
        t_player playerMovementScript = GetComponent<t_player>();
        bool isMoving = playerMovementScript != null && playerMovementScript.IsPlayerMoving;


        // �L�[���͂ɉ����ăC���f�b�N�X������
        if (keyboard.downArrowKey.isPressed)
        {
            newIndex = 1; // �� = 1
        }
        else if (keyboard.upArrowKey.isPressed)
        {
            newIndex = 2; // �� = 2
        }
        else if (keyboard.rightArrowKey.isPressed)
        {
            newIndex = 3; // �E = 3
        }
        else if (keyboard.leftArrowKey.isPressed)
        {
            newIndex = 4; // �� = 4
        }

        // ���d�v: �ړ������A�V�������������o���ꂽ�ꍇ�ɂ̂�lastDirectionIndex���X�V����
        if (newIndex != 0)
        {
            SetDirection(newIndex);
        }

        // ���A�j���[�^�[�̍X�V�͖��t���[���s��
        //   ����ɂ��AAnimator�����̃X�e�[�g�ɐ؂�ւ������A
        //   ���̃X�e�[�g�i��: Idle_Right�j���ێ����邽�߂�Direction=3�𑗂葱���܂��B
        UpdateAnimator(lastDirectionIndex);

        // �� �]���� else �u���b�N�͕s�v�ɂȂ�܂����B
    }
    void SetDirection(int index)
    {
        lastDirectionIndex = index;
        UpdateAnimator(index);
    }

    // Animator��Int�p�����[�^�[���X�V���郁�\�b�h
    void UpdateAnimator(int index)
    {
        if (animator == null) return;

        // Int�p�����[�^�["Direction"��ݒ�
        animator.SetInteger("Direction", index);
    }

    // �O����������𕜌����邽�߂�Public���\�b�h (TimeTravelController����Ă΂��)
    public void LoadDirectionIndex(int index)
    {
        SetDirection(index);
    }

    // ���݂̌����̃C���f�b�N�X���擾����Public�v���p�e�B
    public int CurrentDirectionIndex
    {
        get { return lastDirectionIndex; }
    }
}