using UnityEngine;
using System.Collections;

public class t_player : MonoBehaviour
{
    // --- パラメータ設定 (Inspectorで設定) ---
    public float moveUnit = 1.0f;       // 1回の移動で進む距離（1マス分）
    public float moveSpeed = 5f;        // 移動スピード
    public LayerMask obstacleLayer;     // 衝突判定を行うレイヤー（壁、ブロックなど）

    // --- 内部状態 ---
    [SerializeField]
    private bool isMoving = false;
    private Vector3 targetPos;
    private BoxCollider2D playerCollider;
    // TimeTravelControllerがアクセスするための公開プロパティ
    public Vector3 CurrentTargetPosition
    {
        get { return targetPos; }
    }

    // プレイヤーが移動中かどうかを外部から確認するためのプロパティ
    public bool IsPlayerMoving
    {
        get { return isMoving; }
    }

    // Start()よりも早く実行されるAwake()で初期化と位置ロードを行う
    void Awake()
    {
        // 必須コンポーネントの取得
        playerCollider = GetComponent<BoxCollider2D>();
        if (playerCollider == null)
        {
            Debug.LogError("PlayerオブジェクトにBoxCollider2Dが見つかりません！");
            return;
        }

        // --- シーン切り替え時の位置ロード処理 ---
        if (SceneDataTransfer.Instance != null)
        {
            Vector3 loadPosition = SceneDataTransfer.Instance.playerPositionToLoad;

            // Vector3.zero 以外（データが保存されていた場合）のみ位置をロードする
            if (loadPosition != Vector3.zero)
            {
                transform.position = loadPosition;
                targetPos = loadPosition;
                return;
            }
        }

        // ロードされなかった場合（初回起動時など）は、現在のHierarchy上の位置をターゲットに設定
        targetPos = transform.position;
    }

    void Start()
    {
        // Awake()でほとんどの初期化を終えているため、Start()は空で問題ありません。
    }

    void Update()
    {
        // 移動中は新しい入力を受け付けない
        if (isMoving) return;

        Vector3 dir = Vector3.zero;

        // --- 1. 入力検知 ---
        // 押されたキーに応じて移動方向を決定（斜め移動なし）
        if (Input.GetKeyDown(KeyCode.RightArrow)) dir = Vector3.right;
        else if (Input.GetKeyDown(KeyCode.LeftArrow)) dir = Vector3.left;
        else if (Input.GetKeyDown(KeyCode.UpArrow)) dir = Vector3.up;
        else if (Input.GetKeyDown(KeyCode.DownArrow)) dir = Vector3.down;

        // 方向入力があった場合のみ処理
        if (dir != Vector3.zero)
        {
            // --- 2. BoxCastによる壁の事前チェック ---

            // BoxCastの判定に必要な情報をColliderから取得
            Vector2 origin = (Vector2)transform.position + playerCollider.offset;
            Vector2 size = playerCollider.size;
            float angle = 0f;

            // チェック距離: 移動距離 (moveUnit) よりわずかに長く設定
            float checkDistance = moveUnit * 1.01f;

            // Physics2D.BoxCastを実行: プレイヤーの移動先に何か（壁またはブロック）があるかを確認
            RaycastHit2D hit = Physics2D.BoxCast(origin, size, angle, dir, checkDistance, obstacleLayer);

            
            //衝突判定とブロック押し処理

            // 衝突しなかった場合 (移動先に何もない)
            if (hit.collider == null)
            {
                // 移動を実行
                targetPos = transform.position + dir * moveUnit;
                StartCoroutine(MoveToPosition(targetPos));
            }
            else
            {
                // 何かに衝突した場合
                GameObject hitObject = hit.collider.gameObject;

                // MoveBlock のレイヤー番号を取得
                int moveBlockLayerIndex = LayerMask.NameToLayer("MoveBlock");

                // 衝突したオブジェクトのレイヤーが "MoveBlock" と一致するか確認
                if (hitObject.layer == moveBlockLayerIndex)
                {
                    // 衝突したのが動くブロックの場合、MoveBlockコンポーネントを探す
                    // （MoveBlock.csという名前のスクリプトがある前提）
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
                        // ブロックの移動が失敗した場合は、プレイヤーも停止。
                    }
                }
                // 当たったのがMoveBlockではない（動かない壁など）場合は、プレイヤーは停止。
            }
        }
    }

    // プレイヤーをターゲットの位置まで滑らかに移動させるコルーチン
    IEnumerator MoveToPosition(Vector3 target)
    {
        isMoving = true;

        // ターゲットに近づくまでループ
        while ((transform.position - target).sqrMagnitude > 0.001f)
        {
            // ターゲットに向けて移動 (Time.deltaTimeを使用)
            transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
            yield return null; // 1フレーム待機
        }

        // 最後の仕上げ: 誤差をなくすため、正確なターゲット位置に固定する
        transform.position = target;
        isMoving = false;
    }
}