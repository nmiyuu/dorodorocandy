using UnityEngine;
using TMPro;

public class navi2 : MonoBehaviour
{
    public TMP_Text tmpText;
    public GameObject imageObject;

    string[] messages = {
        "？？",
        "なびくん",
        "なびくん",
        "なびくん"
    };

    int index = 0;

    void Start()
    {
        // すでに終了していれば非表示
        if (PlayerPrefs.GetInt("Navi2Shown", 0) == 1)
        {
            tmpText.text = "";
            if (imageObject != null) imageObject.SetActive(false);
            this.enabled = false;
            return;
        }

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
                tmpText.text = ""; // 全部終わったら消す 
                // ★ 画像オブジェクトも非表示にする
               if (imageObject != null) imageObject.SetActive(false); // ★ 一度表示したことを記録
               PlayerPrefs.SetInt("Navi2Shown", 1);
            }
        }
    }

   
}
