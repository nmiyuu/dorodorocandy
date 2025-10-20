using UnityEngine;
using System.Collections; // ★修正点1: コルーチン(IEnumerator)を使うために必要

public class MoveBlock : MonoBehaviour
{
    public float moveUnit = 1.0f; // プレイヤーの moveUnit と一致させる
    public float moveSpeed = 5f;
    public LayerMask pushBlockerLayer; // ブロックの移動を妨げる障害物（動かない壁や別のブロック）

    private bool isMoving = false;
    private Vector3 targetPos;
    private BoxCollider2D blockCollider;

    void Start()
    {
        blockCollider = GetComponent<BoxCollider2D>();
        // Colliderが見つからない場合のエラーチェック
        if (blockCollider == null)
        {
            Debug.LogError("MoveBlockオブジェクトにBoxCollider2Dが見つかりません！");
        }
        targetPos = transform.position;
    }

    // プレイヤーから呼ばれる移動を試みるメソッド
    public bool TryMove(Vector3 direction)
    {
        // 処理の簡略化のため、TryMove内でのみNullチェックを実行
        if (isMoving || blockCollider == null) return false;

        // ... (BoxCastの計算は省略) ...
        Vector2 origin = (Vector2)transform.position + blockCollider.offset;
        Vector2 size = blockCollider.size;
        float angle = 0f;
        float checkDistance = moveUnit * 1.01f;

        RaycastHit2D hit = Physics2D.BoxCast(origin, size, angle, direction, checkDistance, pushBlockerLayer);

        if (hit.collider == null)
        {
            // 移動先が空いているため、移動を開始
            targetPos = transform.position + direction * moveUnit;

            // StartCoroutineの呼び出し
            StartCoroutine(MoveToPosition(targetPos));
            return true; // 移動成功
        }
        else
        {
            // 移動先に壁や別のブロックがあるため、移動失敗
            return false;
        }
    }

    // ブロックをターゲットの位置まで滑らかに移動させるコルーチン
    // ★修正点2: System.Collections. を外し、IEnumeratorのみにする
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