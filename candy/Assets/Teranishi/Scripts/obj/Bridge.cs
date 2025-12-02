using UnityEngine;

public class Bridge : MonoBehaviour
{
    [Tooltip("対応するSwitchのユニークID（Bridge_PastとBridge_Futureで共通）")]
    public string controllingSwitchID;

    [Header("ビジュアルと衝突")]
    public SpriteRenderer spriteRenderer;
    public Collider2D bridgeCollider; // プレイヤーの移動を妨げる本来のCollider (Is Trigger: OFF)
    public Sprite activatedSprite;
    public Sprite deactivatedSprite;

    // 橋の上にいるプレイヤーの数 (Is Trigger ONの検出用Colliderでカウントされる)
    private int playerCountOnBridge = 0;
    private bool lastState = false;

    // レイヤー設定
    private int activatedLayer;   // 橋が有効な時のレイヤー
    private int deactivatedLayer; // 橋が無効な時のレイヤー

    private const string PlayerTag = "Player";

    void Start()
    {
        if (spriteRenderer == null || bridgeCollider == null)
        {
            Debug.LogError($"[Bridge] コンポーネント参照が不足しています。オブジェクト名: {gameObject.name}");
            enabled = false;
            return;
        }

        if (SceneDataTransfer.Instance == null) return;

        // ★★★ レイヤー設定を反転 ★★★
        // スイッチON時（橋が繋がったとき）: Defaultレイヤー
        activatedLayer = LayerMask.NameToLayer("Default");
        // スイッチOFF時（橋が壊れたとき）: Obstacleレイヤー (プレイヤーをブロックする)
        deactivatedLayer = LayerMask.NameToLayer("Obstacle");

        // 初期状態設定: スイッチの記録状態に厳密に従う（プレイヤー保護は無視）
        bool switchIsActivated = SceneDataTransfer.Instance.IsSwitchActivated(controllingSwitchID);

        if (switchIsActivated)
        {
            // 初期状態：ON (繋がっている)
            spriteRenderer.sprite = activatedSprite;
            bridgeCollider.enabled = true;
            gameObject.layer = activatedLayer; // Defaultレイヤーに設定
            lastState = true;
        }
        else
        {
            // 初期状態：OFF (壊れている)
            spriteRenderer.sprite = deactivatedSprite;
            bridgeCollider.enabled = false;
            gameObject.layer = deactivatedLayer; // Obstacleレイヤーに設定
            lastState = false;
        }

        Debug.Log($"[Bridge] 初期状態設定完了。ID:{controllingSwitchID} は {(lastState ? "有効 (Default)" : "無効 (Obstacle)")}でスタートします。");
    }

    void Update()
    {
        CheckAndSynchronizeBridgeState();
    }

    // プレイヤーが橋の上に乗っているかを判定するためのトリガーイベント
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag(PlayerTag))
        {
            playerCountOnBridge++;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag(PlayerTag))
        {
            playerCountOnBridge--;
            if (playerCountOnBridge < 0) playerCountOnBridge = 0;
        }
    }


    /// <summary>
    /// スイッチの状態を確認し、橋の状態を過去・未来間で同期する
    /// </summary>
    private void CheckAndSynchronizeBridgeState()
    {
        if (SceneDataTransfer.Instance == null) return;

        bool switchIsActivated = SceneDataTransfer.Instance.IsSwitchActivated(controllingSwitchID);

        bool newState = switchIsActivated;

        // ★プレイヤー保護ロジック: スイッチOFFでも、プレイヤーが橋の上にいる間は強制的にON状態を維持する★
        // ここでの「ON状態を維持」とは、橋が繋がった状態（Activated状態）を維持すること。
        if (!switchIsActivated && playerCountOnBridge > 0)
        {
            newState = true; // 強制的にON状態を維持
        }

        // 状態が変化していない場合はスキップ
        if (newState == lastState) return;


        if (newState)
        {
            // --- 有効化（スイッチON）：橋が繋がる ---
            spriteRenderer.sprite = activatedSprite;
            bridgeCollider.enabled = true;
            // ★レイヤーを Default に設定★
            gameObject.layer = activatedLayer;

            Debug.Log($"[Bridge] ID:{controllingSwitchID} 有効化 (Default)。");
        }
        else
        {
            // --- 無効化（スイッチOFF）：橋が壊れる ---
            spriteRenderer.sprite = deactivatedSprite;
            bridgeCollider.enabled = false;
            // ★レイヤーを Obstacle に設定★
            gameObject.layer = deactivatedLayer;

            Debug.Log($"[Bridge] ID:{controllingSwitchID} 無効化 (Obstacle)。");
        }

        Debug.Log($"[Future Bridge Sync] ID:{controllingSwitchID} newStateが {newState} に変化。プレイヤーカウント: {playerCountOnBridge}");

        lastState = newState;
    }
}