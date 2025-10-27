using UnityEngine;

public class CostDisplay : MonoBehaviour
{
    public Sprite[] costSprites;  // コスト0〜3の画像
    public int cost = 3;          // 初期コスト
    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        UpdateSprite();
    }

    public void DecreaseCost()
    {
        if (cost > 0)
        {
            cost--;
            UpdateSprite();
        }
    }

    void UpdateSprite()
    {
        if (cost >= 0 && cost < costSprites.Length)
            spriteRenderer.sprite = costSprites[cost];
    }
}