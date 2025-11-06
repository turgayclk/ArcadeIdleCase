using UnityEngine;
using UnityEngine.UI;

public class MusicToggleButton : MonoBehaviour
{
    [SerializeField] private Sprite musicOnSprite;
    [SerializeField] private Sprite musicOffSprite;
    [SerializeField] private Image buttonImage;

    private Button musicButton;

    private void Awake()
    {
        musicButton = GetComponent<Button>();

        musicButton.onClick.AddListener(OnMusicButtonPressed);
    }

    private void Start()
    {
        UpdateIcon();
    }

    public void OnMusicButtonPressed()
    {
        MusicManager.Instance.ToggleMusic();
        UpdateIcon();
    }

    private void UpdateIcon()
    {
        buttonImage.sprite = MusicManager.Instance.IsMuted() ? musicOffSprite : musicOnSprite;
    }
}
