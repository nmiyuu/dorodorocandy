using UnityEngine;

public class PressurePlate : MonoBehaviour
{
    public LayerMask playerLayer;
    public int targetBlockID; // このスイッチが操作するブロックのID

    private TargetBlock targetBlock;

    private void Start()
    {
        // ID が一致する TargetBlock を探す
        TargetBlock[] blocks = FindObjectsOfType<TargetBlock>();
        foreach (var b in blocks)
        {
            if (b.blockID == targetBlockID)
            {
                targetBlock = b;
                break;
            }
        }

        if (targetBlock == null)
        {
            Debug.LogError($"ID {targetBlockID} の TargetBlock が見つかりません！");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & playerLayer) != 0)
        {
            Debug.Log("指定レイヤーのオブジェクトが踏んだ！");
            if (targetBlock != null)
                targetBlock.SetActive(false);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (targetBlock != null)
                targetBlock.SetActive(true);
        }
    }
}
