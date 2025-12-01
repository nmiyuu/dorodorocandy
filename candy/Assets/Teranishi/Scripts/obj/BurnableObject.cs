using UnityEngine;

public class BurnableObject : MonoBehaviour
{
    [Tooltip("このオブジェクトを特定するためのユニークID (例: STAGE1_VINE_A)")]
    public string objectID;

    void Start()
    {
        // シーンロード時、既に燃やされた記録があるかチェックする
        if (SceneDataTransfer.Instance != null && SceneDataTransfer.Instance.IsObjectBurned(objectID))
        {
            // 既に燃やされている場合は、このオブジェクトを非表示にする
            gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 外部（プレイヤー）から燃やされる処理を呼び出す
    /// </summary>
    public void Burn()
    {
        if (SceneDataTransfer.Instance != null)
        {
            // 燃やされた記録をシングルトンに保存
            SceneDataTransfer.Instance.RecordBurnedObject(objectID);

            // このオブジェクトを非表示にする
            gameObject.SetActive(false);

            Debug.Log($"[BurnableObject] ID:{objectID} が燃やされ、状態が記録されました。");
        }
    }
}