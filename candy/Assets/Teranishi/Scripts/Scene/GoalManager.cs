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
        int nextStageIndex = lastCleared + 1; // ステージ5をクリアしたなら nextStageIndex は 6 になる

        // 2. ゲームクリア判定 
        if (lastCleared >= finalStageIndex) // lastClearedが5以上ならゲームクリア
        {
            Debug.Log("🎉 全ステージをクリアしました。クリアシーンへ遷移します。");

            // クリアシーンへ遷移 (黒フェード)
            SceneFader.Instance.LoadSceneWithFade(gameClearSceneName, FadeColor.Black);
            return; // ここで処理を終了
        }

        // 3. 通常の次のステージへの遷移

        // 次のシーン名を 'Stage[N]_now' の形式で生成
        string nextSceneName = stageNamePrefix + nextStageIndex + "_now";

        // 次のステージへ遷移 (黒フェード)
        Debug.Log($"次のステージ: {nextSceneName} に移動します。");
        SceneFader.Instance.LoadSceneWithFade(nextSceneName, FadeColor.Black);
    }

    public void OnTitleButton()
    {
        if (SceneFader.Instance == null)
        {
            SceneManager.LoadScene(titleSceneName);
            return;
        }

        SceneFader.Instance.LoadSceneWithFade(titleSceneName, FadeColor.Black);
        if (SceneDataTransfer.Instance != null)
        {
            SceneDataTransfer.Instance.FullGameReset();
        }
    }
}