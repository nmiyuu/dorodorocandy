using UnityEngine;
using TMPro;
using System.Collections;

public class navi31 : MonoBehaviour
{
    public TMP_Text tmpText;
    public float typeSpeed = 0.05f;

    public GameObject imageObject;

    string[] messages = {
        "ゴールの前に大きな岩があるよ",
        "でもロポット君の力じゃ動かせないね",
        "あれ？でも見たことないものがここにはあるね。",
        "まず調べてみよう！"
    };

    int index = 0;
    bool isTyping = false;

    // ▼ ゲーム中だけ保持される共通フラグ（永続保存されない）
    private static bool naviShownThisGame = false;

    void Start()
    {
        // ▼ このシーン中ですでに一度表示していたら非表示にする
        if (naviShownThisGame)
        {
            tmpText.text = "";
            if (imageObject != null) imageObject.SetActive(false);

            this.enabled = false;
            return;
        }

        if (tmpText == null)
            Debug.LogError("tmpText がインスペクタで割り当てられていません");

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
                if (imageObject != null)
                    imageObject.SetActive(false);

                // ▼ このゲーム中は2回目以降は表示しない
                naviShownThisGame = true;
            }
        }
    }

    IEnumerator TypeText(string message)
    {
        isTyping = true;

        yield return null;

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
