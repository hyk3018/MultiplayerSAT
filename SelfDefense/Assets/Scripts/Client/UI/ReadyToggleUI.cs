using Server;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ReadyToggleUI : MonoBehaviour
{
    [SerializeField]
    private GameManager gameManager;

    [SerializeField]
    private TextMeshProUGUI readyText;
    
    private Button _button;
    private Image _image;
    private bool _ready;

    private void Awake()
    {
        _button = GetComponent<Button>();
        _image = GetComponent<Image>();
        _button.onClick.AddListener(() =>
        {
            _ready = !_ready;
            gameManager.SetReadyServerRpc(_ready);
            _image.color = _ready ? new Color(0.4f, 0.94f, 0.34f) : new Color(0.94f, 0.32f, 0.32f);
            readyText.text = _ready ? "Ready!" : "Click When Ready";
        });
    }
}
