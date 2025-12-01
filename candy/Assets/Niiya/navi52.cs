using UnityEngine;
using TMPro;
using System.IO;

public class Navi52 : MonoBehaviour
{
    public TMP_Text tmpText;

    // 消したい画像の GameObject
    public GameObject imageObject;

    private string flagPath;

    string[] messages = {
        "なびくん",
        "なびくん",
        "なびくん"
    };

    int index = 0;

    void Start()
    {
        //File.Delete(Path.Combine(Application.persistentDataPath, "navi52_shown.txt"));

        if (tmpText == null)
        {
            Debug.LogError("tmpText が割り当てられていません");
            return;
        }

        flagPath = Path.Combine(Application.persistentDataPath, "navi52_shown.txt");

        // すでに表示済みなら終了
        if (File.Exists(flagPath))
        {
            tmpText.text = "";
            if (imageObject != null) imageObject.SetActive(false);
            this.enabled = false;
            return;
        }

        // 表示する場合
        if (imageObject != null) imageObject.SetActive(true);
        tmpText.text = messages[index];
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            index++;

            if (index < messages.Length)
            {
                tmpText.text = messages[index];
            }
            else
            {
                tmpText.text = "";
                if (imageObject != null) imageObject.SetActive(false);

                // 一度表示したことを記録
                File.WriteAllText(flagPath, "shown");
            }
        }
    }
}
