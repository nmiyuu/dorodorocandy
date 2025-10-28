using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;

    public GameObject playerPrefab;          // �v���C���[��Prefab
    public Vector3 playerStartPos = new Vector3(0, -1, 0); // �v���C���[�����ʒu

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // �풓
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
    }

    IEnumerator ResetGame()
    {
        Debug.Log("�Q�[�����Z�b�g�J�n...");

        // �@ �Â��v���C���[���폜
        GameObject oldPlayer = GameObject.FindGameObjectWithTag("Player");
        if (oldPlayer != null)
        {
            Destroy(oldPlayer);
            yield return null; // 1�t���[���҂�
        }

        // �A Scene1, Scene2 ���A�����[�h
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

        // �B Scene1, Scene2 ���ă��[�h
        foreach (string sceneName in scenesToReload)
        {
            AsyncOperation load = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            while (!load.isDone)
                yield return null;
        }

        // �C �v���C���[�𐶐�
        if (playerPrefab != null)
        {
            Instantiate(playerPrefab, playerStartPos, Quaternion.identity);
        }

        Debug.Log("�Q�[�����Z�b�g�����B�I�u�W�F�N�g���v���C���[������������܂����B");
    }
}