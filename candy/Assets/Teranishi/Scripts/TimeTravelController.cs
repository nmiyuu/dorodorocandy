using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Linq;

public class TimeTravelController : MonoBehaviour
{
    // --- �V�[�����ݒ� (Inspector�Őݒ�) ---
    public string pastSceneName = "Stage1_Past";
    public string presentSceneName = "Stage1_now";

    private bool isSwitchingScene = false;
    private GameObject playerObject;
    private t_pl playerScriptRef; // �X�v���C�g����p
    private t_player playerMovementScript; // ���ǉ�: �ړ�����p
    private BoxCollider2D playerColliderRef;
    private LayerMask obstacleLayer;

    private bool TrySetPlayerReferences()
    {
        if (playerObject == null)
        {
            playerObject = GameObject.FindGameObjectWithTag("Player");
        }

        if (playerObject != null && playerScriptRef == null)
        {
            playerScriptRef = playerObject.GetComponent<t_pl>();

            // t_player�i�ړ�����j�̎Q�Ƃ��擾
            playerMovementScript = playerObject.GetComponent<t_player>();

            if (playerScriptRef != null && playerMovementScript != null)
            {
                playerColliderRef = playerObject.GetComponent<BoxCollider2D>();
                obstacleLayer = playerMovementScript.obstacleLayer; // t_player����LayerMask���擾
            }
            else
            {
                if (playerScriptRef == null) Debug.LogError($"Player�I�u�W�F�N�g '{playerObject.name}' �� t_pl �X�N���v�g��������܂���B");
                if (playerMovementScript == null) Debug.LogError($"Player�I�u�W�F�N�g '{playerObject.name}' �� t_player �X�N���v�g�i�ړ�����j��������܂���B�ړ����`�F�b�N�͋@�\���܂���B");
            }
        }
        return playerObject != null && playerScriptRef != null && playerColliderRef != null && playerMovementScript != null;
    }

    void Start()
    {
        TrySetPlayerReferences();
        if (playerObject == null)
        {
            Debug.LogError("�^�O�� 'Player' �̃I�u�W�F�N�g��������܂���BTimeTravelController�����삵�܂���B");
        }
    }

