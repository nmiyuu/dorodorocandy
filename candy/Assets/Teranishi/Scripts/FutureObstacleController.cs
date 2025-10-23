using UnityEngine;
using System.Linq;

public class FutureObstacleController : MonoBehaviour
{
    // 過去のMoveBlockと対応するためのID
    [Tooltip("対応する過去のMoveBlockと同じユニークIDを設定してください。")]
    public string blockID;

    // 穴が埋まった後のスプライト
    [Header("スプライト設定")]
    [Tooltip("このブロックが穴を埋めた時に切り替わるスプライト。")]
    public Sprite filledBlockSprite;

    // 穴ブロックのレイヤーマスクを取得するための変数
    private LayerMask holeBlockLayer;
    private SpriteRenderer spriteRenderer; // このオブジェクトのスプライトレンダラー

    void Awake()
    {
        // レイヤーマスクを初期化時に取得 (レイヤー名: Hole)
        holeBlockLayer = LayerMask.GetMask("Hole");

        // 自分のSpriteRendererを取得
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogWarning($"FutureObstacleController '{gameObject.name}' に SpriteRenderer が見つかりません。");
        }
    }

    void Start()
    {
        // 1. SceneDataTransferが存在するか確認
        if (SceneDataTransfer.Instance == null)
        {
            Debug.LogError("SceneDataTransfer が見つかりません。");
            return;
        }

        // 2. 過去のブロックの保存位置を SceneDataTransfer から検索
        BlockState? savedState = SceneDataTransfer.Instance.pastBlockStates
            .Where(state => state.id == blockID)
            .Cast<BlockState?>()
            .FirstOrDefault();

        // 3. 保存データが存在する場合、位置を強制的に同期
        if (savedState.HasValue)
        {
            Vector3 finalPosition = savedState.Value.finalPosition;

            // 過去のブロックが動いた後の位置に、未来の動かせないブロックを強制移動させる
            transform.position = finalPosition;
            Debug.Log($"未来の静的障害物 '{blockID}' を、過去の移動位置 {finalPosition} に同期しました。");

            // 位置同期後に穴埋めチェックを実行
            CheckIfFillingHole();
        }
        else
        {
            // 過去のブロックが一度も動かされていない場合は、デフォルト位置に留まります。
        }
    }

    private void CheckIfFillingHole()
    {
        BoxCollider2D selfCollider = GetComponent<BoxCollider2D>();
        if (selfCollider == null) return;

        Collider2D[] hits = Physics2D.OverlapBoxAll(
            (Vector2)transform.position + selfCollider.offset,
            selfCollider.size,
            0f,
            holeBlockLayer
        );

        if (hits.Length > 0)
        {
            // 自分自身のスプライトを切り替える
            if (spriteRenderer != null && filledBlockSprite != null)
            {
                spriteRenderer.sprite = filledBlockSprite;
                Debug.Log($"'{blockID}' のスプライトを埋まった後の状態に切り替えました。");
            }

            selfCollider.enabled = false; // 自分のColliderを無効化

            // 穴ブロックのHoleBlockスクリプトを呼び出して状態を変更させる
            foreach (var hit in hits)
            {
                // レイヤー名が「Hole」のオブジェクトのみを処理
                if (hit.gameObject.layer == LayerMask.NameToLayer("Hole"))
                {
                    HoleBlock hole = hit.gameObject.GetComponent<HoleBlock>();
                    if (hole != null)
                    {
                        hole.BeFilled(); // 穴ブロックに自身の状態変更を依頼する
                    }
                    else
                    {
                        Debug.LogError($"【スクリプトエラー】検出したオブジェクト '{hit.gameObject.name}' に HoleBlock.cs が見つかりません。");
                    }
                }
            }

            Debug.LogWarning($"静的障害物 '{blockID}' は穴を埋めました。道となりました。");
        }
        else
        {
            Debug.LogWarning($"FutureObstacleController '{gameObject.name}' は、この位置で 'Hole' レイヤーのオブジェクトを検出できませんでした。OverlapBoxAllが失敗。");
        }
    }
}