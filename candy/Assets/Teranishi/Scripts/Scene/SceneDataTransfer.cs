using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;
using System; // [Serializable] に必要

// 必要なデータ構造の定義
// シーンをまたいでブロックの状態を保持するための構造体
[Serializable]
public struct BlockState
{
    public string id;            // 対応するブロックのユニークID
    public Vector3 finalPosition; // 過去のブロックが移動し終わった最終位置

    // コンストラクタ
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

    // --- 転送データ ---

    // プレイヤーの復帰位置
    [HideInInspector] public Vector3 playerPositionToLoad = Vector3.zero;

    // プレイヤーの向きのインデックス
    [HideInInspector] public int playerDirectionIndexToLoad = 0;

    // ブロックの状態を List<BlockState> で保持する
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

    // ------------------------------------
    // プレイヤーデータ処理
    // ------------------------------------

    /// <summary>
    /// シーン遷移前にプレイヤーの位置と向きを保存します。
    /// </summary>
    /// <param name="pos">プレイヤーの最終的な位置。</param>
    /// <param name="dirIndex">プレイヤーの向きのインデックス。</param>
    public void SavePlayerState(Vector3 pos, int dirIndex)
    {
        playerPositionToLoad = pos;
        playerDirectionIndexToLoad = dirIndex;
        Debug.Log($"[SceneDataTransfer] プレイヤーの状態を保存しました: Pos={pos}, Dir={dirIndex}");
    }

    // ------------------------------------
    // ブロックデータ処理
    // ------------------------------------

    /// <summary>
    /// 過去シーンから現在シーンに戻る際に呼ばれる（TimeTravelControllerから呼ばれる）
    /// ブロックの位置を保存する処理に特化させる
    /// </summary>
    public void SaveBlockPositions(List<BlockState> statesToSave)
    {
        // 以前のデータを完全に上書き
        pastBlockStates = new List<BlockState>(statesToSave);

        Debug.Log($"[SceneDataTransfer] {pastBlockStates.Count} 個のブロックの状態（位置）を保存しました。");
    }

    /// <summary>
    /// ブロックの状態を保存または更新する (Listを使用して処理を実装)。
    /// </summary>
    /// <param name="blockId">ブロックのID</param>
    /// <param name="finalPos">ブロックの現在の位置</param>
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
            pastBlockStates.Add(new BlockState(blockId, finalPos));
        }
    }

    /// <summary>
    /// 指定されたIDのブロックの保存された状態を取得する。
    /// </summary>
    /// <param name="id">ブロックのID</param>
    /// <returns>保存された BlockState (見つからなかった場合は null)</returns>
    public BlockState? GetBlockState(string id)
    {
        // LinqのFirstOrDefaultを使って検索
        BlockState foundState = pastBlockStates.FirstOrDefault(state => state.id == id);

        // 過去のリストにそのIDが存在するかどうかをチェック
        if (pastBlockStates.Any(state => state.id == id))
        {
            return foundState;
        }
        return null; // データが見つからなかった
    }

    // ------------------------------------
    // リセット処理
    // ------------------------------------

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