using UnityEngine;
using UnityEngine.InputSystem;  // �VInput System�p�̖��O���

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
        // �VInput System�ŃL�[�{�[�h�̖��L�[���͂��擾
        var keyboard = Keyboard.current;

        if (keyboard == null) return;

        // �����ꂽ�u�Ԃ̃L�[���`�F�b�N
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

        // ��������������Ă��Ȃ���΁A������Ă���L�[�ɍ��킹�ăX�v���C�g�X�V�i�����ێ��̂��߁j
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
