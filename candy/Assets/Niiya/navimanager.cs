using UnityEngine;
using TMPro;
using System.Collections;

public class NaviManager : MonoBehaviour
{
    [Header("Text Objects")]
    public TMP_Text naviText;    // タイピングあり
    public TMP_Text navi2Text;   // タイピングなし
    public GameObject naviImage; // 任意の画像


    [Header("Settings")]
    public float typeSpeed = 0.05f;

    [Header("Messages")]
    public string[] naviMessages = {
        "初めまして\n僕はナビ！君のお助けをするよ",
        "ここは地盤沈下で穴ができてるみたいだね！",
        "過去に戻ればどうにかできるかも！",
        "SPACEキーを押して\n時空移動装置を使ってみよう！"
    };

    public string[] navi2Messages = {
        "？？",
        "なびくん",
        "なびくん",
        "なびくん"
    };

    int index = 0;
    bool isTyping = false;
    bool canPressEnter = false;

    void Start()
    {
        // すでに表示済みなら非表示
        if (PlayerPrefs.GetInt("NaviShown", 0) == 1)
        {
            HideAll();
            this.enabled = false;
            return;
        }
       
        StartCoroutine(TypeNaviText(naviMessages[index]));
        navi2Text.text = navi2Messages[index];
    }

    void Update()
    {
        if (!canPressEnter) return;

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            index++;
            canPressEnter = false;

            if (index < naviMessages.Length)
            {
                StartCoroutine(TypeNaviText(naviMessages[index]));
                navi2Text.text = navi2Messages[index];
            }
            else
            {
                HideAll();
                PlayerPrefs.SetInt("NaviShown", 1);
            }
        }
    }

    IEnumerator TypeNaviText(string message)
    {
        isTyping = true;
        naviText.text = "";
        foreach (char c in message)
        {
            naviText.text += c;
            yield return new WaitForSeconds(typeSpeed);
        }
        isTyping = false;

        canPressEnter = true; // タイピング完了で Enter 有効
    }

    void HideAll()
    {
        naviText.text = "";
        navi2Text.text = "";
        if (naviImage != null) naviImage.SetActive(false);
    }
}
