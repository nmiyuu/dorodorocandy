using UnityEngine;

public class ItemObject : MonoBehaviour
{
    public string matchstick; // ƒV[ƒ““à‚ÅˆêˆÓ‚ÌID‚ğİ’è

    void Start()
    {
        if (GameManager.Instance.obtainedItems.Contains(matchstick))
        {
            Destroy(gameObject); // ‚·‚Å‚Éæ“¾Ï‚İ‚È‚çíœ
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            GameManager.Instance.obtainedItems.Add(matchstick);
            Destroy(gameObject);
        }
    }
}
