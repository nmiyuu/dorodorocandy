using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;

    public GameObject playerPrefab;          // プレイヤーのPrefab
    public Vector3 playerStartPos = new Vector3(0, -1, 0); // プレイヤー初期位置
    public int count = 0;


    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // 常駐
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            StartCoroutine(ResetGame());
        }

        count++;
    }

    IEnumerator ResetGame()
    {
        Debug.Log("ゲームリセット開始...");

        // ① 古いプレイヤーを削除
        GameObject oldPlayer = GameObject.FindGameObjectWithTag("Player");
        if (oldPlayer != null)
        {
            Destroy(oldPlayer);
            yield return null; // 1フレーム待つ
        }

        // ② Scene1, Scene2 をアンロード
        int sceneCount = SceneManager.sceneCount;
        List<string> scenesToReload = new List<string>();

        for (int i = 0; i < sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (scene.name != "PersistentScene" && scene.isLoaded)
            {
                scenesToReload.Add(scene.name);
            }
        }

        foreach (string sceneName in scenesToReload)
        {
            AsyncOperation unload = SceneManager.UnloadSceneAsync(sceneName);
            while (!unload.isDone)
                yield return null;
        }

        // ③ Scene1, Scene2 を再ロード
        foreach (string sceneName in scenesToReload)
        {
            AsyncOperation load = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            while (!load.isDone)
                yield return null;
        }

        // ④ プレイヤーを生成
        if (playerPrefab != null)
        {
            Instantiate(playerPrefab, playerStartPos, Quaternion.identity);
        }

        Debug.Log("ゲームリセット完了。オブジェクトもプレイヤーも初期化されました。");
    }
}