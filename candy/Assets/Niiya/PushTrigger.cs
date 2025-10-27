using UnityEngine;

public class PushTrigger : MonoBehaviour
{
    private CostDisplay costDisplay;

    void Start()
    {
        // �^�O�Ŏ擾����ꍇ
        costDisplay = GameObject.FindWithTag("CostImage").GetComponent<CostDisplay>();

        // ���O�Ŏ擾����ꍇ
        // costDisplay = GameObject.Find("CostImage").GetComponent<CostDisplay>();
    }

    public void Push()
    {
        if (costDisplay != null)
            costDisplay.DecreaseCost();
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("MoveBlock")) // ��̃^�O
        {
            Push(); // �R�X�g���炷
        }
    }

}
