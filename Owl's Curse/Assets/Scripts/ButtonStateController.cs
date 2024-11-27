using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ButtonStateController : MonoBehaviour
{
    public TMP_InputField inputField; 
    public Button button;             
    public TextMeshProUGUI buttonText; 

    private Color normalTextColor;    
    private Color disabledTextColor;  

    void Start()
    {
        
        normalTextColor = buttonText.color;
        disabledTextColor = new Color(normalTextColor.r, normalTextColor.g, normalTextColor.b, 0.5f);

        SetButtonState(false);

        
        inputField.onValueChanged.AddListener(OnInputValueChanged);
    }

    void OnInputValueChanged(string text)
    {
      
        SetButtonState(!string.IsNullOrEmpty(text));
    }

    void SetButtonState(bool isActive)
    {
        button.interactable = isActive; 
        buttonText.color = isActive ? normalTextColor : disabledTextColor; 
    }

    void OnDestroy()
    {
       
        inputField.onValueChanged.RemoveListener(OnInputValueChanged);
    }
}
