using UnityEngine;
using TMPro;
using System.Collections;

public class NaviManager3 : MonoBehaviour
{
    [Header("Text Objects")]
    public TMP_Text naviText;    // タイピングあり
    public TMP_Text navi2Text;   // タイピングなし
    public GameObject naviImage; // 任意の画像

    [Header("Settings")]
    public float typeSpeed = 0.05f;

    [Header("Messages")]
    public string[] naviMessages = {
        "わぁ、ゴールの前に大きな岩があるよ！",
        "うーん、ロボット君の力じゃ動かせそうにないね",
        "ん？でも、見たことないものがあるみたい！",
        "岩をそこまで動かしてみよう！"
    };

    public string[] navi2Messages = {
        "なびくん",
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
        if (PlayerPrefs.GetInt("NaviShown3", 0) == 1)
        {
            HideAll();
            if (SceneDataTransfer.Instance != null) SceneDataTransfer.Instance.isTalking = false;
            this.enabled = false;
            return;
        }

        //会話開始：フラグを true にしてプレイヤーの動きを止める
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
                PlayerPrefs.SetInt("NaviShown3", 1);


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
