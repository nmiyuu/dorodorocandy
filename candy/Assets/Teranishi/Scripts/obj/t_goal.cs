using UnityEngine;
using UnityEngine.SceneManagement;

public class t_goal : MonoBehaviour
{
    // ★追加: 最終ステージのインデックスを定義
    [Header("ステージ設定")]
    [Tooltip("このステージのインデックス（例：ステージ1なら1、ステージ2なら2）")]
    public int thisStageIndex = 1;

    [Tooltip("全ステージの数 (この値以上でゲームクリア)")]
    public int finalStageIndex = 5;

    [Tooltip("ステージクリア時の画面名（リザルト画面など）")]
    public string stageGoalSceneName = "goal";

    [Tooltip("全ゲームクリア時の画面名")]
    public string gameClearSceneName = "clear";

    // Start()とUpdate()は変更なし

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 1. プレイヤーかどうかの判定
        if (collision.gameObject.GetComponent<t_player>())
        {
            if (SceneDataTransfer.Instance == null || SceneFader.Instance == null)
            {
                Debug.LogError("SceneDataTransfer または SceneFader が見つかりません。ゴール処理を中止します。");
                return;
            }

            // 2. シーン遷移中（既にフェードアウト開始済みなど）は処理しない
            if (SceneDataTransfer.Instance.isChangingScene)
            {
                return;
            }

            // --- データ転送処理 ---

            // クリア時の移動回数を一時保存 (ゴール画面へ渡す)
            SceneDataTransfer.Instance.movesOnClear = SceneDataTransfer.Instance.currentStageMoveCount;

            // ステージクリアを記録
            SceneDataTransfer.Instance.RecordStageClear(thisStageIndex);

            // 次のステージへ行く準備として、プレイヤー位置、ブロック、そして移動回数（currentStageMoveCount）をリセット
            SceneDataTransfer.Instance.ClearPlayerState();

            // --- シーン遷移の判定と実行 ---

            if (thisStageIndex >= finalStageIndex)
            {
                // 最終ステージの場合: ゲームクリアシーンへ
                Debug.Log($"🎉 ステージ{thisStageIndex}をクリア！ゲームクリアシーン({gameClearSceneName})へ遷移します。");
                SceneFader.Instance.LoadSceneWithFade(gameClearSceneName, FadeColor.Black);
            }
            else
            {
                // 最終ステージでない場合: ステージゴール画面へ
                Debug.Log($"ステージ{thisStageIndex}をクリア。ゴール画面({stageGoalSceneName})へ遷移します。");
                SceneFader.Instance.LoadSceneWithFade(stageGoalSceneName, FadeColor.Black);
            }
        }
    }
}