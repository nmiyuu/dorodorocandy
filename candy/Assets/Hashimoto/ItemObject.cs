using UnityEngine;

public class ItemObject : MonoBehaviour
{
    public string matchstick; // 他と被らないID（例： "item_001"）

    void Start()
    {
        // すでに取得済みなら非表示にする
        if (ItemManager.Instance.obtainedItems.Contains(matchstick))
        {
            gameObject.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // 取得したIDを保存する
            ItemManager.Instance.obtainedItems.Add(matchstick);

            // 自分を消す
            gameObject.SetActive(false);
        }
    }
}