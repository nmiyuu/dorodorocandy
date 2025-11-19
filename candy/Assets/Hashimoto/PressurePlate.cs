using UnityEngine;

public class PressurePlate : MonoBehaviour
{
    public GameObject targetBlock;  // 消したいブロック

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            targetBlock.SetActive(false);   // 踏んだらブロック消える
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            targetBlock.SetActive(true);    // 離したらブロック戻る
        }
    }
}
