using UnityEngine;

public class BreakableBlock : MonoBehaviour
{
    public UniqueID tree;  // © ©“®¶¬‚µ‚½ID‚ğQÆ‚·‚é

    void OnEnable()
    {
        // ”j‰óÏ‚İ‚È‚çÁ‚·
        if (ItemManager.Instance.destroyedBlocks.Contains(tree.id))
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") &&
            ItemManager.Instance.obtainedItems.Count > 0)
        {
            // ‚±‚ÌƒuƒƒbƒN‚ÌID‚ğ“o˜^
            ItemManager.Instance.destroyedBlocks.Add(tree.id);
            Destroy(gameObject);
        }
    }
}