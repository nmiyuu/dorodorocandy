using UnityEngine;

public class BGMManager : MonoBehaviour
{
    void Start()
    {
        GetComponent<AudioSource>().Play();
    }
    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
}
