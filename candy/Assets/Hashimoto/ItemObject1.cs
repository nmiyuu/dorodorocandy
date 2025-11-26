using UnityEngine;

public class ItemObject : MonoBehaviour
{
    public string itemID; // ƒV[ƒ““à‚ÅˆêˆÓ‚ÌID‚ğİ’è

    void Start()
    {
        if (GameManager.Instance.obtainedItems.Contains(itemID))
        {
            Destroy(gameObject); // ‚·‚Å‚Éæ“¾Ï‚İ‚È‚çíœ
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            GameManager.Instance.obtainedItems.Add(itemID);
            Destroy(gameObject);
        }
    }
}
