using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

// シーンをまたいでブロックの状態を保持するための構造体
[Serializable]
public struct BlockState
{
    public string id;            // 対応するブロックのユニークID
    public Vector3 finalPosition; // ブロックが移動し終わった最終位置

    public BlockState(string id, Vector3 finalPosition)
    {
        this.id = id;
        this.finalPosition = finalPosition;
    }
}

public class SceneDataTransfer : MonoBehaviour
{
    // --- シングルトンパターン ---
    public static SceneDataTransfer Instance { get; private set; }

    // ------------------------------------
    // 転送データ
    // ------------------------------------

    [Header("プレイヤーの状態")]
    [HideInInspector] public Vector3 playerPositionToLoad = Vector3.zero;
    [HideInInspector] public int playerDirectionIndexToLoad = 0;

    [Header("ゲーム進行状況と移動回数")]
    [HideInInspector] public int lastClearedStageIndex = 0;
    [HideInInspector] public int currentStageMoveCount = 0; // 現在のステージの移動回数 (ソフトリセットで維持)
    [HideInInspector] public int movesOnClear = 0;          // ゴールシーンへ渡すための最終移動回数

    [Header("ムーブブロックの状態")]
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
            DontDestroyOnLoad(gameObject);
        }
    }

    // ------------------------------------
    // プレイヤーとステージの状態操作
    // ------------------------------------

    /// <summary>
    /// シーン遷移前にプレイヤーの位置と向きを保存します。
    /// </summary>
    public void SavePlayerState(Vector3 pos, int dirIndex)
    {
        playerPositionToLoad = pos;
        playerDirectionIndexToLoad = dirIndex;
    }

    /// <summary>
    /// 移動回数を1加算します。
    /// </summary>
    public void IncrementMoveCount()
    {
        currentStageMoveCount++;
        Debug.Log($"[SceneDataTransfer] 移動回数: {currentStageMoveCount}");
    }

    /// <summary>
    /// クリアしたステージのインデックスを記録します。
    /// </summary>
    public void RecordStageClear(int clearedStageIndex)
    {
        if (clearedStageIndex > lastClearedStageIndex)
        {
            lastClearedStageIndex = clearedStageIndex;
        }
    }

    // ------------------------------------
    // リセット処理
    // ------------------------------------

    /// <summary>
    /// 【ソフトリセット】ステージ内でのリセット(Rキー)に使用。移動回数は維持します。
    /// </summary>
    public void SoftReset()
    {
        playerPositionToLoad = Vector3.zero;
        playerDirectionIndexToLoad = 0;
        pastBlockStates.Clear();
        // currentStageMoveCount は維持
        Debug.Log("[SceneDataTransfer] ソフトリセットを実行しました (位置/ブロックリセット)。");
    }

    /// <summary>
    /// 【ハードリセット】次のステージへ進む際などに使用。全てのステージ進行データ、移動回数をリセットします。
    /// </summary>
    public void ClearPlayerState()
    {
        playerPositionToLoad = Vector3.zero;
        playerDirectionIndexToLoad = 0;
        pastBlockStates.Clear();
        // ステージ切り替え時なので移動回数もリセット
        currentStageMoveCount = 0;

        Debug.Log("[SceneDataTransfer] プレイヤーの状態、ブロックデータ、移動回数をリセットしました。");
    }

    // ------------------------------------
    // ブロックデータ処理
    // ------------------------------------

    /// <summary>
    /// 過去シーンから現在シーンに戻る際に、ブロックの位置のリストを上書き保存する
    /// </summary>
    public void SaveBlockPositions(List<BlockState> statesToSave)
    {
        pastBlockStates = new List<BlockState>(statesToSave);
        Debug.Log($"[SceneDataTransfer] {pastBlockStates.Count} 個のブロックの状態（位置）を保存しました。");
    }

    /// <summary>
    /// ブロックの状態を保存または更新する (移動完了時に MoveBlock から呼ばれる)。
    /// </summary>
    public void AddOrUpdateBlockState(string blockId, Vector3 finalPos)
    {
        int index = pastBlockStates.FindIndex(state => state.id == blockId);

        if (index != -1)
        {
            BlockState updatedState = pastBlockStates[index];
            updatedState.finalPosition = finalPos;
            pastBlockStates[index] = updatedState;
        }
        else
        {
            pastBlockStates.Add(new BlockState(blockId, finalPos));
        }
    }

    /// <summary>
    /// 指定されたIDのブロックの保存された状態を取得する。
    /// </summary>
    public BlockState? GetBlockState(string id)
    {
        BlockState foundState = pastBlockStates.FirstOrDefault(state => state.id == id);

        if (pastBlockStates.Any(state => state.id == id))
        {
            return foundState;
        }
        return null;
    }

    public void FullGameReset()
    {
        // プレイヤーの状態をリセット
        playerPositionToLoad = Vector3.zero;
        playerDirectionIndexToLoad = 0;

        // ブロックの状態リストをクリア
        pastBlockStates.Clear();

        // 移動回数をリセット
        currentStageMoveCount = 0;

        // ★ステージクリア状況を初期値（0）にリセット★
        lastClearedStageIndex = 0;

        Debug.Log("[SceneDataTransfer] フルゲームリセットを実行しました (全データ初期化)。");
    }
}