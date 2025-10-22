using UnityEngine;
using System.Linq;

public class FutureObstacleController : MonoBehaviour
{
    // �ߋ���MoveBlock�ƑΉ����邽�߂�ID
    // �ߋ���MoveBlock��blockID�ƑS������ID��ݒ肷��K�v������܂�
    [Tooltip("�Ή�����ߋ���MoveBlock�Ɠ������j�[�NID��ݒ肵�Ă��������B")]
    public string blockID;

    void Start()
    {
        // 1. SceneDataTransfer�����݂��邩�m�F
        if (SceneDataTransfer.Instance == null)
        {
            Debug.LogError("SceneDataTransfer ��������܂���B");
            return;
        }

        // 2. �ߋ��̃u���b�N�̕ۑ��ʒu�� SceneDataTransfer ���猟��
        // pastBlockStates �͉ߋ��V�[���̃u���b�N���ړ�������̈ʒu��ێ����Ă��܂��B
        BlockState? savedState = SceneDataTransfer.Instance.pastBlockStates
            .Where(state => state.id == blockID)
            .Cast<BlockState?>()
            .FirstOrDefault();

        // 3. �ۑ��f�[�^�����݂���ꍇ�A�ʒu�������I�ɓ���
        if (savedState.HasValue)
        {
            Vector3 finalPosition = savedState.Value.finalPosition;

            // �ߋ��̃u���b�N����������̈ʒu�ɁA�����̓������Ȃ��u���b�N�������ړ�������
            transform.position = finalPosition;
            Debug.Log($"�����̐ÓI��Q�� '{blockID}' ���A�ߋ��̈ړ��ʒu {finalPosition} �ɓ������܂����B");
        }
        else
        {
            // �ߋ��̃u���b�N����x����������Ă��Ȃ��i�f�[�^���ۑ�����Ă��Ȃ��j�ꍇ�A
            // ���̖����̏�Q���̓V�[���ɔz�u���ꂽ�f�t�H���g�ʒu�ɗ��܂�܂��B
        }
    }
}