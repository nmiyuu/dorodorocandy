using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;
using System.Collections.Generic;

/// <summary>
/// �v���C���[�̃A�j���[�V�����ƌ������Ǘ�����X�N���v�g���B
/// ����: �����L�[���������̏ꍇ�A�u�Ō�ɉ����ꂽ�L�[�v�̌������A�j���[�V�����ɔ��f������B
/// </summary>
public class t_pl : MonoBehaviour
{
    // --- �萔�ƃR���|�[�l���g ---
    private Animator _animator;
    private const string DirectionParam = "Direction"; // Animator��Int�p�����[�^�[��

    // --- ������� ---
    // �����̃C���f�b�N�X (1:��, 2:��, 3:�E, 4:��)
    private int lastDirectionIndex = 1;

    // �Ō�ɉ����ꂽ�L�[�ƃ^�C���X�^���v���i�[���鎫�� (InputSystem�ˑ�)
    private Dictionary<int, float> lastKeyPressTime = new Dictionary<int, float>();

    // --- ���J�v���p�e�B (�O���A�g�p) ---
    // t_player.cs�����̃v���p�e�B���Q�Ƃ��邱�ƂŁA�ړ������ƃA�j���[�V���������𓯊������邱�Ƃ��\���B
    public int CurrentDirectionIndex => lastDirectionIndex;

    // --- Unity���C�t�T�C�N�� ---

    /// <summary>
    /// �����������B�R���|�[�l���g�擾�Ǝ���������������B
    /// </summary>
    void Awake()
    {
        _animator = GetComponent<Animator>();
        if (_animator == null)
        {
            Debug.LogError("[t_pl] Animator�R���|�[�l���g��������Ȃ��B", this);
        }

        // �����ɕ����C���f�b�N�X�������ݒ�
        lastKeyPressTime.Add(1, 0f); // ��
        lastKeyPressTime.Add(2, 0f); // ��
        lastKeyPressTime.Add(3, 0f); // �E
        lastKeyPressTime.Add(4, 0f); // ��
    }

    /// <summary>
    /// �V�[�����[�h���̌������������B
    /// </summary>
    void Start()
    {
        // �V�[���f�[�^�]���N���X����A�ۑ����ꂽ�����C���f�b�N�X�����[�h����
        if (SceneDataTransfer.Instance != null && SceneDataTransfer.Instance.playerDirectionIndexToLoad != 0)
        {
            lastDirectionIndex = SceneDataTransfer.Instance.playerDirectionIndexToLoad;
            UpdateAnimator(lastDirectionIndex);
        }
        else
        {
            // �f�[�^���Ȃ��ꍇ��Awake�Őݒ肳�ꂽ�f�t�H���g�l���g�p����
            UpdateAnimator(lastDirectionIndex);
        }
    }

    /// <summary>
    /// ���t���[���̌����v�Z�ƃA�j���[�V�����X�V�����B
    /// </summary>
    void Update()
    {
        if (_animator == null) return;
        var keyboard = Keyboard.current; // InputSystem�Ɉˑ�
        if (keyboard == null) return;

        // 1. ������Ă���L�[���`�F�b�N���A�^�C���X�^���v���X�V����
        List<int> pressedDirections = new List<int>();

        // �L�[��������Ă���� (isPressed) �́A��Ƀ^�C���X�^���v���ŐV�ɍX�V����
        if (keyboard.downArrowKey.isPressed) { lastKeyPressTime[1] = Time.time; pressedDirections.Add(1); }
        if (keyboard.upArrowKey.isPressed) { lastKeyPressTime[2] = Time.time; pressedDirections.Add(2); }
        if (keyboard.rightArrowKey.isPressed) { lastKeyPressTime[3] = Time.time; pressedDirections.Add(3); }
        if (keyboard.leftArrowKey.isPressed) { lastKeyPressTime[4] = Time.time; pressedDirections.Add(4); }

        // 2. ������Ă���L�[�̒�����A�Ō�ɉ����ꂽ�L�[����肷��
        if (pressedDirections.Count > 0)
        {
            int preferredIndex = 0;
            float latestTime = -1f;

            foreach (int index in pressedDirections)
            {
                // �Ō�ɉ����ꂽ�����i�ŐV�̎����j��D��
                if (lastKeyPressTime[index] > latestTime)
                {
                    latestTime = lastKeyPressTime[index];
                    preferredIndex = index;
                }
            }

            // �Ō�ɉ����ꂽ�L�[�̌����ŃA�j���[�V�������X�V����
            SetDirection(preferredIndex);
        }

        // 3. �A�j���[�^�[�Ɍ��݂̌����𔽉f����
        UpdateAnimator(lastDirectionIndex);
    }

    // --- �v���C�x�[�g���\�b�h ---

    /// <summary>
    /// �����̌����C���f�b�N�X���X�V����B
    /// </summary>
    private void SetDirection(int newIndex)
    {
        if (newIndex != 0)
        {
            lastDirectionIndex = newIndex;
        }
    }

    /// <summary>
    /// Animator��Direction�p�����[�^�[���X�V����B
    /// </summary>
    private void UpdateAnimator(int directionIndex)
    {
        if (_animator != null)
        {
            _animator.SetInteger(DirectionParam, directionIndex);
        }
    }

    // --- �O���Ăяo���p���\�b�h ---

    /// <summary>
    /// TimeTravelController����̌Ăяo���p�B�V�[�����[�h��Ɍ����𕜌�����B
    /// </summary>
    public void LoadDirectionIndex(int index)
    {
        if (index != 0)
        {
            lastDirectionIndex = index;
            UpdateAnimator(lastDirectionIndex);
        }
    }
}