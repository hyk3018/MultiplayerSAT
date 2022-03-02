using Client.UI;
using Server;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ReadyToggleUI : MonoBehaviour
{
    [SerializeField]
    private ReadyListener readyListener;

    [SerializeField]
    private TextMeshProUGUI readyTextObject;

    [SerializeField]
    private string readyText, notReadyText;

    [SerializeField]
    private Color readyColour, notReadyColour;
    
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
            readyListener.SetReadyServerRpc(_ready);
            _image.color = _ready ? readyColour : notReadyColour;
            readyTextObject.text = _ready ? readyText : notReadyText;
        });
    }
}