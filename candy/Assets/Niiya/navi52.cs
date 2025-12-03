using UnityEngine;
using TMPro;

public class Navi52 : MonoBehaviour
{
    public TMP_Text tmpText;

    // 消したい画像の GameObject
    public GameObject imageObject;

    string[] messages = {
        "なびくん",
        "なびくん",
        "なびくん"
    };

    int index = 0;

    // ▼ ゲーム中のみ保持されるフラグ（永続データではない）
    private static bool navi52ShownThisGame = false;

    void Start()
    {
        if (tmpText == null)
        {
            Debug.LogError("tmpText が割り当てられていません");
            return;
        }

        // すでにこのゲーム中に表示済みなら非表示で終了
        if (navi52ShownThisGame)
        {
            tmpText.text = "";
            if (imageObject != null) imageObject.SetActive(false);
            this.enabled = false;
            return;
        }

        // 初回表示
        if (imageObject != null) imageObject.SetActive(true);
        tmpText.text = messages[index];
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            index++;

            if (index < messages.Length)
            {
                tmpText.text = messages[index];
            }
            else
            {
                // 全てのメッセージを表示し終わったら非表示
                tmpText.text = "";
                if (imageObject != null) imageObject.SetActive(false);

                // ▼ このゲーム中は再表示しない
                navi52ShownThisGame = true;
            }
        }
    }
}
