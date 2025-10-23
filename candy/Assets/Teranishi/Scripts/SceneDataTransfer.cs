using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Linq;

// �u���b�N�̏�Ԃ�ۑ����邽�߂̍\���� (�ύX�Ȃ�)
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
    // �v���C���[�̕��A�ʒu (����)
    public Vector3 playerPositionToLoad = Vector3.zero;

    // ���ǉ�: �v���C���[�̌�����ۑ�����ϐ�
    public Vector2 playerDirectionToLoad = Vector2.down; // �����l�͐���

    // �ߋ��V�[���œ������ꂽ�u���b�N�̈ʒu��ۑ����郊�X�g (����)
    public List<BlockState> pastBlockStates = new List<BlockState>();

    void Awake()
    {
        // �V���O���g���p�^�[���̊m�� (����)
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

    // �����u���b�N�̍ŏI�ʒu��ۑ�����֐� (����)
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
        Debug.Log("�����u���b�N�̈ʒu�� " + pastBlockStates.Count + " �ۑ����܂����B");
    }
}