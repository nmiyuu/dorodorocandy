using UnityEngine;
using UnityEngine.UI;
public class BatteryController : MonoBehaviour
{
    public Image batteryImage;        // UI��Image���A�^�b�`
    public Sprite[] batterySprites;   // ���^������̏���5���o�^
    private int cost = 5;             // ���݂̃R�X�g�i�ő�5�j
    void Start()
    {
        UpdateBatterySprite();        // �����\��
    }
    void OnCollisionEnter2D(Collision2D collision)
    {
        // ��iMoveblock�j�ɓ���������R�X�g��
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
        // cost=5��index0�i���^���j�Acost=0��index4�i��j
        int index = Mathf.Clamp(5 - cost, 0, batterySprites.Length - 1);
        batteryImage.sprite = batterySprites[index];
    }
}