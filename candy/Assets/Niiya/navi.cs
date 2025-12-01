using UnityEngine;
using TMPro;
using System.Collections;

public class navi : MonoBehaviour
{
    public TMP_Text tmpText;
    public float typeSpeed = 0.05f; // 1文字の表示間隔（秒）

    // ここに最後に消したい画像オブジェクトを割り当てる
    public GameObject imageObject;

    string[] messages = {
        "初めまして\n僕はナビ！君のお助けをするよ",
        "ここは地盤沈下によって穴ができてるよ",
        "過去に行って穴を埋めよう！"
    };

    int index = 0;
    bool isTyping = false;

    void Start()
    {
        // ▼ すでに表示済みなら即非表示にして終了
        if (PlayerPrefs.GetInt("NaviShown", 0) == 1)
        {
            tmpText.text = "";
            if (imageObject != null) imageObject.SetActive(false);

            // このスクリプトを停止（Update も動かさない）
            this.enabled = false;
            return;
        }

        if (tmpText == null)
            Debug.LogError("tmpText がインスペクタで割り当てられていません");
        StartCoroutine(TypeText(messages[index]));
    }

    void Update()
    {
        // タイピング中はEnter受付しない
        if (isTyping) return;

        // Enter（Return）で次の文へ
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            index++;

            if (index < messages.Length)
            {
                StartCoroutine(TypeText(messages[index]));
            }
            else
            {
                tmpText.text = ""; // 全て終わったら消す
                                   // ★ 画像オブジェクトも非表示にする
                if (imageObject != null)
                    imageObject.SetActive(false);

                // ★ 一度表示したことを記録
                PlayerPrefs.SetInt("NaviShown", 1);
                PlayerPrefs.Save();
            }
        }
    }

    IEnumerator TypeText(string message)
    {
        isTyping = true;

        yield return null;

        // ▼ TMP の初期化遅れ対策（最重要）
        tmpText.ForceMeshUpdate();

        tmpText.text = "";

        foreach (char c in message)
        {
            tmpText.text += c;
            yield return new WaitForSeconds(typeSpeed);
        }

        isTyping = false;
    }
}