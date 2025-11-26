using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;

// 必要なデータ構造の定義（FutureObstacleControllerに合わせて追記）
// シーンをまたいでブロックの状態を保持するための構造体
[System.Serializable]
public struct BlockState
{
    public string id;           // 対応するブロックのユニークID
    public Vector3 finalPosition; // 過去のブロックが移動し終わった最終位置
    // 必要に応じて、穴が埋まったかどうかの bool 状態もここに追加可能
}

public class SceneDataTransfer : MonoBehaviour
{
    // --- シングルトンパターン ---
    public static SceneDataTransfer Instance { get; private set; }

    // --- 転送データ ---

    // プレイヤーの復帰位置
    [HideInInspector] public Vector3 playerPositionToLoad = Vector3.zero;

    // プレイヤーの向きのインデックス
    [HideInInspector] public int playerDirectionIndexToLoad = 0;

    //  ブロックの状態を List<BlockState> で保持する
    [HideInInspector] public List<BlockState> pastBlockStates = new List<BlockState>();

    // --- Unityライフサイクル ---

    void Awake()
    {
        Application.targetFrameRate = 60;

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            // シーンをまたいでオブジェクトを保持する（永続化）
            DontDestroyOnLoad(gameObject);
        }
    }

    // --- ブロックデータ処理 ---

    // 過去シーンから現在シーンに戻る際に呼ばれる（TimeTravelControllerから呼ばれる）
    // ブロックの位置を保存する処理に特化させる
    public void SaveBlockPositions(List<BlockState> statesToSave)
    {
        // 以前のデータを完全に上書き
        pastBlockStates = new List<BlockState>(statesToSave);

        Debug.Log($"[SceneDataTransfer] {pastBlockStates.Count} 個のブロックの状態（位置）を保存しました。");
    }

    // FutureObstacleController からブロックが動いた後の位置を保存するメソッド
    // （過去の MoveBlock から呼ばれることを想定）
    public void AddOrUpdateBlockState(string blockId, Vector3 finalPos)
    {
        // 既存の状態を検索
        int index = pastBlockStates.FindIndex(state => state.id == blockId);

        if (index != -1)
        {
            // 既にIDが存在する場合、位置を更新
            BlockState updatedState = pastBlockStates[index];
            updatedState.finalPosition = finalPos;
            pastBlockStates[index] = updatedState;
        }
        else
        {
            // 新しい状態として追加
            pastBlockStates.Add(new BlockState { id = blockId, finalPosition = finalPos });
        }
    }

    public void FullReset()
    {
        // プレイヤーの位置を初期値 (0, 0, 0) にリセット
        playerPositionToLoad = Vector3.zero;

        // プレイヤーの向きを初期値 (0) にリセット
        playerDirectionIndexToLoad = 0;

        // ブロックの状態リストをクリア
        pastBlockStates.Clear();

        Debug.Log("[SceneDataTransfer] ゲーム状態を完全にリセットしました。");
    }
}