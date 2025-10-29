using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// �v���C���[�̃A�j���[�V�����ƌ������Ǘ�����X�N���v�g�B
/// ����: �����̌v�Z���W�b�N��t_player.cs�ɔC���āA�A�j���[�V�����\���ƃV�[���܂����̌����ێ��ɐ�O����B
/// </summary>
public class t_pl : MonoBehaviour
{
    // --- �萔�ƃR���|�[�l���g ---
    private Animator _animator;
    private const string DirectionParam = "Direction"; // Animator��Int�p�����[�^�[��

    // --- ������� ---
    // �����̃C���f�b�N�X (1:��, 2:��, 3:�E, 4:��)�B�����l�́u���v
    private int lastDirectionIndex = 1;

    // t_player.cs�����̒l��ǂ�Ŏg���B�ŐV�̌�����n���v���p�e�B
    public int CurrentDirectionIndex => lastDirectionIndex;

    // --- Unity���C�t�T�C�N�� ---

    void Awake()
    {
        _animator = GetComponent<Animator>();
        if (_animator == null)
        {
            Debug.LogError("[t_pl] Animator�R���|�[�l���g���Ȃ�");
        }
    }

    void Start()
    {
        // �V�[�����[�h���̌�����������
        // SceneDataTransfer����ۑ����ꂽ���������[�h����
        if (SceneDataTransfer.Instance != null && SceneDataTransfer.Instance.playerDirectionIndexToLoad != 0)
        {
            lastDirectionIndex = SceneDataTransfer.Instance.playerDirectionIndexToLoad;
            UpdateAnimator(lastDirectionIndex);
        }
        else
        {
            // ���[�h�f�[�^���Ȃ��ꍇ�͏����l�i��=1�j�ŃA�j���[�^�[��ݒ肷��
            UpdateAnimator(lastDirectionIndex);
        }
    }

    /// <summary>
    /// t_player�Ɉړ����W�b�N���Ϗ������̂ŁA����Update�͊�{�I�ɋ�ɂ���B
    /// </summary>
    void Update()
    {
        // ���͏�����t_player�ōs�����߁A�����ł͉������Ȃ�
    }

    // --- �O���A�g�p���\�b�h ---

    /// <summary>
    /// t_player.cs����Ă΂�A�v�Z���ꂽ�ŐV�̌������Z�b�g���A�A�j���[�^�[���X�V����B
    /// �� t_player.cs�Ƃ̘A�g�ɕK�{�̃��\�b�h
    /// </summary>
    /// <param name="newIndex">t_player�����肵���V���������C���f�b�N�X</param>
    public void SetDirectionFromExternal(int newIndex)
    {
        if (newIndex != 0) // �������L���ȏꍇ�̂ݏ�������
        {
            // �������ς������l���X�V����
            if (newIndex != lastDirectionIndex)
            {
                lastDirectionIndex = newIndex;
            }
            // �A�j���[�^�[���ŐV�̌����ōX�V����
            UpdateAnimator(lastDirectionIndex);
        }
    }

    /// <summary>
    /// TimeTravelController�Ȃǂ���Ă΂��B�V�[�����[�h���^�C���g���x�����Ɍ����𕜌�����B
    /// �� TimeTravelController�Ƃ̘A�g�ɕK�{�̃��\�b�h
    /// </summary>
    public void LoadDirectionIndex(int index)
    {
        if (index != 0)
        {
            lastDirectionIndex = index;
            // �A�j���[�^�[���X�V����
            UpdateAnimator(lastDirectionIndex);
        }
    }

    // --- �v���C�x�[�g���\�b�h ---

    private void UpdateAnimator(int directionIndex)
    {
        if (_animator != null)
        {
            // �A�j���[�^�[��Direction�p�����[�^�[���X�V����
            _animator.SetInteger(DirectionParam, directionIndex);
        }
    }
}