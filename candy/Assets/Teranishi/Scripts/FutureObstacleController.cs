using UnityEngine;
using System.Linq;

public class FutureObstacleController : MonoBehaviour
{
    // �ߋ���MoveBlock�ƑΉ����邽�߂�ID
    [Tooltip("�Ή�����ߋ���MoveBlock�Ɠ������j�[�NID��ݒ肵�Ă��������B")]
    public string blockID;

    // �������܂�����̃X�v���C�g
    [Header("�X�v���C�g�ݒ�")]
    [Tooltip("���̃u���b�N�����𖄂߂����ɐ؂�ւ��X�v���C�g�B")]
    public Sprite filledBlockSprite;

    // ���u���b�N�̃��C���[�}�X�N���擾���邽�߂̕ϐ�
    private LayerMask holeBlockLayer;
    private SpriteRenderer spriteRenderer; // ���̃I�u�W�F�N�g�̃X�v���C�g�����_���[

    void Awake()
    {
        // ���C���[�}�X�N�����������Ɏ擾 (���C���[��: Hole)
        holeBlockLayer = LayerMask.GetMask("Hole");

        // ������SpriteRenderer���擾
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogWarning($"FutureObstacleController '{gameObject.name}' �� SpriteRenderer ��������܂���B");
        }
    }

    void Start()
    {
        // 1. SceneDataTransfer�����݂��邩�m�F
        if (SceneDataTransfer.Instance == null)
        {
            Debug.LogError("SceneDataTransfer ��������܂���B");
            return;
        }

        // 2. �ߋ��̃u���b�N�̕ۑ��ʒu�� SceneDataTransfer ���猟��
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

            // �ʒu������Ɍ����߃`�F�b�N�����s
            CheckIfFillingHole();
        }
        else
        {
            // �ߋ��̃u���b�N����x����������Ă��Ȃ��ꍇ�́A�f�t�H���g�ʒu�ɗ��܂�܂��B
        }
    }

    private void CheckIfFillingHole()
    {
        BoxCollider2D selfCollider = GetComponent<BoxCollider2D>();
        if (selfCollider == null) return;

        Collider2D[] hits = Physics2D.OverlapBoxAll(
            (Vector2)transform.position + selfCollider.offset,
            selfCollider.size,
            0f,
            holeBlockLayer
        );

        if (hits.Length > 0)
        {
            // �������g�̃X�v���C�g��؂�ւ���
            if (spriteRenderer != null && filledBlockSprite != null)
            {
                spriteRenderer.sprite = filledBlockSprite;
                Debug.Log($"'{blockID}' �̃X�v���C�g�𖄂܂�����̏�Ԃɐ؂�ւ��܂����B");
            }

            selfCollider.enabled = false; // ������Collider�𖳌���

            // ���u���b�N��HoleBlock�X�N���v�g���Ăяo���ď�Ԃ�ύX������
            foreach (var hit in hits)
            {
                // ���C���[�����uHole�v�̃I�u�W�F�N�g�݂̂�����
                if (hit.gameObject.layer == LayerMask.NameToLayer("Hole"))
                {
                    HoleBlock hole = hit.gameObject.GetComponent<HoleBlock>();
                    if (hole != null)
                    {
                        hole.BeFilled(); // ���u���b�N�Ɏ��g�̏�ԕύX���˗�����
                    }
                    else
                    {
                        Debug.LogError($"�y�X�N���v�g�G���[�z���o�����I�u�W�F�N�g '{hit.gameObject.name}' �� HoleBlock.cs ��������܂���B");
                    }
                }
            }

            Debug.LogWarning($"�ÓI��Q�� '{blockID}' �͌��𖄂߂܂����B���ƂȂ�܂����B");
        }
        else
        {
            Debug.LogWarning($"FutureObstacleController '{gameObject.name}' �́A���̈ʒu�� 'Hole' ���C���[�̃I�u�W�F�N�g�����o�ł��܂���ł����BOverlapBoxAll�����s�B");
        }
    }
}