using UnityEngine;
using System.Collections;

/// 指定のアイテムにより燃焼・消失するオブジェクトの制御クラス
/// 燃焼アニメーション、SpriteMaskによる隠蔽演出、および状態保存の実装
public class BurnableObject : MonoBehaviour
{
    // ======================================================================================
    // 設定項目
    // ======================================================================================

    [Tooltip("オブジェクト識別用のユニークID")]
    public string objectID;

    [Header("炎の演出設定")]
    [SerializeField] private Sprite fireSprite;      // 燃焼中への切り替え用スプライト
    [SerializeField] private float burnDuration = 1.5f; // 燃焼開始から消失までの秒数

    // ======================================================================================
    // 内部変数
    // ======================================================================================

    private SpriteRenderer spriteRenderer;
    private SpriteMask spriteMask;
    private bool isBurning = false; // 二重燃焼防止用フラグ

    // ======================================================================================
    // ライフサイクル
    // ======================================================================================

    private void Awake()
    {
        /// 描画コンポーネントおよび子要素のマスク機能の参照取得
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteMask = GetComponentInChildren<SpriteMask>();
    }

    private void Start()
    {
        /// シーン開始時の状態復元
        /// 記録済み（焼失済み）IDであれば、即座にオブジェクトを非アクティブ化
        if (SceneDataTransfer.Instance != null && SceneDataTransfer.Instance.IsObjectBurned(objectID))
        {
            gameObject.SetActive(false);
        }
    }

    // ======================================================================================
    // 燃焼制御
    // ======================================================================================

    /// 燃焼開始の外部命令
    public void Burn()
    {
        /// 重複実行を防止し、コルーチンによる時系列処理を開始
        if (isBurning) return;
        StartCoroutine(BurnSequence());
    }

    /// 燃焼演出、隠蔽アニメーション、データの永続化を伴う一連の処理
    private IEnumerator BurnSequence()
    {
        isBurning = true;

        /// スプライトを炎画像へ差し替え
        if (fireSprite != null) spriteRenderer.sprite = fireSprite;

        /// SpriteMaskによるピクセル隠蔽（フェードアウト風）演出
        if (spriteMask != null)
        {
            spriteMask.enabled = true;

            /// マスクの移動範囲設定
            /// startPos: 中心位置（全表示）
            /// endPos: 上方位置（画像を完全に覆い隠す高さ）
            Vector3 startPos = Vector3.zero;
            Vector3 endPos = new Vector3(0, 2.5f, 0);

            float elapsed = 0;
            while (elapsed < burnDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / burnDuration;

                /// 線形補間(Lerp)を用いた、時間経過に伴うマスク位置の移動実行
                spriteMask.transform.localPosition = Vector3.Lerp(startPos, endPos, t);
                yield return null;
            }
        }

        /// 進行データの永続化
        /// シーン遷移後も「消失した」状態を維持するためのフラグ記録
        if (SceneDataTransfer.Instance != null)
        {
            SceneDataTransfer.Instance.RecordBurnedObject(objectID);
        }

        /// 演出完了後の完全消去（非アクティブ化）
        gameObject.SetActive(false);
    }
}