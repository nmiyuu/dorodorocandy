using UnityEngine;
using TMPro;
using System.Collections;

public class NaviManager5 : MonoBehaviour
{
    [Header("Text Objects")]
    public TMP_Text naviText;    // タイピングあり
    public TMP_Text navi2Text;   // タイピングなし
    public GameObject naviImage; // 任意の画像

    [Header("Settings")]
    public float typeSpeed = 0.05f;

    [Header("Messages")]
    public string[] naviMessages = {
        "あれ？穴を埋める石が岩や木に囲まれて取れなくなってるね",
        "左上にマッチが落ちてるみたいだよ",
        "エンターを押すとマッチを使って木を燃やせるよ！"
    };

    public string[] navi2Messages = {
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
        if (PlayerPrefs.GetInt("NaviShown5", 0) == 1)
        {
            HideAll();
            if (SceneDataTransfer.Instance != null) SceneDataTransfer.Instance.isTalking = false;
            this.enabled = false;
            return;
        }

        if (SceneDataTransfer.Instance != null)
        {
            SceneDataTransfer.Instance.isTalking = true;
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
                PlayerPrefs.SetInt("NaviShown5", 1);


                //全会話終了：フラグを false にしてプレイヤーを動けるようにする
                if (SceneDataTransfer.Instance != null)
                {
                    SceneDataTransfer.Instance.isTalking = false;
                }
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
