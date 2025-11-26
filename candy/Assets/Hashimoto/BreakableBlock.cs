using UnityEngine;

public class BreakableBlock : MonoBehaviour
{
    public string tree; // ƒV[ƒ““à‚ÅˆêˆÓ‚ÌID‚ðÝ’è

    void Start()
    {
        if (GameManager.Instance.destroyedBlocks.Contains(tree))
        {
            Destroy(gameObject); // ‚·‚Å‚É”j‰óÏ‚Ý‚È‚çíœ
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && GameManager.Instance.obtainedItems.Count > 0)
        {
            GameManager.Instance.destroyedBlocks.Add(tree);
            Destroy(gameObject);
        }
    }
}
