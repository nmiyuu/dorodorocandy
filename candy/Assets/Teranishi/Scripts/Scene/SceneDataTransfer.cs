using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;
using System; // [Serializable] に必要

// シーンをまたいでブロックの状態を保持するための構造体
[Serializable]
public struct BlockState
{
    public string id;            // 対応するブロックのユニークID
    public Vector3 finalPosition; // ブロックが移動し終わった最終位置

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

    // ------------------------------------
    // 転送データ
    // ------------------------------------

    [Header("プレイヤーの状態")]
    // プレイヤーの復帰位置
    [HideInInspector] public Vector3 playerPositionToLoad = Vector3.zero;
    // プレイヤーの向きのインデックス
    [HideInInspector] public int playerDirectionIndexToLoad = 0;

    [Header("ゲーム進行状況")]
    // 最後にクリアしたステージのインデックス（例：1, 2, 3, ...）。初期値は0。
    // GoalManagerで次のステージ名を動的に決定するために使用されます。
    [HideInInspector] public int lastClearedStageIndex = 0;

    [Header("ムーブブロックの状態")]
    // ブロックの状態を List<BlockState> で保持する
    [HideInInspector] public List<BlockState> pastBlockStates = new List<BlockState>();

    // --- Unityライフサイクル ---

    void Awake()
    {
        Application.targetFrameRate = 60;

        if (Instance != null && Instance != this)
        {
            // 既に存在するインスタンスがあれば、この新しいオブジェクトを破棄
            Destroy(gameObject);
        }
        else
        {
            // インスタンスを自身に設定し、シーン遷移時にも破棄されないようにする
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    // ------------------------------------
    // プレイヤーデータ処理
    // ------------------------------------

    /// <summary>
    /// シーン遷移前にプレイヤーの位置と向きを保存します。
    /// （Time Travel Controllerなどから呼ばれる）
    /// </summary>
    public void SavePlayerState(Vector3 pos, int dirIndex)
    {
        playerPositionToLoad = pos;
        playerDirectionIndexToLoad = dirIndex;
        Debug.Log($"[SceneDataTransfer] プレイヤーの状態を保存しました: Pos={pos}, Dir={dirIndex}");
    }

    /// <summary>
    /// 次のステージへ進む際などに、プレイヤーの位置ロード情報をリセットします。
    /// （次のステージで初期位置から開始させるため）
    /// </summary>
    public void ClearPlayerState()
    {
        // プレイヤーの位置と向きを初期値にリセット
        playerPositionToLoad = Vector3.zero;
        playerDirectionIndexToLoad = 0;

        // **注意**: ブロックの状態もクリアしないと、次のステージで前ステージのブロックが残る可能性がある
        pastBlockStates.Clear();

        Debug.Log("[SceneDataTransfer] プレイヤーの状態とブロックデータをリセットしました。");
    }

    // ------------------------------------
    // ステージクリアデータ処理
    // ------------------------------------

    /// <summary>
    /// クリアしたステージのインデックスを記録します。既に記録されているものより大きい場合のみ更新します。
    /// </summary>
    public void RecordStageClear(int clearedStageIndex)
    {
        if (clearedStageIndex > lastClearedStageIndex)
        {
            lastClearedStageIndex = clearedStageIndex;
            Debug.Log($"[SceneDataTransfer] ステージ {clearedStageIndex} のクリア状況を記録しました。");
        }
    }

    // ------------------------------------
    // ブロックデータ処理
    // ------------------------------------

    /// <summary>
    /// 過去シーンから現在シーンに戻る際に、ブロックの位置のリストを上書き保存する
    /// </summary>
    public void SaveBlockPositions(List<BlockState> statesToSave)
    {
        // 以前のデータを完全に上書き
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
    public BlockState? GetBlockState(string id)
    {
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

    /// <summary>
    /// lastClearedStageIndex も含め、全データを初期状態にリセットします。
    /// </summary>
    public void FullReset()
    {
        // プレイヤーの状態をリセット
        playerPositionToLoad = Vector3.zero;
        playerDirectionIndexToLoad = 0;

        // ブロックの状態リストをクリア
        pastBlockStates.Clear();

        // ステージクリア状況を初期値にリセット
        lastClearedStageIndex = 0;

        Debug.Log("[SceneDataTransfer] ゲーム状態を完全にリセットしました。");
    }
}