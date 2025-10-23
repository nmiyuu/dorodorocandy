using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class TimeTravelController : MonoBehaviour
{
    // --- �V�[�����ݒ� (Inspector�Őݒ�) ---
    public string pastSceneName = "Stage1_Past";
    public string presentSceneName = "Stage1_now";

    private bool isSwitchingScene = false;
    private GameObject playerObject;
    private t_player playerScriptRef;
    private BoxCollider2D playerColliderRef;
    private LayerMask obstacleLayer;

    private bool TrySetPlayerReferences()
    {
        if (playerObject == null)
        {
            // Player��DontDestroyOnLoad�ł͂Ȃ����߁A�V�[�����[�h��ɍČ������K�v
            playerObject = GameObject.FindGameObjectWithTag("Player");
        }

        if (playerObject != null && playerScriptRef == null)
        {
            playerScriptRef = playerObject.GetComponent<t_player>();

            if (playerScriptRef != null)
            {
                playerColliderRef = playerObject.GetComponent<BoxCollider2D>();
                obstacleLayer = playerScriptRef.obstacleLayer;
            }
        }
        return playerObject != null && playerScriptRef != null && playerColliderRef != null;
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
        if (playerScriptRef.IsPlayerMoving) return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            StartCoroutine(TrySwitchTimeLine());
        }
    }

    public IEnumerator TrySwitchTimeLine()
    {
        isSwitchingScene = true;

        Scene currentScene = SceneManager.GetActiveScene();
        string currentSceneName = currentScene.name;
        string nextSceneName = string.Empty;
        Vector3 nextPlayerPosition = playerScriptRef.CurrentTargetPosition;

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
                // �ߋ�������֍s���O�ɁA�u���b�N�̈ʒu��ۑ�
                SceneDataTransfer.Instance.SaveBlockPositions();
            }
        }
        else
        {
            Debug.LogWarning("����`�̃V�[���ł�: " + currentSceneName);
            isSwitchingScene = false;
            yield break;
        }

        // �v���C���[�̕��A�ʒu���f�[�^�]���I�u�W�F�N�g�ɕۑ�
        if (SceneDataTransfer.Instance != null)
        {
            SceneDataTransfer.Instance.playerPositionToLoad = nextPlayerPosition;
        }

        // --- ������ ������y���̂��߂̃��W�b�N�J�n ������ ---

        // 1. �V�����V�[����񓯊��Ń��[�h�i���[�h����������̂�҂j
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

        // 2. �V�����V�[�����A�N�e�B�u�V�[���ɐݒ�
        SceneManager.SetActiveScene(nextScene);

        // 3. �v���C���[�Q�Ƃ��Ď擾 (�V�����V�[���ɂ��邽��)
        // �����Ńv���C���[�I�u�W�F�N�g�ƃX�N���v�g���ď���������܂�
        playerObject = null;
        playerScriptRef = null;
        playerColliderRef = null;
        if (!TrySetPlayerReferences())
        {
            // �v���C���[��������Ȃ������ꍇ�A�G���[���O���o���ď����𒆒f
            Debug.LogError("�V�����V�[���Ńv���C���[�I�u�W�F�N�g��������܂���ł����B");
            // ���̃V�[���ɖ߂��������������ׂ������A�����ł͏������f
            isSwitchingScene = false;
            yield break;
        }

        // 4. ���A�ʒu�̏Փ˃`�F�b�N�i������̑O�Ɉ��S�m�F�j
        // FixedUpdate���I���̂�҂��A�S�Ă̕������肪���������̂�҂�
        yield return new WaitForFixedUpdate();

        // �v���C���[�̐V�������W�ŏ�Q���`�F�b�N
        Collider2D hitCollider = Physics2D.OverlapBox(
            (Vector2)nextPlayerPosition + playerColliderRef.offset,
            playerColliderRef.size,
            0f,
            obstacleLayer
        );

        if (hitCollider != null)
        {
            Debug.LogWarning($"�^�C���g���x�����~: ���A�ʒu({nextPlayerPosition})�ɏ�Q��('{hitCollider.gameObject.name}')������܂��B");

            // �Փ˂��������ꍇ�A�V�������[�h�����V�[�����A�����[�h���A���̃V�[�����A�N�e�B�u�ɖ߂�
            SceneManager.SetActiveScene(currentScene);
            AsyncOperation unloadOp = SceneManager.UnloadSceneAsync(nextScene);
            while (!unloadOp.isDone)
            {
                yield return null;
            }

            isSwitchingScene = false;
            yield break;
        }

        // 5. �Â��V�[����񓯊��ŃA�����[�h�i�����ŉ�ʂ��؂�ւ��j
        // �V�����V�[�������łɕ`�悳��Ă��邽�߁A��������y�������
        AsyncOperation unloadOldScene = SceneManager.UnloadSceneAsync(currentSceneName);
        while (!unloadOldScene.isDone)
        {
            yield return null;
        }

        // 6. ������̍ŏI����
        // �O�̂���1�t���[���҂��A�V�[���؂�ւ���̏����iFutureObstacleController��Start�Ȃǁj����������̂�҂�
        yield return null;

        isSwitchingScene = false;
        Debug.Log($"�V�[���؂�ւ�����: {nextSceneName}�B���̓��͉\�ł��B");
    }
}