using UnityEngine;

/// スイッチ連動型の通行制御システム
/// 過去・未来間の状態同期およびプレイヤーの落下防止保護機能を実装
public class Bridge : MonoBehaviour
{
    // ======================================================================================
    // 設定項目
    // ======================================================================================

    [Header("Gimmick Settings")]
    [Tooltip("過去・未来で共通のスイッチ識別ID")]
    public string controllingSwitchID;

    [Header("Visual & Physics Components")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Collider2D bridgeCollider; // 通行制限用物理Collider

    [Space(10)]
    [SerializeField] private Sprite activatedSprite;   // 通行可能状態のスプライト
    [SerializeField] private Sprite deactivatedSprite; // 通行不可状態のスプライト

    // ======================================================================================
    // 内部変数
    // ======================================================================================

    private int playerCountOnBridge = 0; // 橋上の滞在プレイヤー数
    private bool lastState = false;      // 更新チェック用状態キャッシュ

    private int activatedLayer;   // 通行可能レイヤー (Default)
    private int deactivatedLayer; // 通行遮断レイヤー (Obstacle)

    private const string PlayerTag = "Player";

    // ======================================================================================
    // ライフサイクル
    // ======================================================================================

    private void Start()
    {
        if (spriteRenderer == null || bridgeCollider == null)
        {
            Debug.LogError($"[Bridge] コンポーネント参照不足: {gameObject.name}");
            enabled = false;
            return;
        }

        InitializeBridge();
    }

    private void Update()
    {
        CheckAndSynchronizeBridgeState();
    }

    // ======================================================================================
    // 内部ロジック
    // ======================================================================================

    /// レイヤー定義およびセーブデータに基づく初期状態の構築
    private void InitializeBridge()
    {
        if (SceneDataTransfer.Instance == null) return;

        activatedLayer = LayerMask.NameToLayer("Default");
        deactivatedLayer = LayerMask.NameToLayer("Obstacle");

        bool switchIsActivated = SceneDataTransfer.Instance.IsSwitchActivated(controllingSwitchID);
        ApplyState(switchIsActivated, true);
    }

    /// スイッチ状況とプレイヤー滞在判定の照合による物理状態同期
    private void CheckAndSynchronizeBridgeState()
    {
        if (SceneDataTransfer.Instance == null) return;

        bool switchIsActivated = SceneDataTransfer.Instance.IsSwitchActivated(controllingSwitchID);
        bool newState = switchIsActivated;

        /// プレイヤー保護：滞在中はスイッチOFFでも通行可能状態を維持（落下防止）
        if (!switchIsActivated && playerCountOnBridge > 0)
        {
            newState = true;
        }

        if (newState != lastState)
        {
            ApplyState(newState);
            lastState = newState;
        }
    }

    /// スプライト、当たり判定、レイヤーの物理的切り替え実行
    private void ApplyState(bool isActive, bool isInitial = false)
    {
        if (isActive)
        {
            spriteRenderer.sprite = activatedSprite;
            bridgeCollider.enabled = true;
            gameObject.layer = activatedLayer;
        }
        else
        {
            spriteRenderer.sprite = deactivatedSprite;
            bridgeCollider.enabled = false;
            gameObject.layer = deactivatedLayer;
        }
    }

    // ======================================================================================
    // 物理判定（Trigger Events）
    // ======================================================================================

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag(PlayerTag))
        {
            playerCountOnBridge++;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag(PlayerTag))
        {
            playerCountOnBridge--;
            playerCountOnBridge = Mathf.Max(0, playerCountOnBridge);
        }
    }
}