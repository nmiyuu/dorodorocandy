using UnityEngine;

using UnityEngine.UI;

using System.Collections; // コルーチンに必要



public class GoalDisplay : MonoBehaviour

{

    [Tooltip("移動回数を表示するTextコンポーネント")]

    public Text movesTextDisplay;



    [Header("演出設定")]

    [SerializeField] private GameObject clearUIRoot; // クリア画面の親（Canvas等）



    private void Awake()

    {

        // シーン開始時は、まずクリアUI全体を隠しておく

        if (clearUIRoot != null) clearUIRoot.SetActive(false);

    }



    /// void Start から IEnumerator Start に変更

    private IEnumerator Start()

    {
       
        if (movesTextDisplay == null)

        {

            Debug.LogError("movesTextDisplay に Text コンポーネントが設定されていません。");

            yield break;

        }



        // 1. SceneFader の明転（画面が白/黒から透明になるまで）を待つ

        if (SceneFader.Instance != null)

        {

            // IsFadingがtrue（演出中）の間、ここで待機

            while (SceneFader.Instance.IsFading)

            {

                yield return null;

            }

        }



        // 2. 画面が明るくなってから、データをセットしてUIを表示

        UpdateDisplay();

    }



    private void UpdateDisplay()

    {

        if (SceneDataTransfer.Instance != null)

        {

            int moves = SceneDataTransfer.Instance.movesOnClear;

            movesTextDisplay.text = $"移動回数: {moves} 回";

        }

        else

        {

            movesTextDisplay.text = "移動回数: データなし";

        }

    }

}