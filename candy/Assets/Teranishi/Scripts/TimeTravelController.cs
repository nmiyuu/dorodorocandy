using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class TimeTravelController : MonoBehaviour
{
    // --- シーン名設定 (Inspectorで設定) ---
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
            // PlayerはDontDestroyOnLoadではないため、シーンロード後に再検索が必要
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
            Debug.LogError("タグが 'Player' のオブジェクトが見つかりません。TimeTravelControllerが動作しません。");
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

        // データの保存と次のシーン名の決定
        if (currentSceneName == presentSceneName)
        {
            nextSceneName = pastSceneName;
        }
        else if (currentSceneName == pastSceneName)
        {
            nextSceneName = presentSceneName;
            if (SceneDataTransfer.Instance != null)
            {
                // 過去→現代へ行く前に、ブロックの位置を保存
                SceneDataTransfer.Instance.SaveBlockPositions();
            }
        }
        else
        {
            Debug.LogWarning("未定義のシーンです: " + currentSceneName);
            isSwitchingScene = false;
            yield break;
        }

        // プレイヤーの復帰位置をデータ転送オブジェクトに保存
        if (SceneDataTransfer.Instance != null)
        {
            SceneDataTransfer.Instance.playerPositionToLoad = nextPlayerPosition;
        }

        // --- ★★★ ちらつき軽減のためのロジック開始 ★★★ ---

        // 1. 新しいシーンを非同期でロード（ロードが完了するのを待つ）
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(nextSceneName, LoadSceneMode.Additive);
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        Scene nextScene = SceneManager.GetSceneByName(nextSceneName);
        if (!nextScene.IsValid())
        {
            Debug.LogError("シーンの追加ロードに失敗しました: " + nextSceneName);
            isSwitchingScene = false;
            yield break;
        }

        // 2. 新しいシーンをアクティブシーンに設定
        SceneManager.SetActiveScene(nextScene);

        // 3. プレイヤー参照を再取得 (新しいシーンにいるため)
        // ここでプレイヤーオブジェクトとスクリプトが再初期化されます
        playerObject = null;
        playerScriptRef = null;
        playerColliderRef = null;
        if (!TrySetPlayerReferences())
        {
            // プレイヤーが見つからなかった場合、エラーログを出して処理を中断
            Debug.LogError("新しいシーンでプレイヤーオブジェクトが見つかりませんでした。");
            // 元のシーンに戻す処理も検討すべきだが、ここでは処理中断
            isSwitchingScene = false;
            yield break;
        }

        // 4. 復帰位置の衝突チェック（ちらつきの前に安全確認）
        // FixedUpdateが終わるのを待ち、全ての物理判定が落ち着くのを待つ
        yield return new WaitForFixedUpdate();

        // プレイヤーの新しい座標で障害物チェック
        Collider2D hitCollider = Physics2D.OverlapBox(
            (Vector2)nextPlayerPosition + playerColliderRef.offset,
            playerColliderRef.size,
            0f,
            obstacleLayer
        );

        if (hitCollider != null)
        {
            Debug.LogWarning($"タイムトラベル中止: 復帰位置({nextPlayerPosition})に障害物('{hitCollider.gameObject.name}')があります。");

            // 衝突があった場合、新しくロードしたシーンをアンロードし、元のシーンをアクティブに戻す
            SceneManager.SetActiveScene(currentScene);
            AsyncOperation unloadOp = SceneManager.UnloadSceneAsync(nextScene);
            while (!unloadOp.isDone)
            {
                yield return null;
            }

            isSwitchingScene = false;
            yield break;
        }

        // 5. 古いシーンを非同期でアンロード（ここで画面が切り替わる）
        // 新しいシーンがすでに描画されているため、ちらつきが軽減される
        AsyncOperation unloadOldScene = SceneManager.UnloadSceneAsync(currentSceneName);
        while (!unloadOldScene.isDone)
        {
            yield return null;
        }

        // 6. 完了後の最終処理
        // 念のため1フレーム待ち、シーン切り替え後の処理（FutureObstacleControllerのStartなど）が完了するのを待つ
        yield return null;

        isSwitchingScene = false;
        Debug.Log($"シーン切り替え完了: {nextSceneName}。次の入力可能です。");
    }
}