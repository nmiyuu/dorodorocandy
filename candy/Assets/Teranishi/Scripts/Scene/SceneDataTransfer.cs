using UnityEngine;
using System.Collections.Generic;
using System.Linq; // FindObjectsByTypeやLinqを使う時に必要

// ブロックの状態を保存するための構造体
[System.Serializable]
public struct BlockState
{
    public string id;
    public Vector3 finalPosition;
}

/// シーンをまたいでデータを持ち越すシングルトンクラス。
/// DontDestroyOnLoadでシーン切り替え後も消えないようにする。
public class SceneDataTransfer : MonoBehaviour
{
    // --- シングルトンインスタンス ---
    public static SceneDataTransfer Instance;

    // --- シーンをまたいで引き継ぎたいデータ ---

    // プレイヤーの復帰位置
    public Vector3 playerPositionToLoad = Vector3.zero;

    // プレイヤーの向きを保存する変数 (Int型インデックス: 下=1, 上=2, 右=3, 左=4)
    public int playerDirectionIndexToLoad = 1; // 初期値は1（下）

    // 過去シーンで動かされたブロックの位置を保存するリスト
    public List<BlockState> pastBlockStates = new List<BlockState>();

    void Awake()
    {
        // シングルトンパターンの確立
        if (Instance == null)
        {
            Instance = this;
            // ずっと残す
            DontDestroyOnLoad(gameObject);
        }
        else
        {
           
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Rキーリセットなどで呼ばれる。保存されているすべてのデータを初期状態に戻す。
    /// これで過去/未来の保存データもクリアになる。
    /// </summary>
    public void FullReset()
    {
        // プレイヤーの位置と向きを初期値に戻す
        playerPositionToLoad = Vector3.zero;
        playerDirectionIndexToLoad = 1; // 向きの初期値（下）

        // ブロックの状態リストをクリアする
        pastBlockStates.Clear();

        Debug.Log("[SceneDataTransfer] データと過去/未来の状態を完全リセットする");
    }

    // 動くブロックの最終位置を保存する関数
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
        Debug.Log("動くブロックの位置を " + pastBlockStates.Count + " 個保存する");
    }
}