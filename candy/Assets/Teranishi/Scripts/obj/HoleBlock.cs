using UnityEngine;

public class HoleBlock : MonoBehaviour
{
    [Tooltip("この穴が埋められた後に切り替わるスプライト。")]
    public Sprite filledHoleSprite;

    private SpriteRenderer spriteRenderer;
    private BoxCollider2D holeCollider;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        // BoxCollider2Dがアタッチされていることを前提としています
        holeCollider = GetComponent<BoxCollider2D>();

        if (holeCollider == null)
        {
            Debug.LogError($"【致命的エラー】HoleBlock '{gameObject.name}' に BoxCollider2D が見つかりません。無効化処理がスキップされます。");
        }
    }

    // FutureObstacleControllerから呼ばれるメソッド
    public void BeFilled()
    {
        // 1. 見た目の変更
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

        // 2. 当たり判定の無効化
        if (holeCollider != null)
        {
            holeCollider.enabled = false;
            Debug.Log($"穴ブロック '{gameObject.name}' の Collider を無効化しました。");
        }
        else
        {
            Debug.LogError($"【無効化失敗】HoleBlock '{gameObject.name}' の BoxCollider2D が null です。");
        }

        Debug.Log($"穴ブロック '{gameObject.name}' が埋められました。");
    }
}