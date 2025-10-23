using UnityEngine;

public class HoleBlock : MonoBehaviour
{
    [Tooltip("���̌������߂�ꂽ��ɐ؂�ւ��X�v���C�g�B")]
    public Sprite filledHoleSprite;

    private SpriteRenderer spriteRenderer;
    private BoxCollider2D holeCollider;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        // BoxCollider2D���A�^�b�`����Ă��邱�Ƃ�O��Ƃ��Ă��܂�
        holeCollider = GetComponent<BoxCollider2D>();

        if (holeCollider == null)
        {
            Debug.LogError($"�y�v���I�G���[�zHoleBlock '{gameObject.name}' �� BoxCollider2D ��������܂���B�������������X�L�b�v����܂��B");
        }
    }

    // FutureObstacleController����Ă΂�郁�\�b�h
    public void BeFilled()
    {
        // 1. �����ڂ̕ύX
        if (spriteRenderer != null)
        {
            if (filledHoleSprite != null)
            {
                spriteRenderer.sprite = filledHoleSprite;
            }
            else
            {
                spriteRenderer.enabled = false;
            }
        }

        // 2. �����蔻��̖�����
        if (holeCollider != null)
        {
            holeCollider.enabled = false;
            Debug.Log($"���u���b�N '{gameObject.name}' �� Collider �𖳌������܂����B");
        }
        else
        {
            Debug.LogError($"�y���������s�zHoleBlock '{gameObject.name}' �� BoxCollider2D �� null �ł��B");
        }

        Debug.Log($"���u���b�N '{gameObject.name}' �����߂��܂����B");
    }
}