using UnityEngine;
using UnityEngine.SceneManagement;
// using System.Threading; // 不要なため削除

// FadeColor enumがTitleManagerクラスの外側（グローバル）に定義されていることを前提とします

public class t_title : MonoBehaviour
{
    private int count = 0;
    public bool swich = false;

    // ステージ名生成用の定数
    private const string StagePrefix = "Stage";
    private const string StageSuffix = "_now";

    // 「はじめから」
    public void OnStartButton()
    {
        swich = true;

        // 1. ゲーム内データ（シングルトン）を完全にリセット
        if (SceneDataTransfer.Instance != null)
        {
            // FullGameReset() は lastClearedStageIndex を 0 にリセットします。
            SceneDataTransfer.Instance.FullGameReset();
            Debug.Log("[t_title] SceneDataTransferのデータを初期化しました。");
        }
        else
        {
            Debug.LogWarning("[t_title] SceneDataTransferのインスタンスが見つかりません。");
        }

        // 2. セーブデータ（PlayerPrefs）を初期化
        PlayerPrefs.DeleteAll();

        // 3. 最初のステージ名（Stage1_now）を生成
        string firstStage = StagePrefix + 1 + StageSuffix;

        // 4. SceneFaderを使ってゲームシーンへ黒フェードで切り替え
        if (SceneFader.Instance != null)
        {
            SceneFader.Instance.LoadSceneWithFade(firstStage, FadeColor.Black);
        }
        else
        {
            // SceneFaderがない場合のフォールバック
            SceneManager.LoadScene(firstStage);
        }

        // 【注意】手動カウントによるシーンロードは削除しました。
    }

    // 「つづきから」
    public void OnContinueButton()
    {
        int startStageIndex = 1; // デフォルトはステージ1
        string sceneName = StagePrefix + 1 + StageSuffix; // デフォルトは "Stage1_now"

        if (SceneDataTransfer.Instance != null)
        {
            // 最後にクリアしたステージの次のステージを開始
            int lastCleared = SceneDataTransfer.Instance.lastClearedStageIndex;

            // lastClearedStageIndex が 0 の場合、startStageIndex は 1 になる（Stage1_now）
            startStageIndex = lastCleared + 1;

            if (startStageIndex >= 1)
            {
                // 例: Stage2_now のようなシーン名を生成
                sceneName = StagePrefix + startStageIndex + StageSuffix;
            }

            Debug.Log($"[t_title] つづきから: ステージ {startStageIndex} をロードします。");
        }

        // SceneFaderを使ってシーンを切り替え
        if (SceneFader.Instance != null)
        {
            SceneFader.Instance.LoadSceneWithFade(sceneName, FadeColor.Black);
        }
        else
        {
            SceneManager.LoadScene(sceneName);
        }
    }

    private void Update()
    {
        if (swich == true)
        {
            count++;
        }
    }

    // 「おわり」
    public void OnExitButton()
    {
        Debug.Log("ゲームを終了します");
        Application.Quit();
    }
}