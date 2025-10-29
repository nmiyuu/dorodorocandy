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
    private t_pl playerScriptRef; // スプライト制御用 (Intインデックス対応)
    private t_player playerMovementScript; // 移動制御用
    private BoxCollider2D playerColliderRef;
    private LayerMask obstacleLayer;

    // ★修正済みメソッド: 参照再取得のロジックを改善
    private bool TrySetPlayerReferences()
    {
        // プレイヤーオブジェクトの参照を確実に取得/再取得する
        if (playerObject == null)
        {
            // 新しいシーンから「Player」タグを持つオブジェクトを探す
            playerObject = GameObject.FindGameObjectWithTag("Player");
        }

        if (playerObject == null)
        {
            return false;
        }

        // プレイヤーオブジェクトが見つかった場合、すべてのコンポーネントを再取得する
        playerScriptRef = playerObject.GetComponent<t_pl>();
        playerMovementScript = playerObject.GetComponent<t_player>();
        playerColliderRef = playerObject.GetComponent<BoxCollider2D>();

        //  全ての参照が取得できたかチェック
        bool allReferencesSet = playerScriptRef != null && playerMovementScript != null && playerColliderRef != null;

        if (allReferencesSet)
        {
            obstacleLayer = playerMovementScript.obstacleLayer;
        }
        else
        {
            // 参照が見つからなかった場合の詳細なエラー報告
            if (playerScriptRef == null) Debug.LogError($"Playerオブジェクト '{playerObject.name}' に t_pl スクリプトが見つかりません。");
            if (playerMovementScript == null) Debug.LogError($"Playerオブジェクト '{playerObject.name}' に t_player スクリプトが見つかりません。");
            if (playerColliderRef == null) Debug.LogError($"Playerオブジェクト '{playerObject.name}' に BoxCollider2D が見つかりません。");
        }

        return allReferencesSet;
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
        if (!TrySetPlayerReferences()) return; // 参照が有効かチェック

        // 移動中チェック
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

        // コルーチン開始直後にもう一度移動中チェック
        if (playerMovementScript.IsPlayerMoving)
        {
            isSwitchingScene = false;
            yield break;
        }

        Scene currentScene = SceneManager.GetActiveScene();
        string currentSceneName = currentScene.name;
        string nextSceneName = string.Empty;

        Vector3 nextPlayerPosition = playerObject.transform.position;

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
                SceneDataTransfer.Instance.SaveBlockPositions();
            }
        }
        else
        {
            Debug.LogWarning("未定義のシーンです: " + currentSceneName);
            isSwitchingScene = false;
            yield break;
        }

        // プレイヤーの復帰位置と向きをデータ転送オブジェクトに保存
        if (SceneDataTransfer.Instance != null)
        {
            SceneDataTransfer.Instance.playerPositionToLoad = nextPlayerPosition;
            //  Vector2からIntインデックス（CurrentDirectionIndex）を保存
            SceneDataTransfer.Instance.playerDirectionIndexToLoad = playerScriptRef.CurrentDirectionIndex;
        }

        //  新しいシーンを非同期でロード
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

        //新しいシーンの描画をすぐに抑制
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
            isSwitchingScene = false;
            yield break;
        }

        // 衝突判定
        if (playerColliderRef != null)
        {
            Collider2D hitCollider = Physics2D.OverlapBox(
                (Vector2)nextPlayerPosition + playerColliderRef.offset,
                playerColliderRef.size,
                0f,
                obstacleLayer
            );

            // 衝突判定後の処理
            if (hitCollider != null)
            {
                Debug.LogWarning($"タイムトラベル中止: 復帰位置({nextPlayerPosition})に障害物('{hitCollider.gameObject.name}')があります。");

                SceneManager.SetActiveScene(currentScene);

                AsyncOperation unloadOp = SceneManager.UnloadSceneAsync(nextScene);
                while (!unloadOp.isDone)
                {
                    yield return null;
                }

                yield return null;

                isSwitchingScene = false;
                Debug.Log("切り替えがキャンセルされました。");
                yield break;
            }
        }

        //  成功した場合のみ、描画を有効にし、古いシーンをアンロード
        SetSceneRenderingEnabled(nextScene, true);

        AsyncOperation unloadOldScene = SceneManager.UnloadSceneAsync(currentSceneName);
        while (!unloadOldScene.isDone)
        {
            yield return null;
        }

        //  完了後の最終処理
        yield return new WaitForFixedUpdate();

        if (SceneDataTransfer.Instance != null && playerScriptRef != null)
        {
            // ★修正: Intインデックスをロードし、t_pl.LoadDirectionIndex()でアニメーションを復元
            playerScriptRef.LoadDirectionIndex(SceneDataTransfer.Instance.playerDirectionIndexToLoad);
            Debug.Log($"プレイヤーの向きを {SceneDataTransfer.Instance.playerDirectionIndexToLoad} (Int Index) に復元しました。");
        }

        isSwitchingScene = false;
        Debug.Log($"シーン切り替え完了: {nextSceneName}。次の入力可能です。");
    }
}