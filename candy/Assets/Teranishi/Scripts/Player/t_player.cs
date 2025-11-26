using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

// プレイヤーの移動と衝突判定を全部やるコアスクリプト。
public class t_player : MonoBehaviour // t_player から PlayerController に変更 (ファイルパスに合わせる)
{
    // --- パラメータ設定 (Inspector設定用) ---
    public float moveUnit = 1.0f;       // 1マス進む距離
    public float moveSpeed = 5f;        // 移動スピード
    public LayerMask obstacleLayer;      // ぶつかる対象のレイヤー（壁とかブロック）

    // --- 内部状態とコンポーネント ---
    [SerializeField]
    private string resetSceneName = "Stage1_now"; // Rキーリセット時にロードする基準シーン名
    private bool isMoving = false;       // 移動中フラグ
    private Vector3 targetPos;           // 次の目的地
    private BoxCollider2D playerCollider;
    private t_pl playerAnimScript;       // アニメーション担当のt_plへの参照

    // 最後に押されたキーと時間を記録する辞書（キー優先判定に使う）
    private Dictionary<int, float> lastKeyPressTime = new Dictionary<int, float>();

    // --- 公開プロパティ (外部連携用) ---
    public Vector3 CurrentTargetPosition
    {
        get { return targetPos; }
    }
    public bool IsPlayerMoving
    {
        get { return isMoving; }
    }

    // --- Unityライフサイクル ---

    void Awake()
    {
        playerCollider = GetComponent<BoxCollider2D>();
        playerAnimScript = GetComponent<t_pl>();

        if (playerCollider == null) Debug.LogError("[PlayerController] BoxCollider2Dがない");
        if (playerAnimScript == null) Debug.LogError("[PlayerController] t_plがない");

        // 辞書の初期化（方向インデックスを登録）
        lastKeyPressTime.Add(1, 0f); // 下
        lastKeyPressTime.Add(2, 0f); // 上
        lastKeyPressTime.Add(3, 0f); // 右
        lastKeyPressTime.Add(4, 0f); // 左

        // シーン切り替え時の位置ロード処理 (SceneDataTransferに依存)
        
        if (SceneDataTransfer.Instance != null)
        {
            Vector3 loadPosition = SceneDataTransfer.Instance.playerPositionToLoad;
            if (loadPosition != Vector3.zero)
            {
                transform.position = loadPosition;
                targetPos = loadPosition;
            }
        }
        

        // ロードされなかったら、現在のHierarchy上の位置を目標にする
        if (targetPos == Vector3.zero) targetPos = transform.position;
    }

    // --- メイン処理 ---

    void Update()
    {
        // Rキーが押されたら、FullSceneResetを実行する
        if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
        {
            FullSceneReset();
            return; // リセット処理後は即座に抜ける
        }

        if (playerAnimScript == null || Keyboard.current == null) return;

        // フェードイン/アウト中はすべての移動・操作をブロックする
        // SceneFaderの参照がない場合はコメントアウトまたは適切な処理に置き換えてください
        /*
        if (SceneFader.Instance != null && SceneFader.Instance.IsFading)
        {
            return;
        }
        */

        // --- アニメーション向きの更新 ---
        int newDirectionIndex = CalculateNewDirectionForAnimation();
        playerAnimScript.SetDirectionFromExternal(newDirectionIndex);

        // 移動中は入力を受け付けない
        if (isMoving) return;

        // --- 移動判定と実行 ---

        bool keyWasPressed = Keyboard.current.upArrowKey.wasPressedThisFrame ||
                             Keyboard.current.downArrowKey.wasPressedThisFrame ||
                             Keyboard.current.leftArrowKey.wasPressedThisFrame ||
                             Keyboard.current.rightArrowKey.wasPressedThisFrame;

        if (!keyWasPressed) return;

        // 押されたキーの中で、最新の向きを移動方向として決定
        Vector3 dir = GetMoveDirectionFromLatestPress();

        // 衝突判定と移動の実行
        if (dir != Vector3.zero)
        {
            // --- プレイヤーの位置を0.5刻みのグリッドに丸める ---
            const float snapInverse = 2.0f; // 1.0f / 0.5f
            transform.position = new Vector3(
                Mathf.Round(transform.position.x * snapInverse) / snapInverse,
                Mathf.Round(transform.position.y * snapInverse) / snapInverse,
                transform.position.z
            );

            // --- 衝突判定（BoxCast）の実行 ---
            Vector2 origin = (Vector2)transform.position + playerCollider.offset;
            Vector2 size = playerCollider.size;
            float angle = 0f;
            // プレイヤーのサイズは、グリッドサイズよりわずかに小さくすることで、
            // プレイヤーとブロックが同じ位置にいると判定されるのを防ぐ
            Vector2 boxSize = size * 0.9f;
            float checkDistance = moveUnit * 1.01f;

            // Physics2D.BoxCast の呼び出し
            RaycastHit2D hit = Physics2D.BoxCast(origin, boxSize, angle, dir, checkDistance, obstacleLayer);

            if (hit.collider == null)
            {
                // 1. 何もない（空いている）なら移動する (通常の移動)
                targetPos = transform.position + dir * moveUnit;
                Debug.Log($"[Player Move] OK: 空きマスへ移動します。方向: {dir}, 目標位置: {targetPos}");
                StartCoroutine(MoveToPosition(targetPos));
            }
            else
            {
                // 2. 何かにぶつかった場合、それがMoveBlockかどうかをチェックする
                GameObject hitObject = hit.collider.gameObject;
                MoveBlock blockToPush = hitObject.GetComponent<MoveBlock>();

                // 衝突対象がMoveBlock（押せる岩）の場合
                if (blockToPush != null)
                {
                    // --- 修正箇所: TryMove を GetPushableChain に置き換え、連鎖移動を実行 ---
                    List<MoveBlock> pushableChain = blockToPush.GetPushableChain(dir);

                    if (pushableChain != null)
                    {
                        // ブロックを動かせた（連鎖移動が可能だった）から、自分とブロック全てを移動させる

                        // プレイヤーの移動
                        targetPos = transform.position + dir * moveUnit;
                        StartCoroutine(MoveToPosition(targetPos));

                        // ブロックの連鎖移動を開始
                        foreach (var block in pushableChain)
                        {
                            block.StartMovement(dir);
                        }

                        Debug.Log($"[Player Push] OK: {pushableChain.Count}個のブロックを押して移動します。方向: {dir}");
                    }
                    else
                    {
                        // ブロックが動かせなかった場合（壁に当たった、循環参照、など）は、自分も止まる
                        Debug.Log($"[Player Push] BLOCKED: ブロック ({blockToPush.gameObject.name}) は動かせません。");
                    }
                }
                // 衝突対象がMoveBlockではない場合（動かせない壁や岩）
                else
                {
                    // 壁など動かせない障害物に当たったので、何もせず止まる
                    Debug.Log($"[Player Move] BLOCKED: 障害物 ({hitObject.name}) に衝突しました。");
                }
            }
        }

    }

