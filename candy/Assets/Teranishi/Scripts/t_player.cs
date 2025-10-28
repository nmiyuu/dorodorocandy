using UnityEngine;
using System.Collections;

/// <summary>
/// プレイヤーの移動と衝突判定を管理するコアスクリプトだ。
/// この初期バージョンでは、キー入力に応じて即座に移動を開始（長押し有効）する。
/// </summary>
public class t_player : MonoBehaviour
{
    // --- パラメータ設定 (Inspector設定用) ---
    public float moveUnit = 1.0f;        // 1回の移動で進む距離（グリッド単位）
    public float moveSpeed = 5f;         // 1マス移動にかかる速度
    public LayerMask obstacleLayer;      // 衝突判定対象のレイヤー（Wall, Blockなど）

    // --- 内部状態とコンポーネント ---
    [SerializeField]
    private bool isMoving = false;       // 今、移動中かどうかのフラグ
    private Vector3 targetPos;           // 次に目指す移動目標座標
    private BoxCollider2D playerCollider;

    // t_pl.csへの参照は、この時点ではまだ取得していない。

    // --- 公開プロパティ (外部連携用) ---
    // TimeTravelControllerなどがこの位置を読み取る
    public Vector3 CurrentTargetPosition
    {
        get { return targetPos; }
    }

    public bool IsPlayerMoving
    {
        get { return isMoving; }
    }

    // --- Unityライフサイクル ---

    /// <summary>
    /// 初期化処理。コンポーネントの取得とシーンロード時の位置復元をする。
    /// </summary>
    void Awake()
    {
        playerCollider = GetComponent<BoxCollider2D>();

        if (playerCollider == null)
        {
            Debug.LogError("[t_player] 必須コンポーネントBoxCollider2Dが見つからない。", this);
            return;
        }

        // シーン切り替え時の位置ロード処理 (SceneDataTransferに依存)
        if (SceneDataTransfer.Instance != null)
        {
            Vector3 loadPosition = SceneDataTransfer.Instance.playerPositionToLoad;

            if (loadPosition != Vector3.zero)
            {
                transform.position = loadPosition;
                targetPos = loadPosition;
                return;
            }
        }

        // ロードデータがない場合は、現在の位置を目標に設定
        targetPos = transform.position;
    }

    void Update()
    {
        // 移動中は新しい入力を受け付けず、移動完了を優先する
        if (isMoving) return;

        Vector3 dir = Vector3.zero;

        // --- 入力受付と移動方向の決定 (長押し防止のため GetKeyDown を使用) ---
        // GetKeyDownはキーが押されたフレームでのみtrueを返すため、長押しによる連続移動を防ぐ。

        bool keyWasPressed = false; // キーが押された瞬間があったかをチェックするフラグ

        // 垂直入力（上下）を先にチェック
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            dir = Vector3.up;
            keyWasPressed = true;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            dir = Vector3.down;
            keyWasPressed = true;
        }

        // 水平入力（左右）をチェックし、垂直入力がなかった場合に処理を行う
        // （斜め移動を防ぐため、ここではelse ifを使用せず、先に押された垂直入力を優先する）
        if (dir == Vector3.zero) // 垂直方向のキーが押されていなかった場合のみ、水平方向をチェック
        {
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                dir = Vector3.right;
                keyWasPressed = true;
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                dir = Vector3.left;
                keyWasPressed = true;
            }
        }
        // ★注意: このロジックは「上下キーの入力を左右キーよりも優先する」という古い仕様のままだ。

        // どちらかのキーが押された瞬間、かつ移動方向が決定した場合のみ、衝突判定と移動を開始する
        if (keyWasPressed && dir != Vector3.zero)
        {
            // --- BoxCastによる壁の事前チェック ---

            // BoxCastの判定に必要な情報をColliderから取得
            Vector2 origin = (Vector2)transform.position + playerCollider.offset;
            Vector2 size = playerCollider.size;
            float angle = 0f;

            // チェック距離: 移動距離 (moveUnit) よりわずかに長く設定
            float checkDistance = moveUnit * 1.01f;

            // Physics2D.BoxCastを実行
            RaycastHit2D hit = Physics2D.BoxCast(origin, size, angle, dir, checkDistance, obstacleLayer);

            // --- 移動またはブロック押しの実行 ---
            if (hit.collider == null)
            {
                // 衝突なし: 移動を実行
                targetPos = transform.position + dir * moveUnit;
                StartCoroutine(MoveToPosition(targetPos));
            }
            else
            {
                // 何かに衝突した場合
                GameObject hitObject = hit.collider.gameObject;

                // MoveBlock のレイヤー番号を取得（MoveBlockレイヤーがある前提）
                int moveBlockLayerIndex = LayerMask.NameToLayer("MoveBlock");

                // 衝突したオブジェクトのレイヤーが "MoveBlock" と一致するか確認
                if (hitObject.layer == moveBlockLayerIndex)
                {
                    // 衝突したのが動くブロックの場合、MoveBlockコンポーネントを探す
                    MoveBlock blockToPush = hitObject.GetComponent<MoveBlock>();

                    if (blockToPush != null)
                    {
                        // ブロックを移動させるメソッドを呼び出す
                        if (blockToPush.TryMove(dir))
                        {
                            // ブロックの移動が成功したら、プレイヤーも移動を開始する
                            targetPos = transform.position + dir * moveUnit;
                            StartCoroutine(MoveToPosition(targetPos));
                        }
                        // ブロックの移動が失敗した場合は、プレイヤーも停止する。
                    }
                }
                // 壁などの動かせないオブジェクトの場合、プレイヤーは停止する。
            }
        }
    }

    /// <summary>
    /// プレイヤーをターゲット位置まで滑らかに移動させるコルーチン。
    /// 移動完了までisMovingフラグをtrueに保つ。
    /// </summary>
    IEnumerator MoveToPosition(Vector3 target)
    {
        isMoving = true;

        while ((transform.position - target).sqrMagnitude > 0.001f)
        {
            transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
            yield return null; // 1フレーム待機
        }

        // 誤差修正: 最終位置をターゲットに固定
        transform.position = target;
        isMoving = false;
    }
}