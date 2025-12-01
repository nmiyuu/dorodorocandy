using UnityEngine;
using TMPro;

public class navi52 : MonoBehaviour
{
    public TMP_Text tmpText;

    // ★ ここに消したい画像の GameObject を入れる
    public GameObject imageObject;

    string[] messages = {
        "なびくん",
        "なびくん",
        "なびくん"
    };

    int index = 0;

    void Start()
    {
        // ▼ すでに表示済みなら即非表示にして終了
        if (PlayerPrefs.GetInt("Navi2Shown", 0) == 1)
        {
            tmpText.text = "";
            if (imageObject != null) imageObject.SetActive(false);
            this.enabled = false;   // Update を止める
            return;
        }

        tmpText.text = messages[index];  // 最初の文を表示
    }

    void Update()
    {
        // Enter または Return が押されたら
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            index++;

            if (index < messages.Length)
            {
                tmpText.text = messages[index];  // 次の文を表示
            }
            else
            {
                tmpText.text = "";  // 全部終わったら消す
                                    // ★ 画像オブジェクトも非表示にする
                if (imageObject != null)
                    imageObject.SetActive(false);

                // ★ 一度表示したことを記録
                PlayerPrefs.SetInt("Navi2Shown", 1);
            }
        }
    }
}

