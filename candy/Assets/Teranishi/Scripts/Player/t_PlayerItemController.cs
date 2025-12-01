using UnityEngine;
using UnityEngine.InputSystem;

public class t_PlayerItemController : MonoBehaviour
{
    [Header("アイテム使用設定")]
    [Tooltip("アイテム使用時の判定距離")]
    public float interactionDistance = 1.0f;
    [Tooltip("燃やす対象があるレイヤー")]
    public LayerMask burnableLayer;

    // t_player と t_pl への参照を格納
    private t_player playerController;
    private t_pl playerAnimator; // t_pl の CurrentDirectionIndex を利用するため

    void Start()
    {
        playerController = GetComponent<t_player>();
        if (playerController != null)
        {
            // t_player スクリプトの public t_pl playerAnimScript; を介して参照を取得
            playerAnimator = playerController.playerAnimScript;
        }

        if (playerController == null || playerAnimator == null)
        {
            Debug.LogError("[PlayerItemController] t_player または t_pl の参照が見つかりません。PlayerItemControllerを無効化します。");
            enabled = false;
        }
    }

    void Update()
    {
        // 【修正】Zキーが押されたら、アイテム使用を試みる
        if (Keyboard.current != null && Keyboard.current.zKey.wasPressedThisFrame)
        {
            TryUseMatchStick();
        }
    }

    private void TryUseMatchStick()
    {
        if (SceneDataTransfer.Instance == null) return;

        // マッチ棒を持っていない場合は何もしない
        if (!SceneDataTransfer.Instance.hasMatchStick)
        {
            Debug.Log("[PlayerItemController] マッチ棒を持っていません。");
            return;
        }

        // プレイヤーの現在地と向きを取得
        Vector3 playerPos = transform.position;
        Vector3 playerForward = GetPlayerForwardDirection();

        // 前方にある BurnableObject をレイキャストでチェック
        RaycastHit2D hit = Physics2D.Raycast(playerPos, playerForward, interactionDistance, burnableLayer);

        if (hit.collider != null)
        {
            BurnableObject burnObject = hit.collider.GetComponent<BurnableObject>();

            if (burnObject != null)
            {
                // 燃焼処理を実行
                burnObject.Burn();

                // マッチ棒を消費する (消費後は hasMatchStick = false にする)
                SceneDataTransfer.Instance.hasMatchStick = false;

                Debug.Log("[PlayerItemController] マッチ棒を使用し、オブジェクトを燃やしました！");
            }
        }
    }

    // t_plスクリプトから現在の向きを取得し、Vector3に変換する
    private Vector3 GetPlayerForwardDirection()
    {
        if (playerAnimator == null) return Vector3.up; // 参照がない場合は安全のため上向きを返す

        // 【修正】t_plの公開プロパティ CurrentDirectionIndex を使用して、正確な向きを取得
        int dirIndex = playerAnimator.CurrentDirectionIndex;

        // t_player.csの方向インデックスに従って変換する (1:下, 2:上, 3:右, 4:左)
        switch (dirIndex)
        {
            case 1: return Vector3.down;
            case 2: return Vector3.up;
            case 3: return Vector3.right;
            case 4: return Vector3.left;
            default: return Vector3.up; // デフォルトは上
        }
    }
}