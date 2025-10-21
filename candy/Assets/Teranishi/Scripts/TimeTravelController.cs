using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class TimeTravelController : MonoBehaviour
{
    // --- �V�[�����ݒ� (Inspector�Őݒ�) ---
    public string pastSceneName = "Stage1_Past";
    public string presentSceneName = "Stage1_now";

    // �V�[���؂�ւ����͓��͂��󂯕t���Ȃ����߂̃t���O
    private bool isSwitchingScene = false;
    private GameObject playerObject;
    private t_player playerScriptRef;      // t_player�X�N���v�g�ւ̈��肵���Q��
    private BoxCollider2D playerColliderRef; // �v���C���[��Collider�Q��
    private LayerMask obstacleLayer;          // �v���C���[�̎���Q�����C���[

    // �w���p�[�֐�: �v���C���[�̎Q�Ƃ��m���Ɍ����Đݒ肷��
    private bool TrySetPlayerReferences()
    {
        // 1. playerObject��null�̏ꍇ�ɁA�^�O�ōČ�������
        if (playerObject == null)
        {
            playerObject = GameObject.FindGameObjectWithTag("Player");
        }

        // 2. playerObject��������A����playerScriptRef��null�̏ꍇ�ɁAGetComponent����
        if (playerObject != null && playerScriptRef == null)
        {
            playerScriptRef = playerObject.GetComponent<t_player>();

            if (playerScriptRef != null)
            {
                // t_player������������ACollider��LayerMask���擾
                playerColliderRef = playerObject.GetComponent<BoxCollider2D>();

                // t_player.cs��public LayerMask obstacleLayer����`����Ă���O��
                obstacleLayer = playerScriptRef.obstacleLayer;
            }
            else
            {
                // �^�O��'Player'�̃I�u�W�F�N�g�͌����������At_player�X�N���v�g���Ȃ��ꍇ
                Debug.LogError($"Player�I�u�W�F�N�g '{playerObject.name}' �� t_player �X�N���v�g���A�^�b�`����Ă��܂���B");
            }
        }

        // ���ׂĂ̎Q�Ƃ��L���ȏꍇ�̂� true ��Ԃ�
        return playerObject != null && playerScriptRef != null && playerColliderRef != null;
    }

    void Start()
    {
        // �Q�[���J�n���Ɉ�x�����Q�Ƃ̎擾�����݂�
        TrySetPlayerReferences();

        if (playerObject == null)
        {
            Debug.LogError("�^�O�� 'Player' �̃I�u�W�F�N�g��������܂���BTimeTravelController�����삵�܂���B");
        }
    }

    void Update()
    {
        // 1. �V�[���؂�ւ����͓��͂��X�L�b�v
        if (isSwitchingScene) return;

        // 2. �v���C���[�̎Q�Ƃ��L�����m�F���A�����Ȃ�擾�����݂�
        if (!TrySetPlayerReferences())
        {
            return;
        }

        // 3. �v���C���[���ړ����̏ꍇ�͓��͂𖳎����� (t_player.IsPlayerMoving ���K�v)
        if (playerScriptRef.IsPlayerMoving)
        {
            return;
        }

        // 4. �X�y�[�X�L�[�������ꂽ���`�F�b�N
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // �񓯊��R���[�`���ŃV�[���؂�ւ��ƃ`�F�b�N�����s
            StartCoroutine(TrySwitchTimeLine());
        }
    }

    // �X�y�[�X�L�[�ŌĂ΂��V�[���؂�ւ��̃��C�������i�R���[�`�����j
    public IEnumerator TrySwitchTimeLine()
    {
        isSwitchingScene = true;

        // 1. �؂�ւ���V�[���̌���ƃf�[�^�ۑ��̏���
        Scene currentScene = SceneManager.GetActiveScene(); // ���݂̃V�[���I�u�W�F�N�g���擾
        string currentSceneName = currentScene.name;
        string nextSceneName = string.Empty;

        // �v���C���[�́u���ɖڎw���ʒu�v�i�}�X�ڂ̒��S���W�j���擾
        Vector3 nextPlayerPosition = playerScriptRef.CurrentTargetPosition;

        // �f�[�^�ۑ��̏���
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

        // �v���C���[�̈ʒu���f�[�^�]���I�u�W�F�N�g�ɕۑ�
        if (SceneDataTransfer.Instance != null)
        {
            SceneDataTransfer.Instance.playerPositionToLoad = nextPlayerPosition;
        }

        // 2. ���̃V�[�����ꎞ�I�ɒǉ����[�h���A��Q���`�F�b�N���s��

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

        // 3. ��Q���`�F�b�N�̎��s

        // �Փ˔���̂��߂Ɉꎞ�I�Ɏ��̃V�[�����A�N�e�B�u�ɂ���
        SceneManager.SetActiveScene(nextScene);
        yield return null; // �V�����V�[����Awake/Start���s��҂�

        // OverlapBox�����̕��A�ʒu�Ŏ��s
        Collider2D hitCollider = Physics2D.OverlapBox(
            (Vector2)nextPlayerPosition + playerColliderRef.offset,
            playerColliderRef.size,
            0f,
            obstacleLayer
        );

        // 4. �Փˌ��ʂɊ�Â�����
        if (hitCollider != null)
        {
            // ��Q�����������ꍇ�A�؂�ւ��𒆎~���A�ꎞ���[�h�����V�[�����A�����[�h
            Debug.LogWarning($"�^�C���g���x�����~: ���A�ʒu({nextPlayerPosition})�ɏ�Q��('{hitCollider.gameObject.name}')������܂��B");

            // �ꎞ�I�Ƀ��[�h�����V�[�����A�����[�h
            SceneManager.UnloadSceneAsync(nextScene);
            SceneManager.SetActiveScene(currentScene); // ���̃V�[�����A�N�e�B�u�ɖ߂�
            isSwitchingScene = false;
            yield break; // �R���[�`�����I��
        }

        // ��Q�����Ȃ��ꍇ�A�V�[���؂�ւ��𑱍s

        // 5. �V�[���̐؂�ւ������s

        // �Â��V�[�����A�����[�h
        AsyncOperation unloadOldScene = SceneManager.UnloadSceneAsync(currentSceneName);
        while (!unloadOldScene.isDone)
        {
            yield return null;
        }

        // �A�����[�h������������A�V�����V�[�����A�N�e�B�u�ɐݒ�
        // ���ꂪUnity�Ɂu���̃V�[�������C���ł��v�Ɠ`�B����d�v�ȏ���
        SceneManager.SetActiveScene(nextScene);

        // 6. �Q�Ƃ̍X�V�ƃt���O�̃��Z�b�g
        StartCoroutine(WaitForPlayerInit(nextSceneName));
    }

    // �V�[�����[�h��̌㏈���ƎQ�Ƃ̍Ď擾
    IEnumerator WaitForPlayerInit(string sceneName)
    {
        // STEP 1: �v���C���[�I�u�W�F�N�g�̎Q�Ƃ������I�ɉ���
        // ����ɂ��AUpdate���� TrySetPlayerReferences() ���Ď擾�����������
        playerObject = null;
        playerScriptRef = null;
        playerColliderRef = null;

        // STEP 2: �V�����V�[���̏������iAwake/Start�j�̊�����҂��߂̑ҋ@
        yield return null;

        // STEP 3: �Q�Ƃ��Ď擾
        if (!TrySetPlayerReferences())
        {
            isSwitchingScene = false;
            yield break;
        }

        // �Q�Ƃ��X�V����A���̓��͂��\�ȏ�ԂɂȂ�
        isSwitchingScene = false;
        Debug.Log($"�V�[���؂�ւ�����: {sceneName}�B���̓��͉\�ł��B");
    }
}