using UnityEngine;

public class ClearManager : MonoBehaviour
{
    public AudioSource clearSE;

    public void GameClear()
    {
        clearSE.Play();
        Debug.Log("Game Clear!");
    }
}
