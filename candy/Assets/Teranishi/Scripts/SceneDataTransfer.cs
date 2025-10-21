using UnityEngine;
using System.Collections.Generic; // Listを使うために必要
using UnityEngine.SceneManagement; // DontDestroyOnLoadを使うためにSceneManagementをインポート（厳密には不要ですが念のため）

// ブロックの状態を保存するための構造体を外部に定義（もしくはSceneDataTransferクラスの内部でも可）
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
    public Vector3 playerPositionToLoad = Vector3.zero;
    public List<BlockState> pastBlockStates = new List<BlockState>();

    void Awake()
    {
        // シングルトンパターンの確立
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

    // 過去のブロックの最終位置を保存する関数
    public void SaveBlockPositions()
    {
        pastBlockStates.Clear();

        // ★★★ 修正部分：FindObjectsOfType を FindObjectsByType に変更 ★★★
        // FindObjectsSortMode.None を使うことで、より高速にオブジェクトを取得できます。
        MoveBlock[] pastBlocks = FindObjectsByType<MoveBlock>(FindObjectsSortMode.None);
        // ★★★ 修正部分 ここまで ★★★

        foreach (MoveBlock block in pastBlocks)
        {
            // blockIDが設定されているか確認
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
        Debug.Log("過去のブロック位置を " + pastBlockStates.Count + " 個保存しました。");
    }
}
