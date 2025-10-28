using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class PushTrigger : MonoBehaviour
{
    private CostDisplay costDisplay;
    private bool hasPushed = false; // 一度だけ減らすためのフラグ

    void Start()
    {
        // タグで取得する（CostImageのGameObjectにCostDisplayをつける）
        //costDisplay = GameObject.FindWithTag("Costmage").GetComponent<CostDisplay>();
        // 名前で直接探す
        StartCoroutine(FindCostDisplayCoroutine());
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(FindCostDisplayCoroutine());
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // 岩（MoveBlock）にぶつかった瞬間だけ減らす
        if (collision.gameObject.CompareTag("MoveBlock") && !hasPushed)
        {
            if (costDisplay != null)
            {
                costDisplay.DecreaseCost();
                hasPushed = true;
            }
        }
    }

    // 離れたら再び押せるようにする
    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("MoveBlock"))
        {
            hasPushed = false;
        }
    }

    IEnumerator FindCostDisplayCoroutine()
    {
        while (costDisplay == null)
        {
            GameObject obj = GameObject.Find("CostImage");
            if (obj != null)
            {
                costDisplay = obj.GetComponent<CostDisplay>();
                Debug.Log("✅ CostDisplay found by name: " + obj.name);
                yield break;
            }
            else
            {
                Debug.Log("🔍 CostImage 探してるけどまだ見つからない...");
            }
            yield return null; // 次のフレームまで待つ
        }
    }

}
