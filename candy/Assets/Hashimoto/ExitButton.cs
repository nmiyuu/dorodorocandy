using UnityEngine;

public class ExitButton : MonoBehaviour
{
    // ボタンから呼び出す関数
    public void QuitGame()
    {
        // エディタ上では停止
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // ビルド版ではアプリケーションを終了
        Application.Quit();
#endif
    }
}
