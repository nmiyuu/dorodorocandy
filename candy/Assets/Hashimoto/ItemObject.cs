using UnityEngine;

public class ItemObject : MonoBehaviour
{
    // このアイテムの一意ID（同じIDのアイテムは2つ存在させない）
    public string matchstick;

    void OnEnable()
    {
        // ゲーム開始後に再度シーンが読み込まれた場合など、
        // すでに取得済みのIDと一致していたら自動的に消す
        if (ItemManager.Instance.obtainedItems.Contains(matchstick))
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // プレイヤーが触れたときにアイテムを取得
        if (other.CompareTag("Player"))
        {
            // アイテム取得IDを記録
            ItemManager.Instance.obtainedItems.Add(matchstick);

            // 取得したのでオブジェクトを消す
            Destroy(gameObject);
        }
    }
}