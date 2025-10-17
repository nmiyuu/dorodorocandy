using UnityEngine;

public class pikuseru : MonoBehaviour
{
    public float moveUnit = 1.0f; // 1マス = 1ユニット（64px相当）
    private bool isMoving = false;
    private Vector3 targetPos;

    void Start()
    {
        targetPos = transform.position;
    }

    void Update()
    {
        if (isMoving) return;

        Vector3 dir = Vector3.zero;

        if (Input.GetKeyDown(KeyCode.RightArrow)) dir = Vector3.right;
        if (Input.GetKeyDown(KeyCode.LeftArrow)) dir = Vector3.left;
        if (Input.GetKeyDown(KeyCode.UpArrow)) dir = Vector3.up;
        if (Input.GetKeyDown(KeyCode.DownArrow)) dir = Vector3.down;

        if (dir != Vector3.zero)
        {
            targetPos = transform.position + dir * moveUnit;
            StartCoroutine(MoveToPosition(targetPos));
        }
    }

    System.Collections.IEnumerator MoveToPosition(Vector3 target)
    {
        isMoving = true;

        while ((transform.position - target).sqrMagnitude > 0.001f)
        {
            transform.position = Vector3.MoveTowards(transform.position, target, 10f * Time.deltaTime);
            yield return null;
        }

        transform.position = target;
        isMoving = false;
    }
}