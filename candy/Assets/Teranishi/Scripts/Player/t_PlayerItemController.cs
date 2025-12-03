using UnityEngine;
using UnityEngine.InputSystem;

public class t_PlayerItemController : MonoBehaviour
{
    [Header("アイテム使用設定")]
    public float interactionDistance = 1.0f;
    public LayerMask burnableLayer;

    [Header("サウンド設定")]
    public AudioClip burnSE;               // ← 追加（燃える効果音）
    private AudioSource audioSource;       // ← 追加（SE を鳴らす AudioSource）

    private t_player playerController;
    private t_pl playerAnimator;

    void Start()
    {
        playerController = GetComponent<t_player>();
        if (playerController != null)
        {
            playerAnimator = playerController.playerAnimScript;
        }

        if (playerController == null || playerAnimator == null)
        {
            Debug.LogError("[PlayerItemController] t_player または t_pl の参照が見つかりません。");
            enabled = false;
        }

        // 🔊 AudioSource を取得（プレイヤーに AudioSource が必要）
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        // Enterキーでアイテム使用
        if (Keyboard.current != null && Keyboard.current.enterKey.wasPressedThisFrame)
        {
            TryUseMatchStick();
        }
    }

    private void TryUseMatchStick()
    {
        if (SceneDataTransfer.Instance == null) return;

        if (!SceneDataTransfer.Instance.hasMatchStick)
        {
            Debug.Log("[PlayerItemController] マッチ棒を持っていません。");
            return;
        }

        Vector3 playerPos = transform.position;
        Vector3 playerForward = GetPlayerForwardDirection();

        RaycastHit2D hit = Physics2D.Raycast(playerPos, playerForward, interactionDistance, burnableLayer);

        if (hit.collider != null)
        {
            BurnableObject burnObject = hit.collider.GetComponent<BurnableObject>();

            if (burnObject != null)
            {
                // 燃焼処理
                burnObject.Burn();

                // 🔥【追加】燃焼SEを鳴らす
                if (audioSource != null && burnSE != null)
                {
                    audioSource.PlayOneShot(burnSE);
                }

                // マッチ棒消費
                SceneDataTransfer.Instance.hasMatchStick = false;

                Debug.Log("[PlayerItemController] マッチ棒を使用し、オブジェクトを燃やしました！");
            }
        }
    }

    private Vector3 GetPlayerForwardDirection()
    {
        if (playerAnimator == null) return Vector3.up;

        int dirIndex = playerAnimator.CurrentDirectionIndex;

        switch (dirIndex)
        {
            case 1: return Vector3.down;
            case 2: return Vector3.up;
            case 3: return Vector3.right;
            case 4: return Vector3.left;
            default: return Vector3.up;
        }
    }
}
