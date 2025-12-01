using UnityEngine;
using UnityEngine.SceneManagement;

public class t_goal : MonoBehaviour
{
    [Header("ステージ設定")]
    [Tooltip("このステージのインデックス（例：ステージ1なら1、ステージ2なら2）")]
    public int thisStageIndex = 1;

    void Start()
    {
        // 中身はなし
    }

    void Update()
    {
        // 中身はなし
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.GetComponent<t_player>())
        {
            if (SceneDataTransfer.Instance != null)
            {
                // 1. 【追加】クリア時の移動回数を一時保存 (ゴール画面へ渡す)
                SceneDataTransfer.Instance.movesOnClear = SceneDataTransfer.Instance.currentStageMoveCount;

                // 2. ステージクリアを記録
                SceneDataTransfer.Instance.RecordStageClear(thisStageIndex);

                // 3. 【修正】次のステージへ行く準備として、プレイヤー位置、ブロック、そして移動回数（currentStageMoveCount）をリセット
                SceneDataTransfer.Instance.ClearPlayerState();
            }

            // 4. ゴールシーンへ遷移
            SceneManager.LoadScene("goal");
        }
    }
}