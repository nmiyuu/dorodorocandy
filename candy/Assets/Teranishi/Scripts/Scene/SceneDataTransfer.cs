using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;
using System;

// シーンをまたいでブロックの状態を保持するための構造体
[Serializable]
public struct BlockState
{
    public string id;              // 対応するブロックのユニークID
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

    // スイッチと橋の状態
    [Header("スイッチと橋の状態")]
    [HideInInspector] public List<string> activatedSwitchIDs = new List<string>();       // 押されたスイッチのユニークIDを保存するリスト

    // シーン遷移の状態
    [Header("シーン状態")]
    [HideInInspector] public bool isChangingScene = false; // ★このフラグが t_player.cs で参照されます★

    //ナビキャラクターの会話フラグ
    [HideInInspector] public bool isTalking = false;

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
    // シーン遷移管理メソッド 
    // ------------------------------------

    /// <summary>
    /// シーン遷移を開始する際に呼び出す（フェードアウト開始時）
    /// </summary>
    public void StartSceneChange()
    {
        isChangingScene = true;
        Debug.Log("[SceneDataTransfer] シーン変更処理を開始しました。移動/操作を無効化。");
    }

    /// <summary>
    /// シーン遷移が完了した際に呼び出す（フェードイン完了時）
    /// </summary>
    public void EndSceneChange()
    {
        isChangingScene = false;
        Debug.Log("[SceneDataTransfer] シーン変更処理を完了しました。移動/操作を有効化。");
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

    // --- マッチ棒関連 ---
    public void AcquireMatchStick()
    {
        hasMatchStick = true;
        Debug.Log("[SceneDataTransfer] マッチ棒を取得しました。");
    }

    public void RecordBurnedObject(string objectId)
    {
        if (!burnedObjectIDs.Contains(objectId))
        {
            burnedObjectIDs.Add(objectId);
            Debug.Log($"[SceneDataTransfer] オブジェクトを燃やして消去記録: {objectId}");
        }
    }

    public bool IsObjectBurned(string objectId)
    {
        return burnedObjectIDs.Contains(objectId);
    }

    public void RecordItemVanished(string itemId)
    {
        if (!vanishedItemIDs.Contains(itemId))
        {
            vanishedItemIDs.Add(itemId);
            Debug.Log($"[SceneDataTransfer] アイテムを消去記録: {itemId}");
        }
    }

    public bool IsItemVanished(string itemId)
    {
        return vanishedItemIDs.Contains(itemId);
    }

    // --- スイッチ/橋関連 ---

    /// <summary>
    /// スイッチが押されたことを恒久的に記録する
    /// </summary>
    public void RecordSwitchActivated(string switchId)
    {
        if (!activatedSwitchIDs.Contains(switchId))
        {
            activatedSwitchIDs.Add(switchId);
            Debug.Log($"[SceneDataTransfer] スイッチを起動記録: {switchId}");
        }
    }

    /// <summary>
    /// 指定されたスイッチIDが既に押されているか確認する
    /// </summary>
    public bool IsSwitchActivated(string switchId)
    {
        return activatedSwitchIDs.Contains(switchId);
    }

    // ------------------------------------
    // リセット処理
    // ------------------------------------

    public void SoftReset()
    {
        // --- プレイヤーの状態をリセット ---
        playerPositionToLoad = Vector3.zero;
        playerDirectionIndexToLoad = 0;

        // --- ブロックの状態をリセット ---
        pastBlockStates.Clear();

        // --- ステージ内ギミック状態リセット ---
        burnedObjectIDs.Clear();      // 燃やしたオブジェクトを元に戻す
        vanishedItemIDs.Clear();      // 一時的に消えたアイテムを戻す
        activatedSwitchIDs.Clear();   // 押されたスイッチを元に戻す

        // マッチ棒をステージ内アイテム扱いにしたい場合はリセットする
        hasMatchStick = false;

        //リセットしたときに会話フラグをオフに
        isTalking = false;

        Debug.Log("[SceneDataTransfer] ソフトリセットを実行しました (位置/ギミック/ブロックリセット、移動回数維持)。");
    }

    public void ClearPlayerState()
    {
        // ステージ切り替え時のリセット: 位置、ブロックデータ、移動回数をリセット
        playerPositionToLoad = Vector3.zero;
        playerDirectionIndexToLoad = 0;
        pastBlockStates.Clear();
        currentStageMoveCount = 0;

        // ギミックの状態は維持

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
       isTalking = false;
       burnedObjectIDs.Clear();
       vanishedItemIDs.Clear();
       activatedSwitchIDs.Clear();

        Debug.Log("[SceneDataTransfer] フルゲームリセットを実行しました (全データ初期化)。");
    }

    // ------------------------------------
    // ブロックデータ処理 
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