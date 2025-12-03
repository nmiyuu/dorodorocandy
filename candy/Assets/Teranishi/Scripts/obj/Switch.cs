using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class Switch : MonoBehaviour
{
    // ... (既存のユニークID、LayerMask、ビジュアル設定のフィールド) ...

    [Tooltip("このスイッチを特定するためのユニークID（Bridge.csと共通）")]
    public string switchID;

    [Header("判定対象レイヤー設定")]
    [Tooltip("スイッチが反応するオブジェクトが含まれるレイヤーを選択してください（複数選択可能）")]
    public LayerMask targetLayerMask;

    [Header("ビジュアル設定")]
    public SpriteRenderer spriteRenderer;
    public Sprite activatedSprite;      // 押されている間のSprite
    private Sprite defaultSprite;      // 押されていない間のSprite

    // ★★★ 追加: サウンド設定 ★★★
    [Header("サウンド設定")]
    [Tooltip("スイッチがONになった瞬間に鳴る音")]
    public AudioClip pressSE;
    private AudioSource audioSource;
    // ★★★★★★★★★★★★★

    // スイッチの上に現在乗っているオブジェクトの数
    private int collisionCount = 0;

    void Awake()
    {
        // ★ AudioSourceを取得/追加 ★
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
    }

    void Start()
    {
        if (spriteRenderer != null)
        {
            defaultSprite = spriteRenderer.sprite;
        }

        if (targetLayerMask.value == 0)
        {
            Debug.LogError("[Switch] 有効なターゲットレイヤーが設定されていません。InspectorでLayerMaskを選択してください。");
            enabled = false;
            return;
        }

        UpdateVisuals(false);
    }

    private bool IsTargetObject(GameObject obj)
    {
        int objLayer = obj.layer;
        return (targetLayerMask.value & (1 << objLayer)) != 0;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (IsTargetObject(collision.gameObject))
        {
            // collisionCountが0から1に変わる瞬間が、スイッチONのタイミング
            bool wasOff = collisionCount == 0;

            collisionCount++;

            // カウントが1になったら（何かが初めて乗ったら）スイッチON
            if (wasOff && collisionCount == 1)
            {
                // ★ SE再生 (踏んだ時の音) ★
                PlaySwitchSE(pressSE);

                ToggleSwitchState(true);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (IsTargetObject(collision.gameObject))
        {
            // collisionCountが1から0に変わる瞬間が、スイッチOFFのタイミング
            bool willBeOff = collisionCount == 1;

            if (collisionCount > 0)
            {
                collisionCount--;
            }

            // カウントが0になったら（全て離れたら）スイッチOFF
            if (willBeOff && collisionCount == 0)
            {
                // ここで離れた時のSEを鳴らすことも可能ですが、今回は踏んだ時のみに限定します。

                ToggleSwitchState(false);
            }
        }
    }

    // ... (ToggleSwitchState, UpdateVisuals は変更なし) ...

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

    // ★★★ 追加: SE再生ヘルパー関数 ★★★
    private void PlaySwitchSE(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
}