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

    // ヘルパー関数: プレイヤーの参照を確実に見つけて設定する
    private bool TrySetPlayerReferences()
    {
        // 1. playerObjectがnullの場合に、タグで再検索する
        if (playerObject == null)
        {
            playerObject = GameObject.FindGameObjectWithTag("Player");
        }

        // 2. playerObjectが見つかり、かつplayerScriptRefがnullの場合に、GetComponentする
        if (playerObject != null && playerScriptRef == null)
        {
            playerScriptRef = playerObject.GetComponent<t_player>();

            if (playerScriptRef != null)
            {
                // t_playerが見つかったら、ColliderとLayerMaskを取得
                playerColliderRef = playerObject.GetComponent<BoxCollider2D>();

                // t_player.csにpublic LayerMask obstacleLayerが定義されている前提
                obstacleLayer = playerScriptRef.obstacleLayer;
            }
            else
            {
                // タグが'Player'のオブジェクトは見つかったが、t_playerスクリプトがない場合
                Debug.LogError($"Playerオブジェクト '{playerObject.name}' に t_player スクリプトがアタッチされていません。");
            }
        }

        // すべての参照が有効な場合のみ true を返す
        return playerObject != null && playerScriptRef != null && playerColliderRef != null;
    }

    void Start()
    {
        // ゲーム開始時に一度だけ参照の取得を試みる
        TrySetPlayerReferences();

        if (playerObject == null)
        {
            Debug.LogError("タグが 'Player' のオブジェクトが見つかりません。TimeTravelControllerが動作しません。");
        }
    }

    void Update()
    {
        // 1. シーン切り替え中は入力をスキップ
        if (isSwitchingScene) return;

        // 2. プレイヤーの参照が有効か確認し、無効なら取得を試みる
        if (!TrySetPlayerReferences())
        {
            return;
        }

        // 3. プレイヤーが移動中の場合は入力を無視する (t_player.IsPlayerMoving が必要)
        if (playerScriptRef.IsPlayerMoving)
        {
            return;
        }

        // 4. スペースキーが押されたかチェック
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // 非同期コルーチンでシーン切り替えとチェックを実行
            StartCoroutine(TrySwitchTimeLine());
        }
    }

    // スペースキーで呼ばれるシーン切り替えのメイン処理（コルーチン化）
    public IEnumerator TrySwitchTimeLine()
    {
        isSwitchingScene = true;

        // 1. 切り替え先シーンの決定とデータ保存の準備
        Scene currentScene = SceneManager.GetActiveScene(); // 現在のシーンオブジェクトを取得
        string currentSceneName = currentScene.name;
        string nextSceneName = string.Empty;

        // プレイヤーの「次に目指す位置」（マス目の中心座標）を取得
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

        // プレイヤーの位置をデータ転送オブジェクトに保存
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
        yield return null; // 新しいシーンのAwake/Start実行を待つ

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
            // 障害物があった場合、切り替えを中止し、一時ロードしたシーンをアンロード
            Debug.LogWarning($"タイムトラベル中止: 復帰位置({nextPlayerPosition})に障害物('{hitCollider.gameObject.name}')があります。");

            // 一時的にロードしたシーンをアンロード
            SceneManager.UnloadSceneAsync(nextScene);
            SceneManager.SetActiveScene(currentScene); // 元のシーンをアクティブに戻す
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
        // これがUnityに「このシーンがメインです」と伝達する重要な処理
        SceneManager.SetActiveScene(nextScene);

        // 6. 参照の更新とフラグのリセット
        StartCoroutine(WaitForPlayerInit(nextSceneName));
    }

    // シーンロード後の後処理と参照の再取得
    IEnumerator WaitForPlayerInit(string sceneName)
    {
        // STEP 1: プレイヤーオブジェクトの参照を強制的に解除
        // これにより、Update内の TrySetPlayerReferences() が再取得を強制される
        playerObject = null;
        playerScriptRef = null;
        playerColliderRef = null;

        // STEP 2: 新しいシーンの初期化（Awake/Start）の完了を待つための待機
        yield return null;

        // STEP 3: 参照を再取得
        if (!TrySetPlayerReferences())
        {
            isSwitchingScene = false;
            yield break;
        }

        // 参照が更新され、次の入力が可能な状態になる
        isSwitchingScene = false;
        Debug.Log($"シーン切り替え完了: {sceneName}。次の入力可能です。");
    }
}