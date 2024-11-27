using TMPro;
using UnityEngine;

public class UsernameInputValidator : MonoBehaviour
{
    public TMP_InputField inputField; 

    void Start()
    {
      
        inputField.onValueChanged.AddListener(ValidateInput);
    }

    void ValidateInput(string text)
    {
        
        string filteredText = "";
        foreach (char c in text)
        {
            if (c >= 'a' && c <= 'z')
            {
                filteredText += c;
            }
        }

        
        if (filteredText != text)
        {
            inputField.text = filteredText;
        }
    }

    void OnDestroy()
    {
        
        inputField.onValueChanged.RemoveListener(ValidateInput);
    }
}
