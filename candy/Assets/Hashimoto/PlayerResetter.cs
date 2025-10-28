using UnityEngine;

public class PlayerResetter : MonoBehaviour
{
    void Update()
    {
        // Rキーでプレイヤーをリセット（古いプレイヤーを削除）
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetPlayer();
        }
    }

    void ResetPlayer()
    {
        // タグ「Player」が付いたオブジェクトを探す
        GameObject oldPlayer = GameObject.FindGameObjectWithTag("Player");

        if (oldPlayer != null)
        {
            Destroy(oldPlayer);
            Debug.Log("古いプレイヤーを削除しました");
        }
        else
        {
            Debug.Log("削除対象のプレイヤーが見つかりませんでした");
        }
    }
}
