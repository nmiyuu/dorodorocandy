using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public static Vector2 checkpointPos;
    public static bool hasCheckpoint = false;

    // Playerレイヤー番号をInspectorで設定
    public LayerMask playerLayer;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 衝突した相手がPlayerレイヤーか？
        if ((playerLayer & (1 << collision.gameObject.layer)) != 0)
        {
            checkpointPos = transform.position;
            hasCheckpoint = true;
        }
    }
}
