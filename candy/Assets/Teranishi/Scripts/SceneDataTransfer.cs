using UnityEngine;
using System.Collections.Generic;
using System.Linq; // FindObjectsByType��Linq���g�����ɕK�v

// �u���b�N�̏�Ԃ�ۑ����邽�߂̍\����
[System.Serializable]
public struct BlockState
{
    public string id;
    public Vector3 finalPosition;
}

/// �V�[�����܂����Ńf�[�^�������z���V���O���g���N���X�B
/// DontDestroyOnLoad�ŃV�[���؂�ւ���������Ȃ��悤�ɂ���B
public class SceneDataTransfer : MonoBehaviour
{
    // --- �V���O���g���C���X�^���X ---
    public static SceneDataTransfer Instance;

    // --- �V�[�����܂����ň����p�������f�[�^ ---

    // �v���C���[�̕��A�ʒu
    public Vector3 playerPositionToLoad = Vector3.zero;

    // �v���C���[�̌�����ۑ�����ϐ� (Int�^�C���f�b�N�X: ��=1, ��=2, �E=3, ��=4)
    public int playerDirectionIndexToLoad = 1; // �����l��1�i���j

    // �ߋ��V�[���œ������ꂽ�u���b�N�̈ʒu��ۑ����郊�X�g
    public List<BlockState> pastBlockStates = new List<BlockState>();

    void Awake()
    {
        // �V���O���g���p�^�[���̊m��
        if (Instance == null)
        {
            Instance = this;
            // �����Ǝc��
            DontDestroyOnLoad(gameObject);
        }
        else
        {
           
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// R�L�[���Z�b�g�ȂǂŌĂ΂��B�ۑ�����Ă��邷�ׂẴf�[�^��������Ԃɖ߂��B
    /// ����ŉߋ�/�����̕ۑ��f�[�^���N���A�ɂȂ�B
    /// </summary>
    public void FullReset()
    {
        // �v���C���[�̈ʒu�ƌ����������l�ɖ߂�
        playerPositionToLoad = Vector3.zero;
        playerDirectionIndexToLoad = 1; // �����̏����l�i���j

        // �u���b�N�̏�ԃ��X�g���N���A����
        pastBlockStates.Clear();

        Debug.Log("[SceneDataTransfer] �f�[�^�Ɖߋ�/�����̏�Ԃ����S���Z�b�g����");
    }

    // �����u���b�N�̍ŏI�ʒu��ۑ�����֐�
    public void SaveBlockPositions()
    {
        pastBlockStates.Clear();

        // ���݂̃V�[���ɂ��� MoveBlock �����ׂČ���
        MoveBlock[] currentBlocks = FindObjectsByType<MoveBlock>(FindObjectsSortMode.None);

        foreach (MoveBlock block in currentBlocks)
        {
            // blockID���ݒ肳��Ă���u���b�N�݂̂�����
            if (!string.IsNullOrEmpty(block.blockID))
            {
                BlockState state = new BlockState
                {
                    id = block.blockID,
                    // �ʒu���}�X�ڂ̒��S�Ɋۂ߂�
                    finalPosition = new Vector3(
                        Mathf.Round(block.transform.position.x),
                        Mathf.Round(block.transform.position.y),
                        Mathf.Round(block.transform.position.z)
                    )
                };
                pastBlockStates.Add(state);
            }
        }
        Debug.Log("�����u���b�N�̈ʒu�� " + pastBlockStates.Count + " �ۑ�����");
    }
}