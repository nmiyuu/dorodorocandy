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

    // �w���p�[�֐�: �v���C���[�̎Q�Ƃ��m���Ɍ����Đݒ肷�� (�ύX�Ȃ�)
    private bool TrySetPlayerReferences()
    {
        if (playerObject == null)
        {
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
            else
            {
                Debug.LogError($"Player�I�u�W�F�N�g '{playerObject.name}' �� t_player �X�N���v�g���A�^�b�`����Ă��܂���B");
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

    // �X�y�[�X�L�[�ŌĂ΂��V�[���؂�ւ��̃��C�������i�R���[�`�����j
    public IEnumerator TrySwitchTimeLine()
    {
        isSwitchingScene = true;

        // 1. �؂�ւ���V�[���̌���ƃf�[�^�ۑ��̏���
        Scene currentScene = SceneManager.GetActiveScene();
        string currentSceneName = currentScene.name;
        string nextSceneName = string.Empty;
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
                SceneDataTransfer.Instance.SaveBlockPositions();
            }
        }
        else
        {
            Debug.LogWarning("����`�̃V�[���ł�: " + currentSceneName);
            isSwitchingScene = false;
            yield break;
        }

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

        // ������ �C���ӏ�: �������Z�ƃt���[���X�V�̗����̈����҂� ������
        yield return new WaitForFixedUpdate();
        yield return null; // �`��o�b�t�@���؂�ւ��̂��m���ɂ���
        // ������ �����܂ŏC�� ������

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
            // ��Q�����������ꍇ�A�؂�ւ��𒆎~
            Debug.LogWarning($"�^�C���g���x�����~: ���A�ʒu({nextPlayerPosition})�ɏ�Q��('{hitCollider.gameObject.name}')������܂��B");

            // ������ �C���ӏ�: ���̃V�[���ɖ߂�����AUnload������҂� ������
            // ���̃V�[�����A�N�e�B�u����D��
            SceneManager.SetActiveScene(currentScene);

            // UnloadSceneAsync �̊�����҂��ƂŁA���̃V�[�����`�悳���\�������S�ɔr��
            AsyncOperation unloadOp = SceneManager.UnloadSceneAsync(nextScene);
            while (!unloadOp.isDone)
            {
                yield return null;
            }
            // ������ �����܂ŏC�� ������

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
        SceneManager.SetActiveScene(nextScene);

        // 6. �Q�Ƃ̍X�V�ƃt���O�̃��Z�b�g
        StartCoroutine(WaitForPlayerInit(nextSceneName));
    }

    // �V�[�����[�h��̌㏈���ƎQ�Ƃ̍Ď擾 (�ύX�Ȃ�)
    IEnumerator WaitForPlayerInit(string sceneName)
    {
        playerObject = null;
        playerScriptRef = null;
        playerColliderRef = null;

        yield return null;

        if (!TrySetPlayerReferences())
        {
            isSwitchingScene = false;
            yield break;
        }

        isSwitchingScene = false;
        Debug.Log($"�V�[���؂�ւ�����: {sceneName}�B���̓��͉\�ł��B");
    }
}