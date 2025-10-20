using UnityEngine;
using System.Collections; // IEnumeratorのために必要です

public class t_player : MonoBehaviour
{
    public float moveUnit = 1.0f;      // 1回の移動で進む距離（1マス分）
    public float moveSpeed = 5f;       // 移動スピード
    public LayerMask Obstacle;         // 壁・障害物のレイヤー

    private bool isMoving = false;
    private Vector3 targetPos;

    // BoxCollider2DをStartで取得し、繰り返しGetComponentを避ける
    private BoxCollider2D playerCollider;

    void Start()
    {
        // Startで一度だけBoxColliderを取得
        playerCollider = GetComponent<BoxCollider2D>();
        if (playerCollider == null)
        {
            Debug.LogError("PlayerオブジェクトにBoxCollider2Dが見つかりません！");
        }

        targetPos = transform.position;
    }

    void Update()
    {
        if (isMoving) return;

        Vector3 dir = Vector3.zero;

        // 方向キー入力を検知
        if (Input.GetKeyDown(KeyCode.RightArrow)) dir = Vector3.right;
        else if (Input.GetKeyDown(KeyCode.LeftArrow)) dir = Vector3.left;
        else if (Input.GetKeyDown(KeyCode.UpArrow)) dir = Vector3.up;
        else if (Input.GetKeyDown(KeyCode.DownArrow)) dir = Vector3.down;

        if (dir != Vector3.zero)
        {
            // BoxCastの判定に必要な情報を取得
            Vector2 origin = (Vector2)transform.position + playerCollider.offset; // 始点をオフセット分調整
            Vector2 size = playerCollider.size;
            float angle = 0f; // プレイヤーの回転がない前提

            // BoxCastを使い、壁があるかチェック
            // BoxCastAllを使うと、複数の当たり判定（タイルマップの複数のタイル）も正確に検出できます
            // 距離は moveUnit - 許容誤差(epsilon) に設定することで、ピッタリ隣接した壁にぶつかるのを防ぎます
            float distance = moveUnit;

            // BoxCastを使って壁判定
            // BoxCastが true を返した場合（何かに当たった場合）は移動しない
            if (!Physics2D.BoxCast(origin, size, angle, dir, distance, Obstacle))
            {
                // 壁がなければ目的地を更新して移動を開始
                targetPos = transform.position + dir * moveUnit;
                StartCoroutine(MoveToPosition(targetPos));
            }
        }
    }

    // プレイヤーをターゲットの位置まで滑らかに移動させる処理（IEnumeratorはSystem.Collectionsにあります）
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
    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("a");
    }
}