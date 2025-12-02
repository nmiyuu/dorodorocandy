using UnityEngine;
using TMPro;
using System.Collections;
using System.IO;

public class Navi51 : MonoBehaviour
{
    public TMP_Text tmpText;
    public float typeSpeed = 0.05f; // 1文字の表示間隔（秒）
    public GameObject imageObject;

    // 表示済みフラグを保存するファイルパス
    private string flagPath;

    private string[] messages = {
        "穴を埋める石が岩や木に囲まれて取れなくなってるね",
        "左上にマッチが落ちてるよ",
        "マッチを使って周りの木をどかそう！"
    };

    private int index = 0;
    private bool isTyping = false;

    void Start()
    {
        //File.Delete(Path.Combine(Application.persistentDataPath, "navi51_shown.txt"));

        if (tmpText == null)
        {
            Debug.LogError("tmpText がインスペクタで割り当てられていません");
            return;
        }

        flagPath = Path.Combine(Application.persistentDataPath, "navi51_shown.txt");

        // すでに表示済みなら即終了
        if (File.Exists(flagPath))
        {
            tmpText.text = "";
            if (imageObject != null) imageObject.SetActive(false);
            this.enabled = false;
            return;
        }

        // 表示する場合
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
                if (imageObject != null) imageObject.SetActive(false);

                // 表示済みフラグをファイルに保存
                File.WriteAllText(flagPath, "shown");
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
