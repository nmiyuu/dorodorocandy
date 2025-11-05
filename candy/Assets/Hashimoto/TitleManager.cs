using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{
    // 「はじめから」
    public void OnStartButton()
    {
        // 例: 新しくゲームを開始する場合
        PlayerPrefs.DeleteAll(); // セーブデータを初期化
        SceneManager.LoadScene("Stage1_now"); // ゲームシーンへ切り替え
    }

    // 「つづきから」
    public void OnContinueButton()
    {
        if (PlayerPrefs.HasKey("SaveScene"))
        {
            string sceneName = PlayerPrefs.GetString("SaveScene");
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            // セーブがない場合は新しく開始
            SceneManager.LoadScene("GameScene");
        }
    }

    // 「おわり」
    public void OnExitButton()
    {
        Debug.Log("ゲームを終了します");
        Application.Quit();
    }
}
