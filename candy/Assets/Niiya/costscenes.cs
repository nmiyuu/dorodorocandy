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
        // 残したいゲームシーン名
        if (scene.name != "Stage1_now" && scene.name != "Stage1_Past")
        {
            // ゴールシーンなら消す
            Destroy(gameObject);
        }
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
