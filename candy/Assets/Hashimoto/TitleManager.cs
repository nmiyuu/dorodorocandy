using UnityEngine;
using UnityEngine.SceneManagement;

// FadeColor enumがTitleManagerクラスの外側（グローバル）に定義されていることを前提とします

public class TitleManager : MonoBehaviour
{
    // 「はじめから」
    public void OnStartButton()
    {
        // ゲーム内データ（シングルトン）を完全にリセット
        if (SceneDataTransfer.Instance != null)
        {
            SceneDataTransfer.Instance.FullReset();
            Debug.Log("[TitleManager] SceneDataTransferのデータを初期化しました。");
        }
        else
        {
            Debug.LogWarning("[TitleManager] SceneDataTransferのインスタンスが見つかりません。");
        }

        // セーブデータ（PlayerPrefs）を初期化
        PlayerPrefs.DeleteAll();

        // SceneFaderを使ってゲームシーンへ黒フェードで切り替え
        if (SceneFader.Instance != null)
        {
            SceneFader.Instance.LoadSceneWithFade("Stage1_now", FadeColor.Black);
        }
        else
        {
            // SceneFaderがない場合のフォールバック
            SceneManager.LoadScene("Stage1_now");
        }
    }

    // 「つづきから」
    public void OnContinueButton()
    {
        if (PlayerPrefs.HasKey("SaveScene"))
        {
            string sceneName = PlayerPrefs.GetString("SaveScene");
            // つづきからロード時も黒フェードを入れる
            if (SceneFader.Instance != null)
            {
                SceneFader.Instance.LoadSceneWithFade(sceneName, FadeColor.Black);
            }
            else
            {
                SceneManager.LoadScene(sceneName);
            }
        }
        else
        {
            // セーブがない場合は新しく開始
            Debug.Log("セーブデータがないため、新しくゲームを開始します。");
            if (SceneFader.Instance != null)
            {
                SceneFader.Instance.LoadSceneWithFade("Stage1_now", FadeColor.Black);
            }
            else
            {
                SceneManager.LoadScene("Stage1_now");
            }
        }
    }

    // 「おわり」
    public void OnExitButton()
    {
        Debug.Log("ゲームを終了します");
        Application.Quit();
    }
}