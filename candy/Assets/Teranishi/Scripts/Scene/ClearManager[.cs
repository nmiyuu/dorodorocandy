using UnityEngine;
using UnityEngine.SceneManagement;

public class ClearSceneController : MonoBehaviour
{
    void Update()
    {
        // Enterキー（Return）を押したらタイトルへ戻る
        if (Input.GetKeyDown(KeyCode.Return))
        {
            SceneManager.LoadScene("TitleScene"); // ←タイトルシーン名に変更
        }
    }
}
