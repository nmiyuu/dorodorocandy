using UnityEngine;
using UnityEngine.InputSystem;

public class t_pl : MonoBehaviour
{
    // ★SpriteRendererとSpriteのpublicフィールドは不要です（手動で削除推奨）
    // ただし、もしt_plのインスペクターにSpriteRendererが残っていても、このコードはエラーになりません。

    private Animator animator; // ★Animatorへの参照
    public Vector2 lastDirection = Vector2.down; // 最後に見ていた向き

    void Awake()
    {
        // Animatorコンポーネントを取得
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError("Playerオブジェクトに Animator コンポーネントが見つかりません。");
        }
    }

    void Start()
    {
        // StartではAnimatorに現在の向きを設定する
        // 復元データがある場合はTimeTravelControllerからのLoadDirectionに任せるため、ここではデフォルト設定のみ
        if (SceneDataTransfer.Instance == null || SceneDataTransfer.Instance.playerDirectionToLoad == Vector2.zero)
        {
            UpdateAnimator(lastDirection);
        }
    }

    void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        Vector2 inputDir = Vector2.zero;

        // キーが押されているかをチェックし、入力方向を決定
        // wasPressedThisFrame (瞬間) と isPressed (押しっぱなし) の両方に対応
        if (keyboard.upArrowKey.isPressed)
        {
            inputDir = Vector2.up;
        }
        else if (keyboard.downArrowKey.isPressed)
        {
            inputDir = Vector2.down;
        }
        else if (keyboard.leftArrowKey.isPressed)
        {
            inputDir = Vector2.left;
        }
        else if (keyboard.rightArrowKey.isPressed)
        {
            inputDir = Vector2.right;
        }

        if (inputDir != Vector2.zero)
        {
            SetDirection(inputDir);
        }
        else
        {
            // キー入力がない場合でも、Animatorを更新し、最後の向きを維持させる
            UpdateAnimator(lastDirection);
        }
    }

    void SetDirection(Vector2 dir)
    {
        lastDirection = dir;
        UpdateAnimator(dir);
    }

    // Animatorのパラメーターを更新するメソッド
    void UpdateAnimator(Vector2 dir)
    {
        if (animator == null) return;

        // AnimatorのMoveXとMoveYに現在の向きの成分を設定し、画像を切り替える
        // Animator Controllerで定義したパラメーター名("MoveX", "MoveY")と一致させる必要があります
        animator.SetFloat("MoveX", dir.x);
        animator.SetFloat("MoveY", dir.y);
    }

    // 外部から向きを復元するためのPublicメソッド (TimeTravelControllerから呼ばれる)
    public void LoadDirection(Vector2 direction)
    {
        // 向きを設定し、直ちにAnimatorに反映させる
        SetDirection(direction);
    }

    // 現在の向きを取得するPublicプロパティ
    public Vector2 CurrentDirection
    {
        get { return lastDirection; }
    }
}