using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Linq;

public class MoveBlock : MonoBehaviour
{
    public float moveUnit = 1.0f;
    public float moveSpeed = 5f;

    [Tooltip("���̃u���b�N���V�[�����܂����Ŏ��ʂ��邽�߂�ID��ݒ�")]
    public string blockID;

    public LayerMask pushBlockerLayer;

    private bool isMoving = false;
    private Vector3 targetPos;
    private BoxCollider2D blockCollider;

    public string presentSceneName = "Stage1_now";

    private Vector3 initialPosition;

    void Awake()
    {
        blockCollider = GetComponent<BoxCollider2D>();
        if (blockCollider == null)
        {
            Debug.LogError($"MoveBlock '{gameObject.name}' �� BoxCollider2D ���A�^�b�`����Ă��܂���B");
        }

        initialPosition = new Vector3(
            Mathf.Round(transform.position.x),
            Mathf.Round(transform.position.y),
            Mathf.Round(transform.position.z)
        );

        if (string.IsNullOrEmpty(blockID))
        {
            blockID = gameObject.name;
            Debug.LogWarning($"MoveBlock '{gameObject.name}' �� ID ���ݒ肳��Ă��܂���B�I�u�W�F�N�g����ID�Ƃ��Ďg�p���܂��B");
        }
    }

    void Start()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;

        if (currentSceneName == presentSceneName)
        {
            LoadBlockPosition();
        }
        else
        {
            transform.position = initialPosition;
        }
    }

    private void LoadBlockPosition()
    {
        if (SceneDataTransfer.Instance == null) return;

        BlockState? savedState = SceneDataTransfer.Instance.pastBlockStates
            .Where(state => state.id == blockID)
            .Cast<BlockState?>()
            .FirstOrDefault();

        if (savedState.HasValue)
        {
            Vector3 finalPosition = savedState.Value.finalPosition;

            transform.position = finalPosition;
            Debug.Log($"����u���b�N '{blockID}' ��ۑ��ʒu {finalPosition} �ɔz�u���܂����B");
        }
    }

    public bool TryMove(Vector3 direction)
    {
        if (isMoving || blockCollider == null) return false;

        Vector2 origin = (Vector2)transform.position + blockCollider.offset;
        Vector2 size = blockCollider.size;
        float angle = 0f;
        float checkDistance = moveUnit * 1.01f;

        RaycastHit2D hit = Physics2D.BoxCast(origin, size, angle, direction, checkDistance, pushBlockerLayer);

        if (hit.collider == null)
        {
            targetPos = transform.position + direction * moveUnit;
            StartCoroutine(MoveToPosition(targetPos));
            return true;
        }
        else
        {
            return false;
        }
    }

    IEnumerator MoveToPosition(Vector3 target)
    {
        isMoving = true;

        while ((transform.position - target).sqrMagnitude > 0.001f)
        {
            transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = target;
        isMoving = false;
    }

    public bool IsMoving => isMoving;
}