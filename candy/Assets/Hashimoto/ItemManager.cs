using UnityEngine;
using System.Collections.Generic;

public class ItemManager : MonoBehaviour
{
    public static ItemManager Instance;

    // --- 取得したアイテムのID一覧 ---
    // アイテムIDを保存し、シーン移動後も復活しないようにする
    public HashSet<string> obtainedItems = new HashSet<string>();

    // --- 破壊されたブロックのID一覧 ---
    // ブロックIDを保存し、シーン移動後も復活しないようにする
    public HashSet<string> destroyedBlocks = new HashSet<string>();

    void Awake()
    {
        // シングルトン（ゲーム全体で１つだけ存在させる）
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // シーンをまたいでも破棄しない
        }
        else
        {
            Destroy(gameObject); // すでに存在していたら破棄
        }
    }
}