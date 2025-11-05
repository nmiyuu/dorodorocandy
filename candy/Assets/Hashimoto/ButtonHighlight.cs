using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonHighlight : MonoBehaviour
{
    public Outline outline; // 対応する Outline を Inspector でセット

    void Update()
    {
        // 現在選択されているボタンと自分を比較
        if (EventSystem.current.currentSelectedGameObject == gameObject)
            outline.enabled = true;  // 選択中 → 有効
        else
            outline.enabled = false; // 選択されていない → 無効
    }
}