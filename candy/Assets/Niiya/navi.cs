using UnityEngine;
using TMPro;
using System.Collections;

public class navi : MonoBehaviour
{
    public TMP_Text tmpText;      // 表示するテキスト
    public GameObject imageObject;
    public float typeSpeed = 0.05f;

    string[] messages = {
        "初めまして\n僕はナビ！君のお助けをするよ",
        "ここは地盤沈下で穴ができてるみたいだね！",
        "過去に戻ればどうにかできるかも！",
        "SPACEキーを押して\n時空移動装置を使ってみよう！"
    };

    int index = 0;
    bool isTyping = false;

    // ★ タイピング完了イベント
    public delegate void TypingFinishedHandler(int currentIndex);
    public event TypingFinishedHandler OnTypingFinished;

    void Start()
    {
        if (PlayerPrefs.GetInt("NaviShown", 0) == 1)
        {
            tmpText.text = "";
            if (imageObject != null) imageObject.SetActive(false);
            this.enabled = false;
            return;
        }

        StartCoroutine(TypeText(messages[index]));
    }

    void Update()
    {
        if (isTyping) return;

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            index++;
            if (index < messages.Length)
            {
                StartCoroutine(TypeText(messages[index]));
            }
            else
            {
                tmpText.text = "";
                if (imageObject != null) imageObject.SetActive(false);
                PlayerPrefs.SetInt("NaviShown", 1);
            }
        }
    }

    IEnumerator TypeText(string message)
    {
        isTyping = true;
        tmpText.text = "";
        foreach (char c in message)
        {
            tmpText.text += c;
            yield return new WaitForSeconds(typeSpeed);
        }
        isTyping = false;

        // ★ 完了通知
        OnTypingFinished?.Invoke(index);
    }
}
