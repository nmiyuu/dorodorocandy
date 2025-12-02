using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class t_player : MonoBehaviour
{
    // --- パラメータ設定 (Inspector設定用) ---
    [Header("移動設定")]
    public float moveUnit = 1.0f;        // 1マス進む距離
    public float moveSpeed = 5f;         // 移動スピード
    public LayerMask obstacleLayer;      // ぶつかる対象のレイヤー（壁とかブロック）

    [Header("アニメーション設定")]
    [Tooltip("アニメーション担当のt_plへの参照。")]
    public t_pl playerAnimScript;        // t_pl スクリプトをInspectorで設定可能にする

    // --- 内部状態とコンポーネント ---
    private bool isMoving = false;       // 移動中フラグ
    private Vector3 targetPos;           // 次の目的地
    private BoxCollider2D playerCollider;

    // 最後に押されたキーと時間を記録する辞書 (アニメーションの向き決定用。使用しない場合は削除可)
    private Dictionary<int, float> lastKeyPressTime = new Dictionary<int, float>();

    // --- 公開プロパティ (外部連携用) ---
    public Vector3 CurrentTargetPosition => targetPos;
    public bool IsPlayerMoving => isMoving;

    // --- Unityライフサイクル ---

    void Awake()
    {
        playerCollider = GetComponent<BoxCollider2D>();

        // t_pl スクリプトがInspectorで設定されていない場合、自動で取得を試みる
        if (playerAnimScript == null)
        {
            playerAnimScript = GetComponent<t_pl>();
        }

        if (playerCollider == null) Debug.LogError("[PlayerController] BoxCollider2Dがない");
        if (playerAnimScript == null) Debug.LogWarning("[PlayerController] t_pl (アニメーションスクリプト) が見つかりません。");

        // 辞書の初期化 (アニメーション向き決定用)
        lastKeyPressTime.Add(1, 0f); // 下
        lastKeyPressTime.Add(2, 0f); // 上
        lastKeyPressTime.Add(3, 0f); // 右
        lastKeyPressTime.Add(4, 0f); // 左

        // --- シーン切り替え時の位置ロード処理 ---
        if (SceneDataTransfer.Instance != null)
        {
            Vector3 loadPosition = SceneDataTransfer.Instance.playerPositionToLoad;
            int loadDir = SceneDataTransfer.Instance.playerDirectionIndexToLoad;

            if (loadPosition != Vector3.zero)
            {
                transform.position = loadPosition;
                targetPos = loadPosition;
                if (loadDir != 0 && playerAnimScript != null)
                {
                    // t_plスクリプトのSetDirectionFromExternalを呼び出し、向きを再現
                    playerAnimScript.SetDirectionFromExternal(loadDir);
                }
            }
            else
            {
                targetPos = transform.position;
            }
        }
        else
        {
            targetPos = transform.position;
        }
    }

    void Update()
    {
        // シーン切り替え中は移動入力を無視するガードロジック
        if (SceneDataTransfer.Instance != null && SceneDataTransfer.Instance.isChangingScene)
        {
            return;
        }
        // 

        // Rキーが押されたら、SoftResetを実行する (移動回数は維持される)
        if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
        {
            FullSceneReset();
            return;
        }

        if (playerAnimScript == null || Keyboard.current == null) return;

        // 移動中は入力を受け付けない
        if (isMoving) return;

        // --- 移動方向の決定 ---
        Vector3 dir = Vector3.zero;

        if (Keyboard.current.upArrowKey.wasPressedThisFrame)
        {
            dir = Vector3.up;
            // アニメーション向きを設定する場合 (向きインデックス 2 = 上)
            playerAnimScript?.SetDirectionFromExternal(2);
        }
        else if (Keyboard.current.downArrowKey.wasPressedThisFrame)
        {
            dir = Vector3.down;
            // アニメーション向きを設定する場合 (向きインデックス 1 = 下)
            playerAnimScript?.SetDirectionFromExternal(1);
        }
        else if (Keyboard.current.leftArrowKey.wasPressedThisFrame)
        {
            dir = Vector3.left;
            // アニメーション向きを設定する場合 (向きインデックス 4 = 左)
            playerAnimScript?.SetDirectionFromExternal(4);
        }
        else if (Keyboard.current.rightArrowKey.wasPressedThisFrame)
        {
            dir = Vector3.right;
            // アニメーション向きを設定する場合 (向きインデックス 3 = 右)
            playerAnimScript?.SetDirectionFromExternal(3);
        }


        if (dir != Vector3.zero)
        {
            // --- 衝突判定（BoxCast）の実行 ---
            Vector2 origin = (Vector2)transform.position + playerCollider.offset;
            Vector2 size = playerCollider.size;
            float angle = 0f;
            // 衝突判定に使うBoxのサイズを少し小さくして、ブロックとの接触を確実に判定
            Vector2 boxSize = size * 0.9f;
            float checkDistance = moveUnit * 1.01f;

            RaycastHit2D hit = Physics2D.BoxCast(origin, boxSize, angle, dir, checkDistance, obstacleLayer);

            if (hit.collider == null)
            {
                // 1. 何もない（空いている）なら移動する (通常の移動)

                if (SceneDataTransfer.Instance != null)
                {
                    // 移動回数を記録
                    SceneDataTransfer.Instance.IncrementMoveCount();
                }

                targetPos = transform.position + dir * moveUnit;
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
                    // MoveBlockのGetPushableChainを使って、移動可能かチェック
                    List<MoveBlock> pushableChain = blockToPush.GetPushableChain(dir);

                    if (pushableChain != null)
                    {
                        // ブロックを動かせた（連鎖移動が可能だった）

                        if (SceneDataTransfer.Instance != null)
                        {
                            // 移動回数を記録
                            SceneDataTransfer.Instance.IncrementMoveCount();
                        }

                        // プレイヤーの移動
                        targetPos = transform.position + dir * moveUnit;
                        StartCoroutine(MoveToPosition(targetPos));

                        // ブロックの連鎖移動を開始
                        foreach (var block in pushableChain)
                        {
                            block.StartMovement(dir);
                        }
                    }
                    else
                    {
                        // ブロックが動かせなかった場合 (連鎖禁止、壁にぶつかったなど)
                        Debug.Log($"[Player Push] BLOCKED: ブロック ({blockToPush.gameObject.name}) は動かせません。");
                    }
                }
                // 衝突対象がMoveBlockではない場合（動かせない壁など）
                else
                {
                    Debug.Log($"[Player Move] BLOCKED: 障害物 ({hitObject.name}) に衝突しました。");
                }
            }
        }
    }

    // --- Rキー押下時のソフトリセット処理 ---
    private void FullSceneReset()
    {
        if (SceneDataTransfer.Instance != null)
        {
            // ★ソフトリセットを呼び出し、移動回数を維持する★
            SceneDataTransfer.Instance.SoftReset();
        }

        // 現在のシーンを再ロード
        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);
    }

    // MoveToPosition コルーチン (移動処理)
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