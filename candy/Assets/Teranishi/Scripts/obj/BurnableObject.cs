using UnityEngine;
using System.Collections;

public class BurnableObject : MonoBehaviour
{
    [Tooltip("このオブジェクトを特定するためのユニークID")]
    public string objectID;

    [Header("炎の演出設定")]
    public Sprite fireSprite;      // あなたが描いた炎の画像
    public float burnDuration = 1.5f; // 消えるまでの時間

    private SpriteRenderer spriteRenderer;
    private SpriteMask spriteMask;
    private bool isBurning = false;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        // 子要素にSpriteMaskがあるか確認、なければ追加
        spriteMask = GetComponentInChildren<SpriteMask>();
    }

    void Start()
    {
        if (SceneDataTransfer.Instance != null && SceneDataTransfer.Instance.IsObjectBurned(objectID))
        {
            gameObject.SetActive(false);
        }
    }

    public void Burn()
    {
        if (isBurning) return;
        StartCoroutine(BurnSequence());
    }

    private IEnumerator BurnSequence()
    {
        isBurning = true;
        if (fireSprite != null) spriteRenderer.sprite = fireSprite;

        if (spriteMask != null)
        {
            spriteMask.enabled = true;

            // ▼▼▼ 修正ポイント ▼▼▼

            // スタート地点：中心（全部見えている状態）
            Vector3 startPos = Vector3.zero;

            // ゴール地点：【変更】上方向（プラス）へ移動させる
            // 2.0f くらいの値で、画像が完全に隠れる高さまで上げる
            Vector3 endPos = new Vector3(0, 2.5f, 0);

            // ▲▲▲ 修正ポイント ▲▲▲

            float elapsed = 0;
            while (elapsed < burnDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / burnDuration;
                spriteMask.transform.localPosition = Vector3.Lerp(startPos, endPos, t);
                yield return null;
            }
        }

        if (SceneDataTransfer.Instance != null)
            SceneDataTransfer.Instance.RecordBurnedObject(objectID);

        gameObject.SetActive(false);
    }
}