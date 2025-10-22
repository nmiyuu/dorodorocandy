using UnityEngine;
using System.Linq;

public class FutureObstacleController : MonoBehaviour
{
    // 過去のMoveBlockと対応するためのID
    // 過去のMoveBlockのblockIDと全く同じIDを設定する必要があります
    [Tooltip("対応する過去のMoveBlockと同じユニークIDを設定してください。")]
    public string blockID;

    void Start()
    {
        // 1. SceneDataTransferが存在するか確認
        if (SceneDataTransfer.Instance == null)
        {
            Debug.LogError("SceneDataTransfer が見つかりません。");
            return;
        }

        // 2. 過去のブロックの保存位置を SceneDataTransfer から検索
        // pastBlockStates は過去シーンのブロックが移動した後の位置を保持しています。
        BlockState? savedState = SceneDataTransfer.Instance.pastBlockStates
            .Where(state => state.id == blockID)
            .Cast<BlockState?>()
            .FirstOrDefault();

        // 3. 保存データが存在する場合、位置を強制的に同期
        if (savedState.HasValue)
        {
            Vector3 finalPosition = savedState.Value.finalPosition;

            // 過去のブロックが動いた後の位置に、未来の動かせないブロックを強制移動させる
            transform.position = finalPosition;
            Debug.Log($"未来の静的障害物 '{blockID}' を、過去の移動位置 {finalPosition} に同期しました。");
        }
        else
        {
            // 過去のブロックが一度も動かされていない（データが保存されていない）場合、
            // この未来の障害物はシーンに配置されたデフォルト位置に留まります。
        }
    }
}