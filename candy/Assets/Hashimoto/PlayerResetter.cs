using UnityEngine;

public class PlayerResetter : MonoBehaviour
{
    void Update()
    {
        // R�L�[�Ńv���C���[�����Z�b�g�i�Â��v���C���[���폜�j
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetPlayer();
        }
    }

    void ResetPlayer()
    {
        // �^�O�uPlayer�v���t�����I�u�W�F�N�g��T��
        GameObject oldPlayer = GameObject.FindGameObjectWithTag("Player");

        if (oldPlayer != null)
        {
            Destroy(oldPlayer);
            Debug.Log("�Â��v���C���[���폜���܂���");
        }
        else
        {
            Debug.Log("�폜�Ώۂ̃v���C���[��������܂���ł���");
        }
    }
}
