using UnityEngine;
using UnityEngine.UI; // UIコンポーネントを操作するために必要

/// <summary>
/// ブロックの移動可能回数の制限とそのUI表示（画像分解表示）を管理します。
/// MoveBlock.csと連携して動作します。
/// </summary>
public class PushLimit : MonoBehaviour
{
    [Header("プッシュ回数制限")]
    [Tooltip("このブロックが押せる回数の上限。-1を設定すると無制限になります。")]
    public int maxPushes = 1;

    // 現在残っている押せる回数。インスペクターで確認できるように表示されます。
    [SerializeField]
    private int remainingPushes;

    [Header("UI表示用設定")]
    [Tooltip("0から5に対応する回数インジケータースプライトの配列。インデックスは回数と対応します。")]
    public Sprite[] countIndicatorSprites; // 例: 0, 1, 2, 3, 4, 5の6枚を想定

    [Tooltip("基準値（BASE_COUNT、例：5）の倍数部分を表示するための Image コンポーネント。")]
    public Image baseIndicatorImage;

    [Tooltip("基準値未満の、残りの値（余り）を表示するための Image コンポーネント。")]
    public Image remainderIndicatorImage;

    // バッテリーの満タン（一つの画像で表せる上限）を表す基準値。
    private const int BASE_COUNT = 5;

    void Awake()
    {
        // ゲーム開始時に、残りの回数を最大設定値で初期化します。
        remainingPushes = maxPushes;
    }

    void Start()
    {
        // ゲーム開始時にUIを初期状態に更新します。
        UpdateDisplay();
    }

    /// <summary>
    /// ブロックをプッシュする権限があるかチェックします。
    /// </summary>
    /// <returns>回数が残っている場合、または無制限設定の場合は true。</returns>
    public bool CanPush()
    {
        // 無制限（-1）の場合は常にプッシュを許可します。
        if (maxPushes < 0) return true;

        // 残り回数が1以上の場合にプッシュを許可します。
        return remainingPushes > 0;
    }

    /// <summary>
    /// プッシュ回数を1回消費し、それに伴いUI表示を更新します。
    /// </summary>
    public void ConsumePush()
    {
        if (maxPushes >= 0)
        {
            remainingPushes--;
            // 回数消費後に、表示を更新する関数を呼び出します。
            UpdateDisplay();
        }
    }

    /// <summary>
    /// 残り回数を基準値（BASE_COUNT=5）と余りに分解し、対応する画像を表示します。
    /// 例: 7回 → Base(5の画像) と Remainder(2の画像) を表示。
    /// </summary>
    public void UpdateDisplay()
    {
        // 必要なスプライトが揃っているか、Imageコンポーネントがアタッチされているかを確認します。
        if (countIndicatorSprites == null || countIndicatorSprites.Length < BASE_COUNT + 1)
        {
            Debug.LogError($"[PushLimit] スプライトが不足しています（0〜{BASE_COUNT}の{BASE_COUNT + 1}枚が必要です）。");
            return;
        }

        if (baseIndicatorImage == null || remainderIndicatorImage == null)
        {
            Debug.LogWarning("[PushLimit] インジケーター Image が設定されていません。");
            return;
        }

        // 実際にUIで表示する回数（負の値にならないように制御）
        int currentCount = Mathf.Max(0, remainingPushes);

        // --- 回数分解ロジック ---

        // 基準値（5）の画像がいくつ分あるか (例: 7回なら 1)
        int baseDisplays = currentCount / BASE_COUNT;
        // 基準値未満の、残りの回数 (例: 7回なら 2)
        int remainder = currentCount % BASE_COUNT;

        // 1. 基準値表示の更新 (baseIndicatorImage)
        if (baseDisplays > 0)
        {
            // 基準値の倍数がある場合、満タンの画像（インデックス5）を表示します。
            baseIndicatorImage.sprite = countIndicatorSprites[BASE_COUNT];
            baseIndicatorImage.enabled = true;
        }
        else
        {
            // 基準値未満の場合（4, 3など）は、この基準値の画像を非表示にします。
            baseIndicatorImage.enabled = false;
        }

        // 2. 残り値表示の更新 (remainderIndicatorImage)
        // 余り（0〜4）に対応するスプライトを設定します。
        remainderIndicatorImage.sprite = countIndicatorSprites[remainder];
        remainderIndicatorImage.enabled = true;

        // 3. UIのレイアウト調整 (配置の調整)

        // もし基準値の画像が表示されていない場合（baseDisplays == 0）、
        // 残り値の画像を基準値の画像の位置に寄せることで、中央寄せのように見せます。
        if (baseDisplays == 0)
        {
            // RemainderIndicatorImageをBaseIndicatorImageの位置に移動させます。
            remainderIndicatorImage.rectTransform.anchoredPosition = baseIndicatorImage.rectTransform.anchoredPosition;
        }
        else // 基準値の画像がある場合
        {
            // RemainderIndicatorImageをBaseIndicatorImageの隣の正しい位置に戻します。
            // ※正しい位置への調整は、Unityエディタでの配置（Rect Transform）が必要です。
        }

        // 補足: 割り切れる場合（10回など）に5と0を表示したくない場合は、ここで remainder == 0 の処理を追加します。
    }
}