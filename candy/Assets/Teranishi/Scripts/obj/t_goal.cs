using UnityEngine;
using UnityEngine.SceneManagement;

public class t_goal : MonoBehaviour
{
    [Header("ステージ設定")]
    [Tooltip("このステージのインデックス（例：ステージ1なら1、ステージ2なら2）")]
    // ステージインデックスを設定できるようにする
    public int thisStageIndex = 1;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // 中身はなし
    }

    // Update is called once per frame
    void Update()
    {
        // 中身はなし
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // プレイヤーコンポーネントがあるオブジェクトに衝突した場合
        if (collision.gameObject.GetComponent<t_player>())
        {
            // 1. SceneDataTransferが存在するか確認
            if (SceneDataTransfer.Instance != null)
            {
                // 2. このステージをクリアしたことを記録
                // lastClearedStageIndexが更新される
                SceneDataTransfer.Instance.RecordStageClear(thisStageIndex);

                Debug.Log($"ステージ {thisStageIndex} をクリアしました。クリア状況を記録。");

                // 3. 次のステージへ進む前に、プレイヤーの位置情報をリセットしておく
                // これにより、次のステージで前のステージの位置情報がロードされるのを防ぐ
                SceneDataTransfer.Instance.ClearPlayerState();
            }

            // 4. ゴールシーンへ遷移
            SceneManager.LoadScene("goal");
        }
    }
}