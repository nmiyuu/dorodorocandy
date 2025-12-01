using UnityEngine;
using UnityEngine.SceneManagement;
// SceneFader がある場合は、コメントアウト部分を有効にしてください。

public class GoalManager : MonoBehaviour
{
    [Header("ステージ設定")]
    [Tooltip("ステージ名のプレフィックス (例: 'Stage')")]
    public string stageNamePrefix = "Stage";

    [Tooltip("全ステージの数")]
    public int totalStageCount = 5;

    // タイトルシーン名 (Inspectorで設定するか、TitleManagerから流用)
    public string titleSceneName = "title";

    // --- メソッド ---

    /// <summary>
    /// 「次のステージへ」ボタンが押されたとき、クリア状況に基づいて次の Stage[N]_now シーンをロードする。
    /// </summary>
    public void OnNextStageButton()
    {
        if (SceneDataTransfer.Instance == null)
        {
            Debug.LogError("SceneDataTransfer が見つかりません。");
            return;
        }

        // 1. 次に進むべきステージインデックスを計算
        // lastClearedStageIndexが3なら、次は4
        int lastCleared = SceneDataTransfer.Instance.lastClearedStageIndex;
        int nextStageIndex = lastCleared + 1;

        if (nextStageIndex > totalStageCount)
        {
            Debug.Log("全ステージをクリアしました。");
            // 例: エンディングシーンへ遷移
             SceneManager.LoadScene("clear"); 
            return;
        }

        // 2. 次のシーン名を 'Stage[N]_now' の形式で生成
        string nextSceneName = stageNamePrefix + nextStageIndex + "_now";

        // 3. プレイヤーの状態とブロックデータをリセット
        // ステージクリア情報は残しつつ、次のステージは初期状態から開始させる
        SceneDataTransfer.Instance.ClearPlayerState();

        // 4. 次のステージへ遷移
        Debug.Log($"次のステージ: {nextSceneName} に移動します。");

        // SceneFaderを使って次のステージへ黒フェードで移動（SceneFaderがない場合は下の SceneManager.LoadScene を利用）
        /*
        if (SceneFader.Instance != null)
        {
            SceneFader.Instance.LoadSceneWithFade(nextSceneName, FadeColor.Black);
        }
        else
        {
            SceneManager.LoadScene(nextSceneName);
        }
        */

        // SceneFaderがない場合の暫定処理
        SceneManager.LoadScene(nextSceneName);
    }

    /// <summary>
    /// 「タイトルへ戻る」ボタンが押されたとき。
    /// </summary>
    public void OnTitleButton()
    {
        // 完全にゲーム状態をリセットしたい場合は FullReset() を使用できます。
        // SceneDataTransfer.Instance.FullReset();

        // SceneFaderを使ってタイトルシーンへ黒フェードで移動
        /*
        if (SceneFader.Instance != null)
        {
            SceneFader.Instance.LoadSceneWithFade(titleSceneName, FadeColor.Black);
        }
        else
        {
            SceneManager.LoadScene(titleSceneName);
        }
        */

        // SceneFaderがない場合の暫定処理
        SceneManager.LoadScene(titleSceneName);
    }
}