using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    void Start()
    {
        // �N������ Scene1 �� Scene2 ��ǉ����[�h
        SceneManager.LoadScene("Stage1_now", LoadSceneMode.Additive);
        SceneManager.LoadScene("Stage1_Past", LoadSceneMode.Additive);
    }
}