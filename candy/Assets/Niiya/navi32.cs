using UnityEngine;
using TMPro;

public class navi32 : MonoBehaviour
{
    public TMP_Text tmpText;

    // 消したい画像
    public GameObject imageObject;

    string[] messages = {
        "なびくん",
        "なびくん",
        "なびくん",
        "なびくん"
    };

    int index = 0;

    // ▼ このゲーム中だけ保持（永続保存されない）
    private static bool navi32ShownThisGame = false;

    void Start()
    {
        // ▼ すでにこのゲーム中に表示済みなら終了
        if (navi32ShownThisGame)
        {
            tmpText.text = "";
            if (imageObject != null) imageObject.SetActive(false);
            this.enabled = false;
            return;
        }

        // 最初のメッセージを表示
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
                // 全部終わったら消す
                tmpText.text = "";

                if (imageObject != null)
                    imageObject.SetActive(false);

                // ▼ このゲーム中は2回目以降表示しない
                navi32ShownThisGame = true;
            }
        }
    }
}
