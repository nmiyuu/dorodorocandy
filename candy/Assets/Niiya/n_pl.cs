using UnityEngine;
using UnityEngine.InputSystem;  // 新Input System用の名前空間

public class DirectionalSpriteChanger : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public Sprite upSprite;
    public Sprite downSprite;
    public Sprite leftSprite;
    public Sprite rightSprite;

    private Vector2 lastDirection = Vector2.down;
    private Vector2 input;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        // 新Input Systemでキーボードの矢印キー入力を取得
        var keyboard = Keyboard.current;

        if (keyboard == null) return;

        // 押された瞬間のキーをチェック
        if (keyboard.upArrowKey.wasPressedThisFrame)
        {
            SetDirection(Vector2.up);
        }
        else if (keyboard.downArrowKey.wasPressedThisFrame)
        {
            SetDirection(Vector2.down);
        }
        else if (keyboard.leftArrowKey.wasPressedThisFrame)
        {
            SetDirection(Vector2.left);
        }
        else if (keyboard.rightArrowKey.wasPressedThisFrame)
        {
            SetDirection(Vector2.right);
        }

        // もし何も押されていなければ、押されているキーに合わせてスプライト更新（方向維持のため）
        else
        {
            if (keyboard.upArrowKey.isPressed)
            {
                SetDirection(Vector2.up);
            }
            else if (keyboard.downArrowKey.isPressed)
            {
                SetDirection(Vector2.down);
            }
            else if (keyboard.leftArrowKey.isPressed)
            {
                SetDirection(Vector2.left);
            }
            else if (keyboard.rightArrowKey.isPressed)
            {
                SetDirection(Vector2.right);
            }
        }
    }

    void SetDirection(Vector2 dir)
    {
        lastDirection = dir;

        if (dir == Vector2.up)
            spriteRenderer.sprite = upSprite;
        else if (dir == Vector2.down)
            spriteRenderer.sprite = downSprite;
        else if (dir == Vector2.left)
            spriteRenderer.sprite = leftSprite;
        else if (dir == Vector2.right)
            spriteRenderer.sprite = rightSprite;
    }
}
