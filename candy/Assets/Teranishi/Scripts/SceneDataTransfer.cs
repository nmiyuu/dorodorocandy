using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Linq;

// ブロックの状態を保存するための構造体 (変更なし)
[System.Serializable]
public struct BlockState
{
    public string id;
    public Vector3 finalPosition;
}

public class SceneDataTransfer : MonoBehaviour
{
    // --- シングルトンインスタンス ---
    public static SceneDataTransfer Instance;

    // --- シーンをまたいで引き継ぎたいデータ ---
    // プレイヤーの復帰位置 (既存)
    public Vector3 playerPositionToLoad = Vector3.zero;

    // ★追加: プレイヤーの向きを保存する変数
    public Vector2 playerDirectionToLoad = Vector2.down; // 初期値は正面

    // 過去シーンで動かされたブロックの位置を保存するリスト (既存)
    public List<BlockState> pastBlockStates = new List<BlockState>();

    void Awake()
    {
        // シングルトンパターンの確立 (既存)
        if (Instance == null)
        {
            Instance = this;
            // このオブジェクトをシーン切り替え時に破棄されないようにする（永続化）
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // 既にインスタンスがあれば、新しい方は破棄する
            Destroy(gameObject);
        }
    }

    // 動くブロックの最終位置を保存する関数 (既存)
    public void SaveBlockPositions()
    {
        pastBlockStates.Clear();

        // 現在のシーンにある MoveBlock をすべて検索
        MoveBlock[] currentBlocks = FindObjectsByType<MoveBlock>(FindObjectsSortMode.None);

        foreach (MoveBlock block in currentBlocks)
        {
            // blockIDが設定されているブロックのみを処理
            if (!string.IsNullOrEmpty(block.blockID))
            {
                BlockState state = new BlockState
                {
                    id = block.blockID,
                    // 位置をマス目の中心に丸める
                    finalPosition = new Vector3(
                        Mathf.Round(block.transform.position.x),
                        Mathf.Round(block.transform.position.y),
                        Mathf.Round(block.transform.position.z)
                    )
                };
                pastBlockStates.Add(state);
            }
        }
        Debug.Log("動くブロックの位置を " + pastBlockStates.Count + " 個保存しました。");
    }
}