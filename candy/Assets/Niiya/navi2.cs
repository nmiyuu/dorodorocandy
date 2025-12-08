using UnityEngine;
using TMPro;

public class navi2 : MonoBehaviour
{
    public TMP_Text tmpText;      // 表示するテキスト
    public GameObject imageObject;
    public navi naviInstance;     // Inspectorでnaviオブジェクトを割り当てる

    string[] messages = {
        "？？",
        "なびくん",
        "なびくん",
        "なびくん"
    };

    int index = 0;
    bool canPressEnter = false;

    void Start()
    {
        tmpText.text = messages[index];

        if (naviInstance != null)
        {
            naviInstance.OnTypingFinished += OnNaviTypingFinished;
        }
        else
        {
            Debug.LogError("naviInstance が Inspector に割り当てられていません！");
        }
    }

    void OnNaviTypingFinished(int naviIndex)
    {
        // 同じ行がタイピング完了したら Enter を有効
        if (naviIndex == index)
        {
            canPressEnter = true;
        }
    }

    void Update()
    {
        if (!canPressEnter) return;

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            index++;
            canPressEnter = false; // 次の行は navi の完了待ち

            if (index < messages.Length)
            {
                tmpText.text = messages[index];
            }
            else
            {
                tmpText.text = "";
                if (imageObject != null) imageObject.SetActive(false);
                PlayerPrefs.SetInt("Navi2Shown", 1);
            }
        }
    }
}
