using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class Switch : MonoBehaviour
{
    [Tooltip("このスイッチを特定するためのユニークID（Bridge.csと共通）")]
    public string switchID;

    [Header("判定対象レイヤー設定")]
    [Tooltip("スイッチが反応するオブジェクトが含まれるレイヤーを選択してください（複数選択可能）")]
    // LayerMask型を使用することで、Inspectorでチェックボックス選択が可能になります
    public LayerMask targetLayerMask;

    [Header("ビジュアル設定")]
    public SpriteRenderer spriteRenderer;
    public Sprite activatedSprite;   // 押されている間のSprite
    private Sprite defaultSprite;     // 押されていない間のSprite

    // スイッチの上に現在乗っているオブジェクトの数
    private int collisionCount = 0;

    void Start()
    {
        if (spriteRenderer != null)
        {
            defaultSprite = spriteRenderer.sprite;
        }

        // LayerMaskが全く設定されていない場合のチェック
        if (targetLayerMask.value == 0)
        {
            Debug.LogError("[Switch] 有効なターゲットレイヤーが設定されていません。InspectorでLayerMaskを選択してください。");
            enabled = false;
            return;
        }

        // 初期状態に設定（スイッチは最初から押されていない状態）
        UpdateVisuals(false);
    }

    /// <summary>
    /// 衝突したオブジェクトのレイヤーが、設定されたターゲットレイヤーのいずれかであるかを確認する
    /// LayerMaskによるビット演算チェックを行います。
    /// </summary>
    private bool IsTargetObject(GameObject obj)
    {
        int objLayer = obj.layer;

        // (targetLayerMask.value & (1 << objLayer)) != 0
        // LayerMaskにオブジェクトのレイヤーが含まれているかを確認
        return (targetLayerMask.value & (1 << objLayer)) != 0;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (IsTargetObject(collision.gameObject))
        {
            collisionCount++;

            // カウントが1になったら（何かが初めて乗ったら）スイッチON
            if (collisionCount == 1)
            {
                ToggleSwitchState(true);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (IsTargetObject(collision.gameObject))
        {
            if (collisionCount > 0)
            {
                collisionCount--;
            }

            // カウントが0になったら（全て離れたら）スイッチOFF
            if (collisionCount == 0)
            {
                ToggleSwitchState(false);
            }
        }
    }

    /// <summary>
    /// スイッチの状態を切り替え、橋の状態を同期する
    /// </summary>
    private void ToggleSwitchState(bool isActivated)
    {
        // 1. 見た目を変更
        UpdateVisuals(isActivated);

        // 2. 状態をSceneDataTransferに記録する
        if (SceneDataTransfer.Instance != null)
        {
            if (isActivated)
            {
                SceneDataTransfer.Instance.RecordSwitchActivated(switchID);
                Debug.Log($"[Switch] ID:{switchID} ON: ターゲットオブジェクトが乗りました。");
            }
            else
            {
                // 離されたら記録を削除
                SceneDataTransfer.Instance.activatedSwitchIDs.Remove(switchID);
                Debug.Log($"[Switch] ID:{switchID} OFF: ターゲットオブジェクトが離れました。");
            }
        }
    }

    private void UpdateVisuals(bool isActivated)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = isActivated ? activatedSprite : defaultSprite;
        }
    }
}