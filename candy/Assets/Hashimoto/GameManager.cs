using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    // アイテム取得状態（キー: アイテムID）
    public HashSet<string> obtainedItems = new HashSet<string>();

    // ブロック破壊状態（キー: ブロックID）
    public HashSet<string> destroyedBlocks = new HashSet<string>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}