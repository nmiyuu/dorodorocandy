using UnityEngine;
using UnityEngine.SceneManagement;

public class ItemPersistence : MonoBehaviour
{
    private static ItemPersistence instance;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // �c�������Q�[���V�[����
        if (scene.name != "Stage1_now" && scene.name != "Stage1_Past")
        {
            // �S�[���V�[���Ȃ����
            Destroy(gameObject);
        }
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
