using UnityEngine;

public class MatchStickItem : MonoBehaviour
{
    [Tooltip("このアイテムを特定するためのユニークID (例: STAGE3_MATCH_A)")]
    public string itemID;

    void Start()
    {
        if (SceneDataTransfer.Instance == null) return;

        // 【追加】シーンロード時、既にアイテムが消滅した記録があるかチェックする
        if (SceneDataTransfer.Instance.IsItemVanished(itemID))
        {
            // 既に消滅している場合は、このオブジェクトを非表示にする
            gameObject.SetActive(false);
            Debug.Log($"[MatchStickItem] ID:{itemID} は取得済みのため非表示にしました。");
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // プレイヤーに接触したか確認
        if (collision.gameObject.GetComponent<t_player>())
        {
            if (SceneDataTransfer.Instance != null && !SceneDataTransfer.Instance.hasMatchStick)
            {
                // 1. マッチ棒の保持状態を記録
                SceneDataTransfer.Instance.AcquireMatchStick();

                // 2. 【追加】アイテムが恒久的に消滅したことを記録
                SceneDataTransfer.Instance.RecordItemVanished(itemID);

                // 3. アイテムオブジェクトをシーンから非表示にする
                gameObject.SetActive(false);

                Debug.Log("[MatchStickItem] マッチ棒を取得し、永続的な消滅を記録しました。");
            }
        }
    }
}