using UnityEngine;
using UnityEngine.SceneManagement;

public class GoalManager : MonoBehaviour
{
    // 次のステージ名を設定 (Inspectorで設定)
    public string nextStageName = "Stage2_now";

    // タイトルシーン名 (TitleManagerから流用)
    public string titleSceneName = "title";

    // 「次のステージへ」ボタンが押されたとき
    public void OnNextStageButton()
    {
        // データを完全にリセットする必要はないが、SceneDataTransferのクリアは必要かも
        if (SceneDataTransfer.Instance != null)
        {
            SceneDataTransfer.Instance.FullReset();
            // プレイヤーの位置情報などをリセットし、次のステージの初期位置から開始させる
        }

        // SceneFaderを使って次のステージへ黒フェードで移動（リセットではないので黒が自然）
        if (SceneFader.Instance != null)
        {
            SceneFader.Instance.LoadSceneWithFade(nextStageName, FadeColor.Black);
        }
        else
        {
            SceneManager.LoadScene(nextStageName);
        }
    }

    // 「タイトルへ戻る」ボタンが押されたとき
    public void OnTitleButton()
    {
        // タイトルに戻る前にセーブデータを削除するならここで行う
        // PlayerPrefs.DeleteAll();

        // SceneFaderを使ってタイトルシーンへ黒フェードで移動
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