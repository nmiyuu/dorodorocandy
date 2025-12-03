using UnityEngine;
using TMPro;
using System.Collections;

public class Navi51 : MonoBehaviour
{
    public TMP_Text tmpText;
    public float typeSpeed = 0.05f;
    public GameObject imageObject;

    private string[] messages = {
        "穴を埋める石が岩や木に囲まれて取れなくなってるね",
        "左上にマッチが落ちてるよ",
        "マッチを使って周りの木をどかそう！"
    };

    private int index = 0;
    private bool isTyping = false;

    // ▼ ゲーム中のみ保持されるフラグ
    private static bool navi51ShownThisGame = false;

    void Start()
    {
        if (tmpText == null)
        {
            Debug.LogError("tmpText がインスペクタで割り当てられていません");
            return;
        }

        // すでにこのゲーム中に表示済みなら非表示にして終了
        if (navi51ShownThisGame)
        {
            tmpText.text = "";
            if (imageObject != null) imageObject.SetActive(false);
            this.enabled = false;
            return;
        }

        // 初回表示
        if (imageObject != null) imageObject.SetActive(true);
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

                // ▼ このゲーム中は再表示しない
                navi51ShownThisGame = true;
            }
        }
    }

    IEnumerator TypeText(string message)
    {
        isTyping = true;

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
