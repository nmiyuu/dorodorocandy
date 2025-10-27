using UnityEngine;

public class PushTrigger : MonoBehaviour
{
    private CostDisplay costDisplay;

    void Start()
    {
        // タグで取得する場合
        costDisplay = GameObject.FindWithTag("CostImage").GetComponent<CostDisplay>();

        // 名前で取得する場合
        // costDisplay = GameObject.Find("CostImage").GetComponent<CostDisplay>();
    }

    public void Push()
    {
        if (costDisplay != null)
            costDisplay.DecreaseCost();
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("MoveBlock")) // 岩のタグ
        {
            Push(); // コスト減らす
        }
    }

}
