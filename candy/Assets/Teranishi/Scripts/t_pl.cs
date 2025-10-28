using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;
using System.Collections.Generic;

/// <summary>
/// プレイヤーのアニメーションと向きを管理するスクリプトだ。
/// 特徴: 複数キー同時押しの場合、「最後に押されたキー」の向きをアニメーションに反映させる。
/// </summary>
public class t_pl : MonoBehaviour
{
    // --- 定数とコンポーネント ---
    private Animator _animator;
    private const string DirectionParam = "Direction"; // AnimatorのIntパラメーター名

    // --- 内部状態 ---
    // 向きのインデックス (1:下, 2:上, 3:右, 4:左)
    private int lastDirectionIndex = 1;

    // 最後に押されたキーとタイムスタンプを格納する辞書 (InputSystem依存)
    private Dictionary<int, float> lastKeyPressTime = new Dictionary<int, float>();

    // --- 公開プロパティ (外部連携用) ---
    // t_player.csがこのプロパティを参照することで、移動方向とアニメーション向きを同期させることが可能だ。
    public int CurrentDirectionIndex => lastDirectionIndex;

    // --- Unityライフサイクル ---

    /// <summary>
    /// 初期化処理。コンポーネント取得と辞書初期化をする。
    /// </summary>
    void Awake()
    {
        _animator = GetComponent<Animator>();
        if (_animator == null)
        {
            Debug.LogError("[t_pl] Animatorコンポーネントが見つからない。", this);
        }

        // 辞書に方向インデックスを初期設定
        lastKeyPressTime.Add(1, 0f); // 下
        lastKeyPressTime.Add(2, 0f); // 上
        lastKeyPressTime.Add(3, 0f); // 右
        lastKeyPressTime.Add(4, 0f); // 左
    }

    /// <summary>
    /// シーンロード時の向き復元処理。
    /// </summary>
    void Start()
    {
        // シーンデータ転送クラスから、保存された向きインデックスをロードする
        if (SceneDataTransfer.Instance != null && SceneDataTransfer.Instance.playerDirectionIndexToLoad != 0)
        {
            lastDirectionIndex = SceneDataTransfer.Instance.playerDirectionIndexToLoad;
            UpdateAnimator(lastDirectionIndex);
        }
        else
        {
            // データがない場合はAwakeで設定されたデフォルト値を使用する
            UpdateAnimator(lastDirectionIndex);
        }
    }

    /// <summary>
    /// 毎フレームの向き計算とアニメーション更新処理。
    /// </summary>
    void Update()
    {
        if (_animator == null) return;
        var keyboard = Keyboard.current; // InputSystemに依存
        if (keyboard == null) return;

        // 1. 押されているキーをチェックし、タイムスタンプを更新する
        List<int> pressedDirections = new List<int>();

        // キーが押されている間 (isPressed) は、常にタイムスタンプを最新に更新する
        if (keyboard.downArrowKey.isPressed) { lastKeyPressTime[1] = Time.time; pressedDirections.Add(1); }
        if (keyboard.upArrowKey.isPressed) { lastKeyPressTime[2] = Time.time; pressedDirections.Add(2); }
        if (keyboard.rightArrowKey.isPressed) { lastKeyPressTime[3] = Time.time; pressedDirections.Add(3); }
        if (keyboard.leftArrowKey.isPressed) { lastKeyPressTime[4] = Time.time; pressedDirections.Add(4); }

        // 2. 押されているキーの中から、最後に押されたキーを特定する
        if (pressedDirections.Count > 0)
        {
            int preferredIndex = 0;
            float latestTime = -1f;

            foreach (int index in pressedDirections)
            {
                // 最後に押された時刻（最新の時刻）を優先
                if (lastKeyPressTime[index] > latestTime)
                {
                    latestTime = lastKeyPressTime[index];
                    preferredIndex = index;
                }
            }

            // 最後に押されたキーの向きでアニメーションを更新する
            SetDirection(preferredIndex);
        }

        // 3. アニメーターに現在の向きを反映する
        UpdateAnimator(lastDirectionIndex);
    }

    // --- プライベートメソッド ---

    /// <summary>
    /// 内部の向きインデックスを更新する。
    /// </summary>
    private void SetDirection(int newIndex)
    {
        if (newIndex != 0)
        {
            lastDirectionIndex = newIndex;
        }
    }

    /// <summary>
    /// AnimatorのDirectionパラメーターを更新する。
    /// </summary>
    private void UpdateAnimator(int directionIndex)
    {
        if (_animator != null)
        {
            _animator.SetInteger(DirectionParam, directionIndex);
        }
    }

    // --- 外部呼び出し用メソッド ---

    /// <summary>
    /// TimeTravelControllerからの呼び出し用。シーンロード後に向きを復元する。
    /// </summary>
    public void LoadDirectionIndex(int index)
    {
        if (index != 0)
        {
            lastDirectionIndex = index;
            UpdateAnimator(lastDirectionIndex);
        }
    }
}