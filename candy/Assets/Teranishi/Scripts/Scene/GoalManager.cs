using UnityEngine;
using UnityEngine.SceneManagement;

public class GoalManager : MonoBehaviour
{
    [Header("ステージ設定")]
    [Tooltip("ステージ名のプレフィックス (例: 'Stage')")]
    public string stageNamePrefix = "Stage";

    [Tooltip("全ステージの数")]
    public int totalStageCount = 5;

    [Tooltip("タイトルシーン名")]
    public string titleSceneName = "title";

    public void OnNextStageButton()
    {
        if (SceneDataTransfer.Instance == null)
        {
            Debug.LogError("SceneDataTransfer が見つかりません。");
            return;
        }

        // 1. 次に進むべきステージインデックスを計算
        int lastCleared = SceneDataTransfer.Instance.lastClearedStageIndex;
        int nextStageIndex = lastCleared + 1;

        if (nextStageIndex > totalStageCount)
        {
            Debug.Log("全ステージをクリアしました。");
            return;
        }

        // 2. 次のシーン名を 'Stage[N]_now' の形式で生成
        string nextSceneName = stageNamePrefix + nextStageIndex + "_now";

        // 3. 次のステージへ遷移 (t_goal.csで既にClearPlayerStateは実行済み)
        Debug.Log($"次のステージ: {nextSceneName} に移動します。");
        SceneManager.LoadScene(nextSceneName);
    }

    public void OnTitleButton()
    {
        // 完全にリセットしたい場合は以下を使用
        // if (SceneDataTransfer.Instance != null) SceneDataTransfer.Instance.FullReset(); 

        SceneManager.LoadScene(titleSceneName);
    }
}