using UnityEngine;
using System.Collections;

public class t_player : MonoBehaviour
{
    // --- パラメータ設定 (Inspectorで設定) ---
    public float moveUnit = 1.0f;       // 1回の移動で進む距離（1マス分）
    public float moveSpeed = 5f;        // 移動スピード
    public LayerMask obstacleLayer;     // 壁・障害物のレイヤー (Tilemapと同じレイヤーにチェックを入れる)

    // --- 内部状態 ---
    private bool isMoving = false;
    private Vector3 targetPos;
    private BoxCollider2D playerCollider;

    void Start()
    {
        // 必須コンポーネントの取得
        playerCollider = GetComponent<BoxCollider2D>();
        if (playerCollider == null)
        {
            Debug.LogError("PlayerオブジェクトにBoxCollider2Dが見つかりません！");
            return;
        }

        // 初期位置をターゲット位置に設定
        targetPos = transform.position;
    }

    void Update()
    {
        // 移動中は新しい入力を受け付けない
        if (isMoving) return;

        Vector3 dir = Vector3.zero;

        // --- 1. 入力検知 ---
        // 押されたキーに応じて移動方向を決定
        if (Input.GetKeyDown(KeyCode.RightArrow)) dir = Vector3.right;
        else if (Input.GetKeyDown(KeyCode.LeftArrow)) dir = Vector3.left;
        else if (Input.GetKeyDown(KeyCode.UpArrow)) dir = Vector3.up;
        else if (Input.GetKeyDown(KeyCode.DownArrow)) dir = Vector3.down;

        // 方向入力があった場合のみ処理
        if (dir != Vector3.zero)
        {
            // --- 2. BoxCastによる壁の事前チェック ---

            // BoxCastの判定に必要な情報をColliderから取得
            // 始点は「現在の位置 + Colliderのオフセット」
            Vector2 origin = (Vector2)transform.position + playerCollider.offset;
            // サイズはColliderのサイズ
            Vector2 size = playerCollider.size;
            float angle = 0f;

            // チェック距離: 移動距離 (moveUnit) よりわずかに長く設定する
            // 誤差やタイルの境界線によるすり抜けを防ぐための必須の調整
            float checkDistance = moveUnit * 1.01f; // 1.01倍の余裕を持たせる

            // Physics2D.BoxCastを実行:
            // プレイヤーのColliderと同じ形状・サイズで、移動先まで障害物がないかを確認する
            RaycastHit2D hit = Physics2D.BoxCast(origin, size, angle, dir, checkDistance, obstacleLayer);

            // hit.collider が null（何も衝突しなかった）の場合のみ移動を実行
            if (hit.collider == null)
            {
                // 壁がなければ目的地を更新し、コルーチンで滑らかに移動を開始
                targetPos = transform.position + dir * moveUnit;
                StartCoroutine(MoveToPosition(targetPos));
            }
            // else の場合は壁があるため、処理を終了（移動せずにその場に留まる = 壁に当たって止まる）
        }
    }

    // プレイヤーをターゲットの位置まで滑らかに移動させるコルーチン
    IEnumerator MoveToPosition(Vector3 target)
    {
        isMoving = true;

        // ターゲットに近づくまで（距離の二乗が0.001fより大きい間）ループ
        while ((transform.position - target).sqrMagnitude > 0.001f)
        {
            // ターゲットに向けて移動 (フレームレートに依存しないようにTime.deltaTimeを使用)
            transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
            yield return null; // 1フレーム待機
        }

        // 最後の仕上げ: 誤差をなくすため、正確なターゲット位置に固定する
        transform.position = target;
        isMoving = false;
    }
}