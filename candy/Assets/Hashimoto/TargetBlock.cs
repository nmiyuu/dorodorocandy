using UnityEngine;

public class TargetBlock : MonoBehaviour
{
    // 別シーンからブロックの状態を制御する静的変数
    public static bool isActive = true;

    void Update()
    {
        // 静的変数に従ってブロックの表示/非表示を切り替える
        gameObject.SetActive(isActive);
    }
}