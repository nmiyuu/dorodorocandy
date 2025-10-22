using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class TimeTravelController : MonoBehaviour
{
    // --- シーン名設定 (Inspectorで設定) ---
    public string pastSceneName = "Stage1_Past";
    public string presentSceneName = "Stage1_now";

    // シーン切り替え中は入力を受け付けないためのフラグ
    private bool isSwitchingScene = false;
    private GameObject playerObject;
    private t_player playerScriptRef;      // t_playerスクリプトへの安定した参照
    private BoxCollider2D playerColliderRef; // プレイヤーのCollider参照
    private LayerMask obstacleLayer;          // プレイヤーの持つ障害物レイヤー

    // ヘルパー関数: プレイヤーの参照を確実に見つけて設定する (変更なし)
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
                Debug.LogError($"Playerオブジェクト '{playerObject.name}' に t_player スクリプトがアタッチされていません。");
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

    // スペースキーで呼ばれるシーン切り替えのメイン処理（コルーチン化）
    public IEnumerator TrySwitchTimeLine()
    {
        isSwitchingScene = true;

        // 1. 切り替え先シーンの決定とデータ保存の準備
        Scene currentScene = SceneManager.GetActiveScene();
        string currentSceneName = currentScene.name;
        string nextSceneName = string.Empty;
        Vector3 nextPlayerPosition = playerScriptRef.CurrentTargetPosition;

        // データ保存の処理
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
            Debug.LogWarning("未定義のシーンです: " + currentSceneName);
            isSwitchingScene = false;
            yield break;
        }

        if (SceneDataTransfer.Instance != null)
        {
            SceneDataTransfer.Instance.playerPositionToLoad = nextPlayerPosition;
        }

        // 2. 次のシーンを一時的に追加ロードし、障害物チェックを行う
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

        // 3. 障害物チェックの実行
        // 衝突判定のために一時的に次のシーンをアクティブにする
        SceneManager.SetActiveScene(nextScene);

        // ★★★ 修正箇所: 物理演算とフレーム更新の両方の安定を待つ ★★★
        yield return new WaitForFixedUpdate();
        yield return null; // 描画バッファが切り替わるのを確実にする
        // ★★★ ここまで修正 ★★★

        // OverlapBoxを次の復帰位置で実行
        Collider2D hitCollider = Physics2D.OverlapBox(
            (Vector2)nextPlayerPosition + playerColliderRef.offset,
            playerColliderRef.size,
            0f,
            obstacleLayer
        );

        // 4. 衝突結果に基づく処理
        if (hitCollider != null)
        {
            // 障害物があった場合、切り替えを中止
            Debug.LogWarning($"タイムトラベル中止: 復帰位置({nextPlayerPosition})に障害物('{hitCollider.gameObject.name}')があります。");

            // ★★★ 修正箇所: 元のシーンに戻した後、Unload完了を待つ ★★★
            // 元のシーンをアクティブ化を優先
            SceneManager.SetActiveScene(currentScene);

            // UnloadSceneAsync の完了を待つことで、次のシーンが描画される可能性を完全に排除
            AsyncOperation unloadOp = SceneManager.UnloadSceneAsync(nextScene);
            while (!unloadOp.isDone)
            {
                yield return null;
            }
            // ★★★ ここまで修正 ★★★

            isSwitchingScene = false;
            yield break; // コルーチンを終了
        }

        // 障害物がない場合、シーン切り替えを続行

        // 5. シーンの切り替えを実行

        // 古いシーンをアンロード
        AsyncOperation unloadOldScene = SceneManager.UnloadSceneAsync(currentSceneName);
        while (!unloadOldScene.isDone)
        {
            yield return null;
        }

        // アンロードが完了したら、新しいシーンをアクティブに設定
        SceneManager.SetActiveScene(nextScene);

        // 6. 参照の更新とフラグのリセット
        StartCoroutine(WaitForPlayerInit(nextSceneName));
    }

    // シーンロード後の後処理と参照の再取得 (変更なし)
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
        Debug.Log($"シーン切り替え完了: {sceneName}。次の入力可能です。");
    }
}