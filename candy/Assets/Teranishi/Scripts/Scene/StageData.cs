using UnityEngine;

public static class SaveManager
{
    // 最後にクリアしたステージのインデックス（またはID）を保存するためのキー
    private const string CLEARED_STAGE_KEY = "LastClearedStageIndex";

    /// <summary>
    /// ステージクリア情報を保存します。
    /// </summary>
    /// <param name="stageIndex">クリアしたステージのインデックス（番号）</param>
    public static void SaveProgress(int stageIndex)
    {
        // 現在保存されているインデックスよりも新しい（大きい）場合にのみ更新します。
        // これにより、低いレベルを再クリアしても進行状況が戻りません。
        int currentMaxCleared = LoadMaxClearedStageIndex();

        if (stageIndex > currentMaxCleared)
        {
            PlayerPrefs.SetInt(CLEARED_STAGE_KEY, stageIndex);
            PlayerPrefs.Save(); // データをディスクに書き込みます
            Debug.Log($"進行状況を保存しました。クリア済み最大ステージ: {stageIndex}");
        }
    }

    /// <summary>
    /// 現在のセーブデータで到達した最大クリアステージインデックスを取得します。
    /// </summary>
    /// <returns>クリア済み最大ステージのインデックス。セーブデータがなければ0を返します。</returns>
    public static int LoadMaxClearedStageIndex()
    {
        // 第2引数は、キーが存在しない場合に返すデフォルト値です。
        return PlayerPrefs.GetInt(CLEARED_STAGE_KEY, 0);
    }
}