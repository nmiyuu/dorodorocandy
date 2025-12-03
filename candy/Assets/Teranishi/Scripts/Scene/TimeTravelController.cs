using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Linq;

public class TimeTravelController : MonoBehaviour
{
    // --- シーン名設定 (Inspectorで設定) ---
    [Header("シーン設定")]
    public string pastSceneName = "Stage1_Past";
    public string presentSceneName = "Stage1_now";

    // ★★★ 時代切替SE設定 ★★★
    [Header("時代切替SE")]
    [Tooltip("時代切替が成功する時に再生するオーディオクリップ (フェード開始時)")]
    public AudioClip timeTravelSuccessSE;

    [Tooltip("障害物があり切り替えに失敗した時に再生するオーディオクリップ")]
    public AudioClip timeTravelFailureSE;

    private AudioSource audioSource;
    // ★★★★★★★★★★★★★

    private GameObject playerObject;
    private t_pl playerScriptRef;             // スプライト制御用
    private t_player playerMovementScript;    // 移動制御用
    private BoxCollider2D playerColliderRef;
    private LayerMask obstacleLayer;

    // --- Unityライフサイクル ---

    void Awake()
    {
        // AudioSource の初期化。このオブジェクトに AudioSource が必要
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            // なければ追加する
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
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
        // 1. SceneDataTransfer が初期化されていない、またはシーン切り替え中は停止
        if (SceneDataTransfer.Instance == null || SceneDataTransfer.Instance.isChangingScene)
        {
            return;
        }

        // 2. 自分がアクティブシーンに属していないなら処理を停止 (Additive Load時の保険)
        if (gameObject.scene != SceneManager.GetActiveScene())
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
        if (SceneDataTransfer.Instance != null)
        {
            SceneDataTransfer.Instance.StartSceneChange();
        }
        else
        {
            Debug.LogError("SceneDataTransfer が見つかりません。タイムトラベルを中止します。");
            if (SceneDataTransfer.Instance != null)
                SceneDataTransfer.Instance.EndSceneChange();
            yield break;
        }

        // 成功時のSE再生: ロック直後に鳴らす
        if (audioSource != null && timeTravelSuccessSE != null)
        {
            audioSource.PlayOneShot(timeTravelSuccessSE);
        }

        // 3. フェードアウトを開始し、完了を待つ (白フェードを指定)
        if (SceneFader.Instance != null)
        {
            yield return SceneFader.Instance.FadeOut(FadeColor.White);
        }

        // 4. フェードアウト完了後、シーン切り替え本体を実行
        bool success = false;
        yield return StartCoroutine(ExecuteTimeTravelLogic(result => success = result));

        // 5. シーン切り替え成功/失敗後の処理
        if (success)
        {
            // 成功時: SceneFaderの FadeInAfterLoad で EndSceneChange が呼ばれるため、ここでは何もしない
        }
        else // 失敗/キャンセル時
        {
            // ExecuteTimeTravelLogic()内で失敗(衝突)が検出された場合、ここで処理する

            // 白画面を解除する (FadeIn)
            if (SceneFader.Instance != null)
            {
                yield return SceneFader.Instance.FadeIn(FadeColor.White);
            }

            // ロック解除
            if (SceneDataTransfer.Instance != null)
            {
                SceneDataTransfer.Instance.EndSceneChange();
            }

            Debug.Log("タイムトラベルがキャンセルされました。");
        }
    }

    // フェード処理を含まない、純粋なタイムトラベルの切り替え処理
    private IEnumerator ExecuteTimeTravelLogic(System.Action<bool> setResult)
    {
        Scene currentScene = SceneManager.GetActiveScene();
        string currentSceneName = currentScene.name;
        string nextSceneName = string.Empty;

        // プレイヤーの位置を安全に取得
        Vector3 nextPlayerPosition;
        if (playerObject != null)
        {
            nextPlayerPosition = playerObject.transform.position;
        }
        else
        {
            Debug.LogError("プレイヤーオブジェクトの参照が取れていません！タイムトラベルを中止します。");
            setResult(false);
            yield break;
        }

        // 次のシーン名の決定
        if (currentSceneName == presentSceneName)
        {
            nextSceneName = pastSceneName;
        }
        else if (currentSceneName == pastSceneName)
        {
            nextSceneName = presentSceneName;
        }
        else
        {
            Debug.LogWarning("未定義のシーンです: " + currentSceneName);
            setResult(false);
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
            setResult(false);
            yield break;
        }

        // 1. 新しいシーンの描画をすぐに抑制
        SetSceneRenderingEnabled(nextScene, false);

        // 2. 新しいシーンをアクティブシーンに設定
        SceneManager.SetActiveScene(nextScene);

        // 3. ブロックの配置と物理演算の安定を待つ
        yield return new WaitForFixedUpdate();
        yield return new WaitForEndOfFrame();

        // 4. プレイヤー参照の再取得
        playerObject = null;
        if (!TrySetPlayerReferences())
        {
            Debug.LogError("新しいシーンでプレイヤーの参照再取得に失敗しました。");

            // 失敗時はアンロードとアクティブシーンの復元
            SceneManager.SetActiveScene(currentScene);
            if (nextScene.IsValid() && nextScene.isLoaded)
            {
                SceneManager.UnloadSceneAsync(nextScene);
            }
            setResult(false);
            yield break;
        }

        // 5. 衝突判定の実行
        if (playerColliderRef != null)
        {
            LayerMask currentObstacleLayer = playerMovementScript.obstacleLayer;

            // 復帰位置に障害物がないかチェック
            Collider2D hitCollider = Physics2D.OverlapBox(
                (Vector2)nextPlayerPosition + playerColliderRef.offset,
                playerColliderRef.size * 0.9f,
                0f,
                currentObstacleLayer
            );

            // 衝突判定後の処理 (キャンセルロジック)
            if (hitCollider != null)
            {
                Debug.LogWarning($"タイムトラベル中止: 復帰位置に障害物('{hitCollider.gameObject.name}')があります。");

                // ★★★ 失敗SE再生処理 ★★★
                if (audioSource != null && timeTravelFailureSE != null)
                {
                    // 成功SEを中断し、失敗SEを鳴らす
                    audioSource.Stop();
                    audioSource.PlayOneShot(timeTravelFailureSE);
                }
                // ★★★★★★★★★★★★★★★★★

                // アクティブシーンを元に戻す
                SceneManager.SetActiveScene(currentScene);

                // 新しいシーンをアンロード
                if (nextScene.IsValid() && nextScene.isLoaded)
                {
                    SceneManager.UnloadSceneAsync(nextScene);
                }

                setResult(false); // 失敗を親コルーチンに通知
                yield break;
            }
        }

        // 6. 成功時の最終処理: 古いシーンのアンロード

        // 描画を有効にする 
        SetSceneRenderingEnabled(nextScene, true);

        // 古いシーンのアンロード
        if (currentScene.IsValid() && currentScene.isLoaded)
        {
            SceneManager.UnloadSceneAsync(currentSceneName);
        }

        // プレイヤーの向きを復元
        if (SceneDataTransfer.Instance != null && playerScriptRef != null)
        {
            playerScriptRef.LoadDirectionIndex(SceneDataTransfer.Instance.playerDirectionIndexToLoad);
            Debug.Log($"プレイヤーの向きを {SceneDataTransfer.Instance.playerDirectionIndexToLoad} (Int Index) に復元しました。");
        }

        // 成功を親コルーチンに通知
        setResult(true);
        Debug.Log($"シーン切り替え成功: {nextSceneName}");
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
}