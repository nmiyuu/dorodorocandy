using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Linq;

public class PlayerController : MonoBehaviour
{
    // --- プレイヤー固有の定数とコンポーネント ---
    private Animator _animator;
    private BoxCollider2D playerCollider;
    private const string DirectionParam = "Direction"; // AnimatorのIntパラメーター名

    // --- パラメータ設定 (Inspectorで設定) ---
    public float moveUnit = 1.0f;        // 1回の移動で進む距離（1マス分）
    public float moveSpeed = 5f;         // 移動スピード
    public LayerMask obstacleLayer;      // 衝突判定を行うレイヤー（壁、ブロックなど）

    // --- 状態管理 ---
    [SerializeField]
    private bool isMoving = false;
    private Vector3 targetPos;

    // 現在の向き (1:下, 2:上, 3:右, 4:左)
    public int CurrentDirectionIndex => lastDirectionIndex;
    private int lastDirectionIndex = 1;

    // 最後に押されたキーとタイムスタンプを格納する辞書 (最後に押されたキー優先ロジック用)
    private Dictionary<int, float> lastKeyPressTime = new Dictionary<int, float>();

    // TimeTravelControllerがアクセスするための公開プロパティ
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
        // コンポーネントの取得
        _animator = GetComponent<Animator>();
        playerCollider = GetComponent<BoxCollider2D>();

        // 必須コンポーネントのチェック
        if (_animator == null) Debug.LogError("Animatorコンポーネントが見つかりません。");
        if (playerCollider == null) Debug.LogError("BoxCollider2Dが見つかりません。");

        // 辞書の初期化
        lastKeyPressTime.Add(1, 0f); // 下
        lastKeyPressTime.Add(2, 0f); // 上
        lastKeyPressTime.Add(3, 0f); // 右
        lastKeyPressTime.Add(4, 0f); // 左

        // --- シーン切り替え時の位置と向きのロード処理 ---
        if (SceneDataTransfer.Instance != null)
        {
            // 1. 位置のロード
            Vector3 loadPosition = SceneDataTransfer.Instance.playerPositionToLoad;
            if (loadPosition != Vector3.zero)
            {
                transform.position = loadPosition;
                targetPos = loadPosition;
            }
            else
            {
                targetPos = transform.position;
            }

            // 2. 向きのロード
            int loadDirection = SceneDataTransfer.Instance.playerDirectionIndexToLoad;
            if (loadDirection != 0)
            {
                lastDirectionIndex = loadDirection;
                UpdateAnimator(lastDirectionIndex);
            }
            else
            {
                UpdateAnimator(lastDirectionIndex); // デフォルト向きで初期化
            }
        }
        else
        {
            targetPos = transform.position;
            UpdateAnimator(lastDirectionIndex); // デフォルト向きで初期化
        }
    }

    void Update()
    {
        if (Keyboard.current == null || _animator == null) return;

        // 1. アニメーション向きの更新 (キーが押されている間、最新のキーを追跡)
        UpdateDirection();

        // 2. 移動中は新しい入力を受け付けない
        if (isMoving) return;

        // 3. 移動トリガーの判定 (長押し防止のため wasPressedThisFrame を使用)
        bool keyWasPressed = Keyboard.current.rightArrowKey.wasPressedThisFrame ||
                             Keyboard.current.leftArrowKey.wasPressedThisFrame ||
                             Keyboard.current.upArrowKey.wasPressedThisFrame ||
                             Keyboard.current.downArrowKey.wasPressedThisFrame;

        // キーが押された瞬間ではない場合、移動は開始しない
        if (!keyWasPressed) return;

        // 4. アニメーション向きに基づいて移動方向を決定
        Vector3 dir = ConvertDirectionIndexToVector(lastDirectionIndex);

        if (dir != Vector3.zero)
        {
            TryMove(dir);
        }
    }

    // --- プライベートメソッド：向きの更新とアニメーション制御 ---

    private void UpdateDirection()
    {
        var keyboard = Keyboard.current;
        List<int> pressedDirections = new List<int>();

        // 押されているキーのタイムスタンプを更新
        if (keyboard.downArrowKey.isPressed) { lastKeyPressTime[1] = Time.time; pressedDirections.Add(1); }
        if (keyboard.upArrowKey.isPressed) { lastKeyPressTime[2] = Time.time; pressedDirections.Add(2); }
        if (keyboard.rightArrowKey.isPressed) { lastKeyPressTime[3] = Time.time; pressedDirections.Add(3); }
        if (keyboard.leftArrowKey.isPressed) { lastKeyPressTime[4] = Time.time; pressedDirections.Add(4); }

        // 最後に押されたキーを特定し、向きを更新
        if (pressedDirections.Count > 0)
        {
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

            SetDirection(preferredIndex);
        }

        // 常に現在の向きでAnimatorを更新
        UpdateAnimator(lastDirectionIndex);
    }

    private void SetDirection(int newIndex)
    {
        if (newIndex != 0)
        {
            lastDirectionIndex = newIndex;
        }
    }

    private void UpdateAnimator(int directionIndex)
    {
        if (_animator != null)
        {
            _animator.SetInteger(DirectionParam, directionIndex);
        }
    }

    // 向きのインデックスをVector3に変換するヘルパー関数
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

    // --- プライベートメソッド：移動と衝突判定 ---

    private void TryMove(Vector3 dir)
    {
        // BoxCastの判定に必要な情報をColliderから取得
        Vector2 origin = (Vector2)transform.position + playerCollider.offset;
        Vector2 size = playerCollider.size;
        float angle = 0f;
        float checkDistance = moveUnit * 1.01f;

        RaycastHit2D hit = Physics2D.BoxCast(origin, size, angle, dir, checkDistance, obstacleLayer);

        // 衝突判定とブロック押し処理
        if (hit.collider == null)
        {
            // 衝突なし: 移動を実行
            targetPos = transform.position + dir * moveUnit;
            StartCoroutine(MoveToPosition(targetPos));
        }
        else
        {
            // 衝突あり: ブロック押しのチェック
            GameObject hitObject = hit.collider.gameObject;
            int moveBlockLayerIndex = LayerMask.NameToLayer("MoveBlock");

            if (hitObject.layer == moveBlockLayerIndex)
            {
                MoveBlock blockToPush = hitObject.GetComponent<MoveBlock>();

                if (blockToPush != null)
                {
                    if (blockToPush.TryMove(dir))
                    {
                        // ブロックの移動が成功したら、プレイヤーも移動を開始する
                        targetPos = transform.position + dir * moveUnit;
                        StartCoroutine(MoveToPosition(targetPos));
                    }
                }
            }
        }
    }

    // プレイヤーをターゲットの位置まで滑らかに移動させるコルーチン
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

    // シーン切り替え時の向き復元用（TimeTravelControllerから呼ばれる）
    public void LoadDirectionIndex(int index)
    {
        if (index != 0)
        {
            lastDirectionIndex = index;
            UpdateAnimator(lastDirectionIndex);
        }
    }
}