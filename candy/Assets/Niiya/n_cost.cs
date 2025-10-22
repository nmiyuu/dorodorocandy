using UnityEngine;
using UnityEngine.UI;
public class BatteryController : MonoBehaviour
{
    public Image batteryImage;        // UIのImageをアタッチ
    public Sprite[] batterySprites;   // 満タン→空の順で5枚登録
    private int cost = 5;             // 現在のコスト（最大5）
    void Start()
    {
        UpdateBatterySprite();        // 初期表示
    }
    void OnCollisionEnter2D(Collision2D collision)
    {
        // 岩（Moveblock）に当たったらコスト減
        if (collision.gameObject.CompareTag("Moveblock"))
        {
            DecreaseCost();
        }
    }
    void DecreaseCost()
    {
        if (cost > 0)
        {
            cost--;
            UpdateBatterySprite();
        }
    }
    void UpdateBatterySprite()
    {
        // cost=5→index0（満タン）、cost=0→index4（空）
        int index = Mathf.Clamp(5 - cost, 0, batterySprites.Length - 1);
        batteryImage.sprite = batterySprites[index];
    }
}