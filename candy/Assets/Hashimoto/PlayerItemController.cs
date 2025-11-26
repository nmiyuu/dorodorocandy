using UnityEngine;

public class PlayerItemController : MonoBehaviour
{
    private bool hasItem = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("接触2D: " + other.name);

        if (other.CompareTag("Item"))
        {
            Debug.Log("アイテム取得！");
            hasItem = true;
            Destroy(other.gameObject);
        }

        if (other.CompareTag("BreakableBlock") && hasItem)
        {
            Debug.Log("ブロック破壊！");
            Destroy(other.gameObject);
            hasItem = false;
        }
    }
}