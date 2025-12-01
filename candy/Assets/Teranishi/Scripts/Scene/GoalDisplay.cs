using UnityEngine;
using UnityEngine.UI;

public class GoalDisplay : MonoBehaviour
{
    [Tooltip("移動回数を表示するTextコンポーネント")]
    public Text movesTextDisplay;
    // TextMeshPro を使う場合は public TMPro.TextMeshProUGUI movesTextDisplay; に変更してください。

    void Start()
    {
        if (movesTextDisplay == null)
        {
            Debug.LogError("movesTextDisplay に Text コンポーネントが設定されていません。");
            return;
        }

        if (SceneDataTransfer.Instance != null)
        {
            // t_goal.cs で一時保存した移動回数を取得
            int moves = SceneDataTransfer.Instance.movesOnClear;
            movesTextDisplay.text = $"移動回数: {moves} 回";
        }
        else
        {
            movesTextDisplay.text = "移動回数: データなし";
        }
    }
}