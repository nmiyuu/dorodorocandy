using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    void Start()
    {
        // ‹N“®‚É Scene1 ‚Æ Scene2 ‚ğ’Ç‰Áƒ[ƒh
        SceneManager.LoadScene("Stage1_now", LoadSceneMode.Additive);
        SceneManager.LoadScene("Stage1_Past", LoadSceneMode.Additive);
    }
}