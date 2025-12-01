using UnityEngine;
using UnityEngine.SceneManagement;

public class BreakableBlock : MonoBehaviour
{
    public string tree; // ← Inspector で空なら自動生成される

    void Awake()
    {
        // blockID が未設定ならシーン名 + オブジェクト名で自動生成
        if (string.IsNullOrEmpty(tree))
        {
            tree = $"{SceneManager.GetActiveScene().name}_{gameObject.name}";
        }
    }

    void Start()
    {
        Debug.Log($"ブロックID: {tree}");

        if (ItemManager.Instance == null)
        {
            Debug.Log("ItemManager が存在していません！！");
            return;
        }

        if (ItemManager.Instance.destroyedBlocks.Contains(tree))
        {
            Debug.Log(tree + " は破壊済み → 非表示にします");
            gameObject.SetActive(false);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Item")) // 当たり判定は適宜変更
        {
            // 破壊記録
            ItemManager.Instance.destroyedBlocks.Add(tree);

            // 消す
            gameObject.SetActive(false);
        }
    }
}