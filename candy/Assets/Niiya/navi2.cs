using UnityEngine;
using TMPro;

public class navi2 : MonoBehaviour
{
    public TMP_Text tmpText;

    // ★ ここに消したい画像の GameObject を入れる
    public GameObject imageObject;

    string[] messages = {
        "？？",
        "ナビ",
       "ナビ"
    };

    int index = 0;

    void Start()
    {
        tmpText.text = messages[index];  //最初の文を表示 

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
            }
        }
    }
}
