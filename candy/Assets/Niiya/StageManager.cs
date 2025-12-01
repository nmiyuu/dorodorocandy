using UnityEngine;

public class StageManager : MonoBehaviour
{
    // ステージをクリアしたときに呼ぶ
    public void ClearStage(int stageIndex)
    {
        // 現在保存されているクリア済みステージの最大値を取得（未保存なら0）
        int savedStage = PlayerPrefs.GetInt("ClearedStage", 0);

        // 今回クリアしたステージ番号 + 1 が既存の値より大きければ更新
        if (stageIndex + 1 > savedStage)
        {
            PlayerPrefs.SetInt("ClearedStage", stageIndex + 1);
            PlayerPrefs.Save(); // 忘れずに保存
            Debug.Log("ステージ " + stageIndex + " クリア済みとして保存しました");
        }
    }
}