    void Update()
    {
        if (isSwitchingScene) return;
        if (!TrySetPlayerReferences()) return;

        // �ړ����`�F�b�N��ǉ�
        if (playerMovementScript.IsPlayerMoving) return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            StartCoroutine(TrySwitchTimeLine());
        }
    }

    private void SetSceneRenderingEnabled(Scene scene, bool isEnabled)
    {
        foreach (GameObject rootObject in scene.GetRootGameObjects())
        {
            foreach (Renderer renderer in rootObject.GetComponentsInChildren<Renderer>(true))
            {
                renderer.enabled = isEnabled;
            }
        }
    }


    public IEnumerator TrySwitchTimeLine()
    {
        isSwitchingScene = true;

        // �R���[�`���J�n����ɂ�����x�ړ����`�F�b�N
        if (playerMovementScript.IsPlayerMoving)
        {
            isSwitchingScene = false;
            yield break;
        }

        Scene currentScene = SceneManager.GetActiveScene();
        string currentSceneName = currentScene.name;
        string nextSceneName = string.Empty;

        Vector3 nextPlayerPosition = playerObject.transform.position;

        // �f�[�^�̕ۑ��Ǝ��̃V�[�����̌���
        if (currentSceneName == presentSceneName)
        {
            nextSceneName = pastSceneName;
        }
        else if (currentSceneName == pastSceneName)
        {
            nextSceneName = presentSceneName;
            if (SceneDataTransfer.Instance != null)
            {
                SceneDataTransfer.Instance.SaveBlockPositions();
            }
        }
        else
        {
            Debug.LogWarning("����`�̃V�[���ł�: " + currentSceneName);
            isSwitchingScene = false;
            yield break;
        }

        // �v���C���[�̕��A�ʒu�ƌ������f�[�^�]���I�u�W�F�N�g�ɕۑ�
        if (SceneDataTransfer.Instance != null)
        {
            SceneDataTransfer.Instance.playerPositionToLoad = nextPlayerPosition;

            // ���C��: ���݂̌������f�[�^�]���I�u�W�F�N�g�ɕۑ�
            SceneDataTransfer.Instance.playerDirectionToLoad = playerScriptRef.CurrentDirection;
        }

        // 1. �V�����V�[����񓯊��Ń��[�h
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(nextSceneName, LoadSceneMode.Additive);
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        Scene nextScene = SceneManager.GetSceneByName(nextSceneName);
        if (!nextScene.IsValid())
        {
            Debug.LogError("�V�[���̒ǉ����[�h�Ɏ��s���܂���: " + nextSceneName);
            isSwitchingScene = false;
            yield break;
        }

        // 2. �V�����V�[���̕`��������ɗ}��
        SetSceneRenderingEnabled(nextScene, false);

        // 3. �V�����V�[�����A�N�e�B�u�V�[���ɐݒ�iStart() / Awake() �����s�����j
        SceneManager.SetActiveScene(nextScene);

        // 4. �u���b�N�̔z�u�ƕ������Z�̈����҂�
        yield return new WaitForFixedUpdate();
        yield return new WaitForEndOfFrame();
        yield return null;

        // 5. �v���C���[�Q�Ƃ̍Ď擾
        playerObject = null;
        playerScriptRef = null;
        playerColliderRef = null;
        playerMovementScript = null;

        if (!TrySetPlayerReferences())
        {
            Debug.LogError("�V�����V�[���Ńv���C���[�I�u�W�F�N�g��������܂���ł����B");
            isSwitchingScene = false;
            yield break;
        }

        // 6. �Փ˔���
        if (playerColliderRef != null)
        {
            Collider2D hitCollider = Physics2D.OverlapBox(
                (Vector2)nextPlayerPosition + playerColliderRef.offset,
                playerColliderRef.size,
                0f,
                obstacleLayer
            );

            // 7. �Փ˔����̏���
            if (hitCollider != null)
            {
                Debug.LogWarning($"�^�C���g���x�����~: ���A�ʒu({nextPlayerPosition})�ɏ�Q��('{hitCollider.gameObject.name}')������܂��B");

                SceneManager.SetActiveScene(currentScene);

                AsyncOperation unloadOp = SceneManager.UnloadSceneAsync(nextScene);
                while (!unloadOp.isDone)
                {
                    yield return null;
                }

                yield return null;

                isSwitchingScene = false;
                Debug.Log("�؂�ւ����L�����Z������܂����B");
                yield break;
            }
        }

        // 8. ���������ꍇ�̂݁A�`���L���ɂ��A�Â��V�[�����A�����[�h
        SetSceneRenderingEnabled(nextScene, true); // �V�����V�[���̕`����J�n

        AsyncOperation unloadOldScene = SceneManager.UnloadSceneAsync(currentSceneName);
        while (!unloadOldScene.isDone)
        {
            yield return null;
        }

        // 9. ������̍ŏI����

        // ���C��: ����������WaitForFixedUpdate�̒���ɂ��邱�ƂŁAt_pl.Start()��Update()�̎��s���m���Ɍ�����
        yield return new WaitForFixedUpdate();

        if (SceneDataTransfer.Instance != null && playerScriptRef != null)
        {
            // �ۑ����ꂽ���������[�h
            playerScriptRef.LoadDirection(SceneDataTransfer.Instance.playerDirectionToLoad);
            Debug.Log($"�v���C���[�̌����� {SceneDataTransfer.Instance.playerDirectionToLoad} �ɕ������܂����B");
        }

        isSwitchingScene = false;
        Debug.Log($"�V�[���؂�ւ�����: {nextSceneName}�B���̓��͉\�ł��B");
    }
}