using UnityEngine;

public class TargetBlock : MonoBehaviour
{
    public int blockID; // ÉuÉçÉbÉNÇ≤Ç∆ÇÃID
    public bool isActive = true;

    public void SetActive(bool value)
    {
        isActive = value;
        gameObject.SetActive(value);
    }
}
