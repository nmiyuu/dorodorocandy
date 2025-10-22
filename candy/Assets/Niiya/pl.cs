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

        input = Vector2.zero;

        if (keyboard.leftArrowKey.isPressed)
            input.x = -1;
        else if (keyboard.rightArrowKey.isPressed)
            input.x = 1;

        if (keyboard.upArrowKey.isPressed)
            input.y = 1;
        else if (keyboard.downArrowKey.isPressed)
            input.y = -1;

        if (input != Vector2.zero)
        {
            lastDirection = input;

            if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
            {
                spriteRenderer.sprite = input.x > 0 ? rightSprite : leftSprite;
            }
            else
            {
                spriteRenderer.sprite = input.y > 0 ? upSprite : downSprite;
            }
        }
    }
}