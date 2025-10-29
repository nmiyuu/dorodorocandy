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
    private t_pl playerScriptRef; // �X�v���C�g����p (Int�C���f�b�N�X�Ή�)
    private t_player playerMovementScript; // �ړ�����p
    private BoxCollider2D playerColliderRef;
    private LayerMask obstacleLayer;

    // ���C���ς݃��\�b�h: �Q�ƍĎ擾�̃��W�b�N�����P
    private bool TrySetPlayerReferences()
    {
        // �v���C���[�I�u�W�F�N�g�̎Q�Ƃ��m���Ɏ擾/�Ď擾����
        if (playerObject == null)
        {
            // �V�����V�[������uPlayer�v�^�O�����I�u�W�F�N�g��T��
            playerObject = GameObject.FindGameObjectWithTag("Player");
        }

        if (playerObject == null)
        {
            return false;
        }

        // �v���C���[�I�u�W�F�N�g�����������ꍇ�A���ׂẴR���|�[�l���g���Ď擾����
        playerScriptRef = playerObject.GetComponent<t_pl>();
        playerMovementScript = playerObject.GetComponent<t_player>();
        playerColliderRef = playerObject.GetComponent<BoxCollider2D>();

        //  �S�Ă̎Q�Ƃ��擾�ł������`�F�b�N
        bool allReferencesSet = playerScriptRef != null && playerMovementScript != null && playerColliderRef != null;

        if (allReferencesSet)
        {
            obstacleLayer = playerMovementScript.obstacleLayer;
        }
        else
        {
            // �Q�Ƃ�������Ȃ������ꍇ�̏ڍׂȃG���[��
            if (playerScriptRef == null) Debug.LogError($"Player�I�u�W�F�N�g '{playerObject.name}' �� t_pl �X�N���v�g��������܂���B");
            if (playerMovementScript == null) Debug.LogError($"Player�I�u�W�F�N�g '{playerObject.name}' �� t_player �X�N���v�g��������܂���B");
            if (playerColliderRef == null) Debug.LogError($"Player�I�u�W�F�N�g '{playerObject.name}' �� BoxCollider2D ��������܂���B");
        }

        return allReferencesSet;
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
        if (!TrySetPlayerReferences()) return; // �Q�Ƃ��L�����`�F�b�N

        // �ړ����`�F�b�N
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
            //  Vector2����Int�C���f�b�N�X�iCurrentDirectionIndex�j��ۑ�
            SceneDataTransfer.Instance.playerDirectionIndexToLoad = playerScriptRef.CurrentDirectionIndex;
        }

        //  �V�����V�[����񓯊��Ń��[�h
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

        //�V�����V�[���̕`��������ɗ}��
        SetSceneRenderingEnabled(nextScene, false);

        // �V�����V�[�����A�N�e�B�u�V�[���ɐݒ�
        SceneManager.SetActiveScene(nextScene);

        // �u���b�N�̔z�u�ƕ������Z�̈����҂�
        yield return new WaitForFixedUpdate();
        yield return new WaitForEndOfFrame();
        yield return null;

        // �v���C���[�Q�Ƃ̍Ď擾�̂��߂ɎQ�Ƃ��N���A
        playerObject = null;
        playerScriptRef = null;
        playerColliderRef = null;
        playerMovementScript = null;

        if (!TrySetPlayerReferences())
        {
            // �Q�ƍĎ擾���s: �V�[���؂�ւ��𒆎~
            isSwitchingScene = false;
            yield break;
        }

        // �Փ˔���
        if (playerColliderRef != null)
        {
            Collider2D hitCollider = Physics2D.OverlapBox(
                (Vector2)nextPlayerPosition + playerColliderRef.offset,
                playerColliderRef.size,
                0f,
                obstacleLayer
            );

            // �Փ˔����̏���
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

        //  ���������ꍇ�̂݁A�`���L���ɂ��A�Â��V�[�����A�����[�h
        SetSceneRenderingEnabled(nextScene, true);

        AsyncOperation unloadOldScene = SceneManager.UnloadSceneAsync(currentSceneName);
        while (!unloadOldScene.isDone)
        {
            yield return null;
        }

        //  ������̍ŏI����
        yield return new WaitForFixedUpdate();

        if (SceneDataTransfer.Instance != null && playerScriptRef != null)
        {
            // ���C��: Int�C���f�b�N�X�����[�h���At_pl.LoadDirectionIndex()�ŃA�j���[�V�����𕜌�
            playerScriptRef.LoadDirectionIndex(SceneDataTransfer.Instance.playerDirectionIndexToLoad);
            Debug.Log($"�v���C���[�̌����� {SceneDataTransfer.Instance.playerDirectionIndexToLoad} (Int Index) �ɕ������܂����B");
        }

        isSwitchingScene = false;
        Debug.Log($"�V�[���؂�ւ�����: {nextSceneName}�B���̓��͉\�ł��B");
    }
}