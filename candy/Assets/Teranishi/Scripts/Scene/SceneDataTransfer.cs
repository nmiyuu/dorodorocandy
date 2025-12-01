using UnityEngine;
using UnityEngine.SceneManagement;
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
    [HideInInspector] public int currentStageMoveCount = 0;
    [HideInInspector] public int movesOnClear = 0;

    [Header("ムーブブロックの状態")]
    [HideInInspector] public List<BlockState> pastBlockStates = new List<BlockState>();

    // --- アイテムとギミックの状態 ---
    [Header("アイテムとギミックの状態")]
    [HideInInspector] public bool hasMatchStick = false;                             // マッチ棒を持っているか (所持フラグ)
    [HideInInspector] public List<string> burnedObjectIDs = new List<string>();      // 燃やされて消えたオブジェクトのIDリスト (過去の変化)
    [HideInInspector] public List<string> vanishedItemIDs = new List<string>();      // 恒久的に消滅したアイテムのIDリスト (永続的な取得)
    // ----------------------------------------


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

    public void SavePlayerState(Vector3 pos, int dirIndex)
    {
        playerPositionToLoad = pos;
        playerDirectionIndexToLoad = dirIndex;
    }

    public void IncrementMoveCount()
    {
        currentStageMoveCount++;
        Debug.Log($"[SceneDataTransfer] 移動回数: {currentStageMoveCount}");
    }

    public void RecordStageClear(int clearedStageIndex)
    {
        if (clearedStageIndex > lastClearedStageIndex)
        {
            lastClearedStageIndex = clearedStageIndex;
        }
    }

    // --- ギミック関連のメソッド ---

    /// <summary>
    /// マッチ棒をアイテムとして取得したことを記録する
    /// </summary>
    public void AcquireMatchStick()
    {
        hasMatchStick = true;
        Debug.Log("[SceneDataTransfer] マッチ棒を取得しました。");
    }

    /// <summary>
    /// オブジェクトが燃やされて消えたことを記録する
    /// </summary>
    public void RecordBurnedObject(string objectId)
    {
        if (!burnedObjectIDs.Contains(objectId))
        {
            burnedObjectIDs.Add(objectId);
            Debug.Log($"[SceneDataTransfer] オブジェクトを燃やして消去記録: {objectId}");
        }
    }

    /// <summary>
    /// 指定されたオブジェクトが既に燃やされているか確認する
    /// </summary>
    public bool IsObjectBurned(string objectId)
    {
        return burnedObjectIDs.Contains(objectId);
    }

    /// <summary>
    /// アイテムがシーンから消滅したことを恒久的に記録する
    /// </summary>
    public void RecordItemVanished(string itemId)
    {
        if (!vanishedItemIDs.Contains(itemId))
        {
            vanishedItemIDs.Add(itemId);
            Debug.Log($"[SceneDataTransfer] アイテムを消去記録: {itemId}");
        }
    }

    /// <summary>
    /// 指定されたアイテムIDが既に消滅しているか確認する
    /// </summary>
    public bool IsItemVanished(string itemId)
    {
        return vanishedItemIDs.Contains(itemId);
    }
    // --------------------------------------

    // ------------------------------------
    // リセット処理
    // ------------------------------------

    public void SoftReset()
    {
        // ソフトリセット: 位置とブロック状態のみリセットし、移動回数は維持
        playerPositionToLoad = Vector3.zero;
        playerDirectionIndexToLoad = 0;
        pastBlockStates.Clear();
        Debug.Log("[SceneDataTransfer] ソフトリセットを実行しました (位置/ブロックリセット)。");
    }

    public void ClearPlayerState()
    {
        // ステージ切り替え時のリセット: 位置、ブロックデータ、移動回数をリセット
        playerPositionToLoad = Vector3.zero;
        playerDirectionIndexToLoad = 0;
        pastBlockStates.Clear();
        currentStageMoveCount = 0;

        // ギミックの状態（hasMatchStick, burnedObjectIDs, vanishedItemIDs）は維持
        // （次のステージに持ち越し、または永続的な状態のため）

        Debug.Log("[SceneDataTransfer] プレイヤーの状態、ブロックデータ、移動回数をリセットしました。");
    }

    public void FullGameReset()
    {
        // フルゲームリセット: 全てのゲーム進行データを初期化
        playerPositionToLoad = Vector3.zero;
        playerDirectionIndexToLoad = 0;
        pastBlockStates.Clear();
        currentStageMoveCount = 0;
        lastClearedStageIndex = 0;

        // ギミックの状態もリセット
        hasMatchStick = false;
        burnedObjectIDs.Clear();
        vanishedItemIDs.Clear(); // ★追加★

        Debug.Log("[SceneDataTransfer] フルゲームリセットを実行しました (全データ初期化)。");
    }

    // ------------------------------------
    // ブロックデータ処理 (省略なし)
    // ------------------------------------

    public void SaveBlockPositions(List<BlockState> statesToSave)
    {
        pastBlockStates = new List<BlockState>(statesToSave);
        Debug.Log($"[SceneDataTransfer] {pastBlockStates.Count} 個のブロックの状態（位置）を保存しました。");
    }

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

    public BlockState? GetBlockState(string id)
    {
        BlockState foundState = pastBlockStates.FirstOrDefault(state => state.id == id);

        if (pastBlockStates.Any(state => state.id == id))
        {
            return foundState;
        }
        return null;
    }
}