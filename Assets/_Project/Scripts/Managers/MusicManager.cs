using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    [SerializeField] private AudioSource musicSource;
    private bool isMuted = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void ToggleMusic()
    {
        isMuted = !isMuted;
        musicSource.mute = isMuted;
    }

    public bool IsMuted() => isMuted;
}
