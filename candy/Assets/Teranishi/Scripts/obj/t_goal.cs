using UnityEngine;
using UnityEngine.SceneManagement;

public class t_goal : MonoBehaviour
{
    [Header("ステージ設定")]
    [Tooltip("このステージのインデックス（例：ステージ1なら1、ステージ2なら2）")]
    public int thisStageIndex = 1; // ステージ5では、この値を 5 に設定する

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.GetComponent<t_player>())
        {
            if (SceneDataTransfer.Instance != null && SceneFader.Instance != null)
            {
                if (SceneDataTransfer.Instance.isChangingScene) return;

                // 1. クリア時の移動回数を一時保存
                SceneDataTransfer.Instance.movesOnClear = SceneDataTransfer.Instance.currentStageMoveCount;

                // 2. ステージクリアを記録
                SceneDataTransfer.Instance.RecordStageClear(thisStageIndex);

                // ★★★ デバッグコード: 記録後の値をコンソールに出力して確認 ★★★
                Debug.Log($"[t_goal DEBUG] ステージ{thisStageIndex}をクリアしました。");
                if (SceneDataTransfer.Instance.lastClearedStageIndex >= thisStageIndex)
                {
                    Debug.Log($"[t_goal DEBUG] ✅ 記録成功: lastClearedStageIndex は {SceneDataTransfer.Instance.lastClearedStageIndex} に更新されました。");
                }
                else
                {
                    Debug.LogError($"[t_goal DEBUG] ❌ 記録失敗: lastClearedStageIndex は {SceneDataTransfer.Instance.lastClearedStageIndex} のままです。");
                }
                // ★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★

                // 3. 次のステージへ行く準備として、プレイヤー状態をリセット
                SceneDataTransfer.Instance.ClearPlayerState();

                // 4. ゴールシーンへ遷移
                SceneFader.Instance.LoadSceneWithFade("goal", FadeColor.Black);
            }
        }
    }
}