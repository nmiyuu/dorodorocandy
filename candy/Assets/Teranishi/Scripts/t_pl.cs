using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// プレイヤーのアニメーションと向きを管理するスクリプト。
/// 特徴: 向きの計算ロジックはt_player.csに任せて、アニメーション表示とシーンまたぎの向き保持に専念する。
/// </summary>
public class t_pl : MonoBehaviour
{
    // --- 定数とコンポーネント ---
    private Animator _animator;
    private const string DirectionParam = "Direction"; // AnimatorのIntパラメーター名

    // --- 内部状態 ---
    // 向きのインデックス (1:下, 2:上, 3:右, 4:左)。初期値は「下」
    private int lastDirectionIndex = 1;

    // t_player.csがこの値を読んで使う。最新の向きを渡すプロパティ
    public int CurrentDirectionIndex => lastDirectionIndex;

    // --- Unityライフサイクル ---

    void Awake()
    {
        _animator = GetComponent<Animator>();
        if (_animator == null)
        {
            Debug.LogError("[t_pl] Animatorコンポーネントがない");
        }
    }

    void Start()
    {
        // シーンロード時の向き復元処理
        // SceneDataTransferから保存された向きをロードする
        if (SceneDataTransfer.Instance != null && SceneDataTransfer.Instance.playerDirectionIndexToLoad != 0)
        {
            lastDirectionIndex = SceneDataTransfer.Instance.playerDirectionIndexToLoad;
            UpdateAnimator(lastDirectionIndex);
        }
        else
        {
            // ロードデータがない場合は初期値（下=1）でアニメーターを設定する
            UpdateAnimator(lastDirectionIndex);
        }
    }

    /// <summary>
    /// t_playerに移動ロジックを委譲したので、このUpdateは基本的に空にする。
    /// </summary>
    void Update()
    {
        // 入力処理はt_playerで行うため、ここでは何もしない
    }

    // --- 外部連携用メソッド ---

    /// <summary>
    /// t_player.csから呼ばれ、計算された最新の向きをセットし、アニメーターを更新する。
    /// ★ t_player.csとの連携に必須のメソッド
    /// </summary>
    /// <param name="newIndex">t_playerが決定した新しい向きインデックス</param>
    public void SetDirectionFromExternal(int newIndex)
    {
        if (newIndex != 0) // 向きが有効な場合のみ処理する
        {
            // 向きが変わったら値を更新する
            if (newIndex != lastDirectionIndex)
            {
                lastDirectionIndex = newIndex;
            }
            // アニメーターを最新の向きで更新する
            UpdateAnimator(lastDirectionIndex);
        }
    }

    /// <summary>
    /// TimeTravelControllerなどから呼ばれる。シーンロード後やタイムトラベル時に向きを復元する。
    /// ★ TimeTravelControllerとの連携に必須のメソッド
    /// </summary>
    public void LoadDirectionIndex(int index)
    {
        if (index != 0)
        {
            lastDirectionIndex = index;
            // アニメーターを更新する
            UpdateAnimator(lastDirectionIndex);
        }
    }

    // --- プライベートメソッド ---

    private void UpdateAnimator(int directionIndex)
    {
        if (_animator != null)
        {
            // アニメーターのDirectionパラメーターを更新する
            _animator.SetInteger(DirectionParam, directionIndex);
        }
    }
}