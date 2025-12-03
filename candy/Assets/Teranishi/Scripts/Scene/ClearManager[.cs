using UnityEngine;
using UnityEngine.SceneManagement;

public class ClearManager : MonoBehaviour
{
    void Update()
    {
        // Enterキー（Return）を押したらタイトルへ戻る
        if (Input.GetKeyDown(KeyCode.Return))
        {
            SceneManager.LoadScene("title"); // ←タイトルシーン名に変更
        }
    }
}
