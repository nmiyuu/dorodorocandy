using UnityEngine;

[ExecuteAlways]
public class UniqueID : MonoBehaviour
{
    public string id;

    private void Reset()
    {
        // 新しい GUID を自動生成（重複しない）
        id = System.Guid.NewGuid().ToString();
    }
}
