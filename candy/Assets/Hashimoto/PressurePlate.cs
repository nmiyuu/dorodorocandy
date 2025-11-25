using UnityEngine;

public class PressurePlate : MonoBehaviour
{
    public LayerMask playerLayer;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & playerLayer) != 0)
        {
            Debug.Log("指定レイヤーのオブジェクトが踏んだ！");
            // ブロックを消す処理
            // ブロックを非表示にする
            TargetBlock.isActive = false;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // ブロックを再表示する（必要なら）
            TargetBlock.isActive = true;
        }
    }
}