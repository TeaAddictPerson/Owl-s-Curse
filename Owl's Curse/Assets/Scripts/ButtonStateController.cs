using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ButtonStateController : MonoBehaviour
{
    public TMP_InputField inputField1; 
    public TMP_InputField inputField2;
    public Button button;
    public TextMeshProUGUI buttonText;

    private Color normalTextColor;
    private Color disabledTextColor;

    void Start()
    {
        normalTextColor = buttonText.color;
        disabledTextColor = new Color(normalTextColor.r, normalTextColor.g, normalTextColor.b, 0.5f);

        SetButtonState(false);

       
        inputField1.onValueChanged.AddListener(OnInputValueChanged);
        inputField2.onValueChanged.AddListener(OnInputValueChanged);
    }

    void OnInputValueChanged(string text)
    {
     
        bool areBothFieldsFilled = !string.IsNullOrEmpty(inputField1.text) && !string.IsNullOrEmpty(inputField2.text);
        SetButtonState(areBothFieldsFilled);
    }

    void SetButtonState(bool isActive)
    {
        button.interactable = isActive;
        buttonText.color = isActive ? normalTextColor : disabledTextColor;
    }

    void OnDestroy()
    {
       
        inputField1.onValueChanged.RemoveListener(OnInputValueChanged);
        inputField2.onValueChanged.RemoveListener(OnInputValueChanged);
    }
}
