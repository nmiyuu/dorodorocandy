using UnityEngine;
using UnityEngine.InputSystem;

public class t_pl : MonoBehaviour
{
    private Animator animator;
    // lastDirectionをint（インデックス）で保持
    public int lastDirectionIndex = 1; // デフォルトは1（下）

    void Awake()
    {
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError("Playerオブジェクトに Animator コンポーネントが見つかりません。");
        }
    }

    void Start()
    {
        // SceneDataTransferが存在し、かつ保存された向きのインデックスがある場合（0ではない場合）
        if (SceneDataTransfer.Instance != null && SceneDataTransfer.Instance.playerDirectionIndexToLoad != 0)
        {
            // ★★★ 復元データを即座にロードし、アニメーターを更新 ★★★

            // 1. lastDirectionIndexをロードデータで上書き
            lastDirectionIndex = SceneDataTransfer.Instance.playerDirectionIndexToLoad;

            // 2. Animatorを更新（これにより、見た目がすぐに変わる）
            UpdateAnimator(lastDirectionIndex);

            // 補足: TimeTravelControllerの復元処理を待つ必要がなくなりますが、
            // TimeTravelController側の復元処理は念のためそのまま残しておきます。
        }
        else
        {
            // SceneDataTransferがない、またはロードデータがない場合は、
            // デフォルトの lastDirectionIndex (1:下) でAnimatorを初期化
            UpdateAnimator(lastDirectionIndex);
        }
    }

    void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        int newIndex = 0;

        // t_playerスクリプトの参照が有効か確認（念のため）
        t_player playerMovementScript = GetComponent<t_player>();
        bool isMoving = playerMovementScript != null && playerMovementScript.IsPlayerMoving;


        // キー入力に応じてインデックスを決定
        if (keyboard.downArrowKey.isPressed)
        {
            newIndex = 1; // 下 = 1
        }
        else if (keyboard.upArrowKey.isPressed)
        {
            newIndex = 2; // 上 = 2
        }
        else if (keyboard.rightArrowKey.isPressed)
        {
            newIndex = 3; // 右 = 3
        }
        else if (keyboard.leftArrowKey.isPressed)
        {
            newIndex = 4; // 左 = 4
        }

        // ★重要: 移動中か、新しい方向が検出された場合にのみlastDirectionIndexを更新する
        if (newIndex != 0)
        {
            SetDirection(newIndex);
        }

        // ★アニメーターの更新は毎フレーム行う
        //   これにより、Animatorが次のステートに切り替わった後、
        //   そのステート（例: Idle_Right）を維持するためにDirection=3を送り続けます。
        UpdateAnimator(lastDirectionIndex);

        // ※ 従来の else ブロックは不要になりました。
    }
    void SetDirection(int index)
    {
        lastDirectionIndex = index;
        UpdateAnimator(index);
    }

    // AnimatorのIntパラメーターを更新するメソッド
    void UpdateAnimator(int index)
    {
        if (animator == null) return;

        // Intパラメーター"Direction"を設定
        animator.SetInteger("Direction", index);
    }

    // 外部から向きを復元するためのPublicメソッド (TimeTravelControllerから呼ばれる)
    public void LoadDirectionIndex(int index)
    {
        SetDirection(index);
    }

    // 現在の向きのインデックスを取得するPublicプロパティ
    public int CurrentDirectionIndex
    {
        get { return lastDirectionIndex; }
    }
}