using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Linq;

public class TimeTravelController : MonoBehaviour
{
    // --- シーン名設定 (Inspectorで設定) ---
    public string pastSceneName = "Stage1_Past";
    public string presentSceneName = "Stage1_now";

    private bool isSwitchingScene = false;
    private GameObject playerObject;
    private t_pl playerScriptRef; // スプライト制御用
    private t_player playerMovementScript; // 移動制御用
    private BoxCollider2D playerColliderRef;
    private LayerMask obstacleLayer;

    // --- Unityライフサイクル ---

    void Start()
    {
        // Startでは参照取得を一度試みる
        TrySetPlayerReferences();
        if (playerObject == null)
        {
            Debug.LogError("タグが 'Player' のオブジェクトが見つかりません。TimeTravelControllerが動作しません。");
        }
    }

    void Update()
    {
        // 1. シーン切り替え中は停止
        if (isSwitchingScene) return;

        // 2. 自分がアクティブシーンに属していないなら処理を停止
        if (gameObject.scene != SceneManager.GetActiveScene())
        {
            return;
        }

        // フェード中チェック: フェードイン/アウト中はタイムトラベルをブロックする
        if (SceneFader.Instance != null && SceneFader.Instance.IsFading)
        {
            return;
        }

        if (!TrySetPlayerReferences()) return; // 参照が有効かチェック

        // 移動中チェック
        if (playerMovementScript.IsPlayerMoving) return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            // Spaceキーが押されたら、白フェードを伴う切り替えを開始
            StartCoroutine(TriggerTimeTravelWithFade());
        }
    }

    // --- メインロジック ---

    // Spaceキーで呼ばれるフェード付きタイムトラベルの起点
    public IEnumerator TriggerTimeTravelWithFade()
    {
        // 1. ロック開始
        isSwitchingScene = true;

        // 2. フェードアウトを開始し、完了を待つ (白フェードを指定)
        if (SceneFader.Instance != null)
        {
            yield return SceneFader.Instance.FadeOut(FadeColor.White);
        }

        // 3. フェードアウト完了後、シーン切り替え本体を実行
        yield return StartCoroutine(ExecuteTimeTravelLogic());

        // 4. シーン切り替え成功後、フェードイン（解除）処理

        // isSwitchingScene が true のまま（つまり切り替えが成功した）場合のみ、フェードインを実行
        if (isSwitchingScene == true && SceneFader.Instance != null)
        {
            // 画面が「白」で覆われているので、FadeInの引数に White を指定する
            yield return SceneFader.Instance.FadeIn(FadeColor.White);
        }
    }

    // フェード処理を含まない、純粋なタイムトラベルの切り替え処理
    private IEnumerator ExecuteTimeTravelLogic()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        string currentSceneName = currentScene.name;
        string nextSceneName = string.Empty;

        // プレイヤーの位置を安全に取得（参照が取れていない場合はここで処理を中断）
        Vector3 nextPlayerPosition;
        if (playerObject != null)
        {
            nextPlayerPosition = playerObject.transform.position;
        }
        else
        {
            Debug.LogError("プレイヤーオブジェクトの参照が取れていません！タイムトラベルを中止します。");
            isSwitchingScene = false; // ★ロック解除ポイント★
            yield break;
        }

        // データの保存と次のシーン名の決定
        if (currentSceneName == presentSceneName)
        {
            nextSceneName = pastSceneName;
        }
        else if (currentSceneName == pastSceneName)
        {
            nextSceneName = presentSceneName;

            // ★ブロックデータは MoveBlock がリアルタイムで更新するため、TimeTravelControllerからのSave呼び出しは不要。★
        }
        else
        {
            Debug.LogWarning("未定義のシーンです: " + currentSceneName);
            isSwitchingScene = false; // ★ロック解除ポイント★
            yield break;
        }

        // プレイヤーの復帰位置と向きをデータ転送オブジェクトに保存
        if (SceneDataTransfer.Instance != null)
        {
            SceneDataTransfer.Instance.playerPositionToLoad = nextPlayerPosition;
            SceneDataTransfer.Instance.playerDirectionIndexToLoad = playerScriptRef.CurrentDirectionIndex;
        }

        // 新しいシーンを非同期でロード
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(nextSceneName, LoadSceneMode.Additive);
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        Scene nextScene = SceneManager.GetSceneByName(nextSceneName);
        if (!nextScene.IsValid())
        {
            Debug.LogError("シーンの追加ロードに失敗しました: " + nextSceneName);
            isSwitchingScene = false; // ★ロック解除ポイント★
            yield break;
        }

        // 新しいシーンの描画をすぐに抑制
        SetSceneRenderingEnabled(nextScene, false);

        // 新しいシーンをアクティブシーンに設定
        SceneManager.SetActiveScene(nextScene);

        // ブロックの配置と物理演算の安定を待つ
        yield return new WaitForFixedUpdate();
        yield return new WaitForEndOfFrame();
        yield return null;

        // プレイヤー参照の再取得のために参照をクリア
        playerObject = null;
        playerScriptRef = null;
        playerColliderRef = null;
        playerMovementScript = null;

        if (!TrySetPlayerReferences())
        {
            // 参照再取得失敗: シーン切り替えを中止
            isSwitchingScene = false; // ★ロック解除ポイント★
            yield break;
        }

        // 衝突判定
        if (playerColliderRef != null)
        {
            LayerMask currentObstacleLayer = playerMovementScript.obstacleLayer;

            Collider2D hitCollider = Physics2D.OverlapBox(
                (Vector2)nextPlayerPosition + playerColliderRef.offset,
                playerColliderRef.size,
                0f,
                currentObstacleLayer
            );

            // 衝突判定後の処理
            if (hitCollider != null)
            {
                Debug.LogWarning($"タイムトラベル中止: 復帰位置({nextPlayerPosition})に障害物('{hitCollider.gameObject.name}')があります。");

                SceneManager.SetActiveScene(currentScene);

                // シーンが有効かつロードされているかを確認してからアンロード
                if (nextScene.IsValid() && nextScene.isLoaded)
                {
                    AsyncOperation unloadOp = SceneManager.UnloadSceneAsync(nextScene);
                    while (!unloadOp.isDone)
                    {
                        yield return null;
                    }
                }

                // 衝突によりキャンセルされたので、白画面を解除する！
                if (SceneFader.Instance != null)
                {
                    yield return SceneFader.Instance.FadeIn(FadeColor.White);
                }

                yield return null;
                isSwitchingScene = false; // ★ロック解除ポイント★
                Debug.Log("切り替えがキャンセルされました。");
                yield break;
            }
        }

        // 成功した場合のみ、描画を有効にし、古いシーンをアンロード
        SetSceneRenderingEnabled(nextScene, true);

        // 古いシーンのアンロード前にロードされているかチェック
        if (currentScene.IsValid() && currentScene.isLoaded)
        {
            AsyncOperation unloadOldScene = SceneManager.UnloadSceneAsync(currentSceneName);
            while (!unloadOldScene.isDone)
            {
                yield return null;
            }
        }

        // 完了後の最終処理
        yield return new WaitForFixedUpdate();

        if (SceneDataTransfer.Instance != null && playerScriptRef != null)
        {
            // プレイヤーの向きを復元
            playerScriptRef.LoadDirectionIndex(SceneDataTransfer.Instance.playerDirectionIndexToLoad);
            Debug.Log($"プレイヤーの向きを {SceneDataTransfer.Instance.playerDirectionIndexToLoad} (Int Index) に復元しました。");
        }

        isSwitchingScene = false; // ★成功時のロック解除ポイント★
        Debug.Log($"シーン切り替え完了: {nextSceneName}。次の入力可能です。");
    }

    // シーン内のレンダラーの有効/無効を切り替える
    private void SetSceneRenderingEnabled(Scene scene, bool isEnabled)
    {
        if (!scene.IsValid() || !scene.isLoaded)
        {
            Debug.LogWarning($"[TimeTravel] 無効なシーン '{scene.name}' のレンダリング設定をスキップします。");
            return;
        }

        foreach (GameObject rootObject in scene.GetRootGameObjects())
        {
            foreach (Renderer renderer in rootObject.GetComponentsInChildren<Renderer>(true))
            {
                renderer.enabled = isEnabled;
            }
        }
    }

    // プレイヤーの参照を確実に取得/再取得する
    private bool TrySetPlayerReferences()
    {
        if (playerObject == null)
        {
            playerObject = GameObject.FindGameObjectWithTag("Player");
        }

        if (playerObject == null)
        {
            return false;
        }

        playerScriptRef = playerObject.GetComponent<t_pl>();
        playerMovementScript = playerObject.GetComponent<t_player>();
        playerColliderRef = playerObject.GetComponent<BoxCollider2D>();

        bool allReferencesSet = playerScriptRef != null && playerMovementScript != null && playerColliderRef != null;

        if (allReferencesSet)
        {
            obstacleLayer = playerMovementScript.obstacleLayer;
        }

        return allReferencesSet;
    }

    void OnDestroy()
    {
        Debug.Log($"[TimeTravelController: {gameObject.name}] は正常に破棄されました。", this);
    }
}