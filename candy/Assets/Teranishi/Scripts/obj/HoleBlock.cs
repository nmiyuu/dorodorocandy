using UnityEngine;

public class HoleBlock : MonoBehaviour
{
    [Tooltip("この穴が埋められた後に切り替わるスプライト。")]
    public Sprite filledHoleSprite;

    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        // AwakeでのBoxCollider2Dの取得は不要になりました
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
                // スプライトが設定されていない場合は非表示にする
                spriteRenderer.enabled = false;
            }
        }

        

        // アタッチされているすべてのCollider2Dを取得する
        Collider2D[] colliders = GetComponents<Collider2D>();

        if (colliders.Length > 0)
        {
            foreach (var col in colliders)
            {
                col.enabled = false; // 型に関係なく無効化
                Debug.Log($"穴ブロック '{gameObject.name}' の Collider ({col.GetType().Name}) を無効化しました。");
            }
        }
        else
        {
            Debug.LogError($"【無効化失敗】HoleBlock '{gameObject.name}' に Collider2D が見つかりません。");
        }

        Debug.Log($"穴ブロック '{gameObject.name}' が埋められました。");
    }
}