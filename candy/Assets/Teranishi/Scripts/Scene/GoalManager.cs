using UnityEngine;
using UnityEngine.SceneManagement;

public class GoalManager : MonoBehaviour
{
    // --- 設定 ---
    [Header("ステージ設定")]
    public string stageNamePrefix = "Stage";

    // ★修正箇所: 最終ステージのインデックスを設定
    public int finalStageIndex = 5;

    [Tooltip("タイトルシーン名")]
    public string titleSceneName = "title";

    // ★追加: ゲームクリアシーン名
    public string gameClearSceneName = "clear";

    // --- メソッド ---

    public void OnNextStageButton()
    {
        if (SceneDataTransfer.Instance == null || SceneFader.Instance == null)
        {
            Debug.LogError("SceneDataTransfer または SceneFader が見つかりません。");
            return;
        }

        // 1. 次に進むべきステージインデックスを計算
        int lastCleared = SceneDataTransfer.Instance.lastClearedStageIndex;
        int nextStageIndex = lastCleared + 1;

        // 2. ゲームクリア判定 
        if (lastCleared >= finalStageIndex)
        {
            Debug.Log("🎉 全ステージをクリアしました。クリアシーンへ遷移します。");

            // クリアシーンへ遷移 (黒フェード)
            SceneFader.Instance.LoadSceneWithFade(gameClearSceneName, FadeColor.Black);
            return;
        }

        // 3. 通常の次のステージへの遷移
        string nextSceneName = stageNamePrefix + nextStageIndex + "_now";

        // 次のステージへ遷移 (黒フェード)
        Debug.Log($"次のステージ: {nextSceneName} に移動します。");
        SceneFader.Instance.LoadSceneWithFade(nextSceneName, FadeColor.Black);
    }

    public void OnTitleButton()
    {
        // ★修正点: FullGameResetを呼び出さないようにします。
        // 代わりに、シーン上のプレイヤー位置などの一時データだけを消す
        // ClearPlayerState を呼び出し、フェード演出でタイトルへ戻ります。

        if (SceneDataTransfer.Instance != null)
        {
            // クリア状況(lastClearedStageIndex)は維持し、
            // 現在の歩数やブロックの位置情報だけをリセットします。
            SceneDataTransfer.Instance.ClearPlayerState();
            Debug.Log("[GoalManager] プレイヤーの一時データをリセットしました（クリア状況は維持）。");
        }

        if (SceneFader.Instance != null)
        {
            SceneFader.Instance.LoadSceneWithFade(titleSceneName, FadeColor.Black);
        }
        else
        {
            SceneManager.LoadScene(titleSceneName);
        }
    }
}