using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

// プレイヤーの移動と衝突判定を全部やるコアスクリプト。
public class t_player : MonoBehaviour
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

        if (playerCollider == null) Debug.LogError("[t_player] BoxCollider2Dがない");
        if (playerAnimScript == null) Debug.LogError("[t_player] t_plがない");

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
            // リセット処理後、次のフェードロック処理に任せる
        }

        if (playerAnimScript == null || Keyboard.current == null) return;

        // フェードイン/アウト中はすべての移動・操作をブロックする
        if (SceneFader.Instance != null && SceneFader.Instance.IsFading)
        {
            // フェード中は移動も向きの更新もしない
            return;
        }

        // アニメーション向きを最新に更新する
        // 移動判定の前に、このフレームでどのキーが最後に押されたか計算し、t_plに教えて同期させる
        int newDirectionIndex = CalculateNewDirection();
        playerAnimScript.SetDirectionFromExternal(newDirectionIndex);

        // 移動中は入力を受け付けない
        if (isMoving) return;

        // 移動トリガーの判定 (長押し防止のためwasPressedThisFrameを使う)
        bool keyWasPressed = Keyboard.current.upArrowKey.wasPressedThisFrame ||
                             Keyboard.current.downArrowKey.wasPressedThisFrame ||
                             Keyboard.current.leftArrowKey.wasPressedThisFrame ||
                             Keyboard.current.rightArrowKey.wasPressedThisFrame;

        if (!keyWasPressed) return;

        // 計算した最新の向き（newDirectionIndex）で移動方向を決定
        Vector3 dir = ConvertDirectionIndexToVector(newDirectionIndex);

        // 衝突判定と移動の実行
        if (dir != Vector3.zero)
        {
            Vector2 origin = (Vector2)transform.position + playerCollider.offset;
            Vector2 size = playerCollider.size;
            float angle = 0f;
            float checkDistance = moveUnit * 1.01f; // ちょい長めにチェック

            RaycastHit2D hit = Physics2D.BoxCast(origin, size, angle, dir, checkDistance, obstacleLayer);

            if (hit.collider == null)
            {
                // 何もないなら移動する
                targetPos = transform.position + dir * moveUnit;
                StartCoroutine(MoveToPosition(targetPos));
            }
            else
            {
                // 何かにぶつかったら、それがブロックかチェックする
                GameObject hitObject = hit.collider.gameObject;
                int moveBlockLayerIndex = LayerMask.NameToLayer("MoveBlock");

                if (hitObject.layer == moveBlockLayerIndex)
                {
                    MoveBlock blockToPush = hitObject.GetComponent<MoveBlock>();
                    if (blockToPush != null && blockToPush.TryMove(dir))
                    {
                        // ブロックを動かせたから、自分も移動する
                        targetPos = transform.position + dir * moveUnit;
                        StartCoroutine(MoveToPosition(targetPos));
                    }
                    // ブロックが動かせなかったら、自分も止まる
                }
                // 壁とかだったら、何もせず止まる
            }
        }
    }

    // --- プライベートメソッド (ロジック統合部分) ---

    // 今押されているキーの中で、一番最後に押されたキーの向きインデックスを計算して返す。
    private int CalculateNewDirection()
    {
        var keyboard = Keyboard.current;
        List<int> pressedDirections = new List<int>();

        // 押されているキーのタイムスタンプを更新する
        if (keyboard.downArrowKey.isPressed) { lastKeyPressTime[1] = Time.time; pressedDirections.Add(1); }
        if (keyboard.upArrowKey.isPressed) { lastKeyPressTime[2] = Time.time; pressedDirections.Add(2); }
        if (keyboard.rightArrowKey.isPressed) { lastKeyPressTime[3] = Time.time; pressedDirections.Add(3); }
        if (keyboard.leftArrowKey.isPressed) { lastKeyPressTime[4] = Time.time; pressedDirections.Add(4); }

        if (pressedDirections.Count == 0)
        {
            // キーが何も押されてないなら、t_plが持ってる今の向きをそのまま返す
            return playerAnimScript.CurrentDirectionIndex;
        }

        // 最後に押されたキーを特定するロジック
        int preferredIndex = 0;
        float latestTime = -1f;

        foreach (int index in pressedDirections)
        {
            // Time.timeが大きい方が最新（最後に押されたキー）
            if (lastKeyPressTime[index] > latestTime)
            {
                latestTime = lastKeyPressTime[index];
                preferredIndex = index;
            }
        }
        return preferredIndex;
    }

    // 向きのインデックス（1～4）をUnityのVector3（方向ベクトル）に変換する関数。
    private Vector3 ConvertDirectionIndexToVector(int index)
    {
        switch (index)
        {
            case 1: return Vector3.down;
            case 2: return Vector3.up;
            case 3: return Vector3.right;
            case 4: return Vector3.left;
            default: return Vector3.zero; // 0なら移動なし
        }
    }


    // Rキーで呼ばれる完全リセット機能。
    // シングルトン内のデータをクリアし、基準シーンを再ロードする。
    private void FullSceneReset()
    {
        // シングルトンのデータ（過去/未来の状態）をリセットする
        if (SceneDataTransfer.Instance != null)
        {
            SceneDataTransfer.Instance.FullReset();
        }

        // シーン再ロード処理を SceneFader に置き換える
        if (SceneFader.Instance != null)
        {
            // Rキーリセットは黒フェードを指定する
            SceneFader.Instance.LoadSceneWithFade(resetSceneName, FadeColor.Black);
        }
        else
        {
            // フォールバック時
            SceneManager.LoadScene(resetSceneName);
        }

        Debug.Log($"シーン '{resetSceneName}' をRキーで完全リセットする (基準シーンへ)");
    }

    // --- コルーチン ---

    IEnumerator MoveToPosition(Vector3 target)
    {
        isMoving = true;

        // 目的地にほぼ着くまで移動を続ける
        while ((transform.position - target).sqrMagnitude > 0.001f)
        {
            transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
            yield return null; // 1フレーム待つ
        }

        // 最後に目的地にピタッと合わせる
        transform.position = target;
        isMoving = false;
    }
}