    // --- プライベートメソッド (ロジック統合部分) ---
    // (アニメーション、方向計算、リセット処理は t_player.cs からそのまま引き継ぐ)

    private int CalculateNewDirectionForAnimation()
    {
        var keyboard = Keyboard.current;
        List<int> pressedDirections = new List<int>();

        if (keyboard.downArrowKey.isPressed) { lastKeyPressTime[1] = Time.time; pressedDirections.Add(1); }
        if (keyboard.upArrowKey.isPressed) { lastKeyPressTime[2] = Time.time; pressedDirections.Add(2); }
        if (keyboard.rightArrowKey.isPressed) { lastKeyPressTime[3] = Time.time; pressedDirections.Add(3); }
        if (keyboard.leftArrowKey.isPressed) { lastKeyPressTime[4] = Time.time; pressedDirections.Add(4); }

        if (pressedDirections.Count == 0)
        {
            return playerAnimScript.CurrentDirectionIndex;
        }

        int preferredIndex = 0;
        float latestTime = -1f;

        foreach (int index in pressedDirections)
        {
            if (lastKeyPressTime[index] > latestTime)
            {
                latestTime = lastKeyPressTime[index];
                preferredIndex = index;
            }
        }
        return preferredIndex;
    }

    private Vector3 GetMoveDirectionFromLatestPress()
    {
        var keyboard = Keyboard.current;

        float latestTime = -1f;
        int latestIndex = 0;

        if (keyboard.downArrowKey.wasPressedThisFrame)
        {
            if (Time.time > latestTime) { latestTime = Time.time; latestIndex = 1; }
        }
        if (keyboard.upArrowKey.wasPressedThisFrame)
        {
            if (Time.time > latestTime) { latestTime = Time.time; latestIndex = 2; }
        }
        if (keyboard.rightArrowKey.wasPressedThisFrame)
        {
            if (Time.time > latestTime) { latestTime = Time.time; latestIndex = 3; }
        }
        if (keyboard.leftArrowKey.wasPressedThisFrame)
        {
            if (Time.time > latestTime) { latestTime = Time.time; latestIndex = 4; }
        }

        return ConvertDirectionIndexToVector(latestIndex);
    }


    private Vector3 ConvertDirectionIndexToVector(int index)
    {
        switch (index)
        {
            case 1: return Vector3.down;
            case 2: return Vector3.up;
            case 3: return Vector3.right;
            case 4: return Vector3.left;
            default: return Vector3.zero;
        }
    }


    private void FullSceneReset()
    {
        
        if (SceneDataTransfer.Instance != null)
        {
            SceneDataTransfer.Instance.FullReset();
        }

        if (SceneFader.Instance != null)
        {
            SceneFader.Instance.LoadSceneWithFade(resetSceneName, FadeColor.Black);
        }
        else
        {
            SceneManager.LoadScene(resetSceneName);
        }
        

        // 暫定的なリセット処理（SceneManagerのみ使用）
        SceneManager.LoadScene(resetSceneName);


        Debug.Log($"シーン '{resetSceneName}' をRキーで完全リセットする (基準シーンへ)");
    }


    // --- コルーチン ---

    IEnumerator MoveToPosition(Vector3 target)
    {
        isMoving = true;

        while ((transform.position - target).sqrMagnitude > 0.001f)
        {
            transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = target;
        isMoving = false;
    }
}