using UnityEngine;
using System.Collections.Generic; // List���g�����߂ɕK�v
using UnityEngine.SceneManagement; // DontDestroyOnLoad���g�����߂�SceneManagement���C���|�[�g�i�����ɂ͕s�v�ł����O�̂��߁j

// �u���b�N�̏�Ԃ�ۑ����邽�߂̍\���̂��O���ɒ�`�i��������SceneDataTransfer�N���X�̓����ł��j
[System.Serializable]
public struct BlockState
{
    public string id;
    public Vector3 finalPosition;
}

public class SceneDataTransfer : MonoBehaviour
{
    // --- �V���O���g���C���X�^���X ---
    public static SceneDataTransfer Instance;

    // --- �V�[�����܂����ň����p�������f�[�^ ---
    public Vector3 playerPositionToLoad = Vector3.zero;
    public List<BlockState> pastBlockStates = new List<BlockState>();

    void Awake()
    {
        // �V���O���g���p�^�[���̊m��
        if (Instance == null)
        {
            Instance = this;
            // ���̃I�u�W�F�N�g���V�[���؂�ւ����ɔj������Ȃ��悤�ɂ���i�i�����j
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // ���ɃC���X�^���X������΁A�V�������͔j������
            Destroy(gameObject);
        }
    }

    // �ߋ��̃u���b�N�̍ŏI�ʒu��ۑ�����֐�
    public void SaveBlockPositions()
    {
        pastBlockStates.Clear();

        // ������ �C�������FFindObjectsOfType �� FindObjectsByType �ɕύX ������
        // FindObjectsSortMode.None ���g�����ƂŁA��荂���ɃI�u�W�F�N�g���擾�ł��܂��B
        MoveBlock[] pastBlocks = FindObjectsByType<MoveBlock>(FindObjectsSortMode.None);
        // ������ �C������ �����܂� ������

        foreach (MoveBlock block in pastBlocks)
        {
            // blockID���ݒ肳��Ă��邩�m�F
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
        Debug.Log("�ߋ��̃u���b�N�ʒu�� " + pastBlockStates.Count + " �ۑ����܂����B");
    }
}
