using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Push Block logic for grid-based movement.
public class MoveBlock : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("グリッド移動のサイズ。通常は1.0f (プレイヤーのmoveUnitと同じ)に設定。")]
    public float gridSize = 1f;
    public float moveSpeed = 5f; // スムーズな移動のための移動速度

    [Tooltip("障害物として判定するレイヤー。これには動かせない壁や、他のMoveBlockが含まれます。")]
    public LayerMask obstacleLayer;

    // --- State and IDs ---
    [Tooltip("このブロックをシーンをまたいで識別するためのIDを設定")]
    public string blockID;
    public string presentSceneName = "Stage1_now";

    private bool isMoving = false;
    private Vector3 targetPos;
    private Collider2D blockCollider;
    private Vector3 initialPosition;


    void Awake()
    {
        blockCollider = GetComponent<Collider2D>();
        if (blockCollider == null)
        {
            Debug.LogError($"MoveBlock '{gameObject.name}' に Collider2D がアタッチされていません。");
        }

        // BoxCast/OverlapBoxを正しく機能させるためにRigidbody2Dを確保する
        if (GetComponent<Rigidbody2D>() == null)
        {
            Rigidbody2D rb = gameObject.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic; // 物理演算の影響を受けないようにKinematicに設定
        }

        // 初期位置をグリッドにスナップ（0.5刻み）
        const float snapInverse = 2.0f; // 1.0f / 0.5f
        initialPosition = new Vector3(
            Mathf.Round(transform.position.x * snapInverse) / snapInverse,
            Mathf.Round(transform.position.y * snapInverse) / snapInverse,
            transform.position.z
        );
        transform.position = initialPosition;
        targetPos = initialPosition;

        if (string.IsNullOrEmpty(blockID))
        {
            blockID = gameObject.name;
            Debug.LogWarning($"MoveBlock '{gameObject.name}' に ID が設定されていません。オブジェクト名をIDとして使用します。");
        }

        // ----------------------------------------------------
        // --- 【追加】ブロックの位置ロード処理 ---
        // シーンをロードした際、過去に移動していたらその位置を再現する
        // ----------------------------------------------------
        if (SceneDataTransfer.Instance != null)
        {
            BlockState? savedState = SceneDataTransfer.Instance.GetBlockState(blockID);

            if (savedState.HasValue)
            {
                // 保存された位置を読み込み、ブロックと目標位置を更新
                Vector3 loadPosition = savedState.Value.finalPosition;

                transform.position = loadPosition;
                targetPos = loadPosition;

                Debug.Log($"[MoveBlock {gameObject.name}] SceneDataTransferから位置 ({loadPosition}) をロードしました。");
            }
        }
        // ----------------------------------------------------
    }

    // --- Movement Logic ---

    // Public entry point: プレイヤーから呼び出される
    public List<MoveBlock> GetPushableChain(Vector3 direction)
    {
        if (isMoving) return null;

        List<MoveBlock> chain = new List<MoveBlock>();
        Vector3 normalizedDirection = direction.normalized;

        if (CheckChainRecursive(normalizedDirection, chain))
        {
            return chain;
        }
        else
        {
            return null;
        }
    }

    // 再帰ヘルパー関数: 移動可能かをチェックし、可能なら chain リストに自身を追加する
    private bool CheckChainRecursive(Vector3 direction, List<MoveBlock> chain)
    {
        // 1. 無限再帰チェック
        if (chain.Contains(this))
        {
            Debug.LogError($"[MoveBlock {gameObject.name}] 循環参照による再帰ループを検出しました。", this);
            return false;
        }

        // 2. チェック済みリストに追加（移動可能と仮定）
        chain.Add(this);

        // 3. 衝突判定（次のグリッドに何があるか）
        Vector2 boxCastDirection = new Vector2(direction.x, direction.y);
        Vector2 destination = (Vector2)transform.position + boxCastDirection * gridSize;

        BoxCollider2D boxCol = blockCollider as BoxCollider2D;
        Vector2 boxSize = boxCol.size * transform.localScale * 0.9f;

        Collider2D[] hitColliders = Physics2D.OverlapBoxAll(destination, boxSize, 0f, obstacleLayer);

        MoveBlock nextBlock = null;
        bool isWallCollision = false;

        foreach (Collider2D hitCol in hitColliders)
        {
            if (hitCol.gameObject == gameObject) continue;

            MoveBlock tempBlock = hitCol.GetComponent<MoveBlock>();

            if (tempBlock != null)
            {
                nextBlock = tempBlock;
            }
            else
            {
                isWallCollision = true;
                break;
            }
        }

        // --- 4. 判定ロジック ---

        if (isWallCollision)
        {
            // 目的地に動かせない壁や障害物があった
            Debug.Log($"[MoveBlock {gameObject.name}] 目的地に動かせない障害物があり、移動をブロック。", this);
            chain.Remove(this);
            return false;
        }

        if (nextBlock != null)
        {
            // ***** 連鎖移動を禁止するロジック *****
            // 目的地に別の MoveBlock があったため、移動不可
            Debug.Log($"[MoveBlock {gameObject.name}] 目的地に他の岩ブロック ({nextBlock.gameObject.name}) があり、移動をブロック。", this);
            chain.Remove(this);
            return false;
            // ****************************************
        }

        // 目的地に何も検出されなかった（空きマス）
        return true;
    }

    // プレイヤーが移動を許可した後に呼び出す実際の移動開始メソッド
    public void StartMovement(Vector3 direction)
    {
        if (isMoving) return;

        targetPos = transform.position + direction.normalized * gridSize;
        StartCoroutine(MoveToPosition(targetPos));
    }


    // 実際の移動を処理するコルーチン
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

        // ----------------------------------------------------
        // --- 【追加】移動完了後に位置を保存する処理 ---
        // ブロックが停止した時点の最終位置を永続化データに保存する
        // ----------------------------------------------------
        if (SceneDataTransfer.Instance != null)
        {
            SceneDataTransfer.Instance.AddOrUpdateBlockState(blockID, transform.position);
            Debug.Log($"[MoveBlock {gameObject.name}] 移動完了後、位置 ({transform.position}) を保存しました。");
        }
        // ----------------------------------------------------
    }

    // --- Public Properties (省略) ---
    public bool IsMoving => isMoving;
}