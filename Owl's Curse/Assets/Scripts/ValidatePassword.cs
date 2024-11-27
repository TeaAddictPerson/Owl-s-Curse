using System.Linq;
using TMPro;
using UnityEngine;

public class ValidatePassword : MonoBehaviour
{
    public TMP_InputField passwordInputField; 
    public int minLength = 3;                
    public int maxLength = 8;                

    void Start()
    {
   
        passwordInputField.onValueChanged.AddListener(ValidateInput);
    }

    void ValidateInput(string text)
    {
        string validCharacters = "abcdefghijklmnopqrstuvwxyz0123456789_+-/()";
        string filteredText = "";

     
        foreach (char c in text)
        {
            if (validCharacters.Contains(c))
            {
                filteredText += c;
            }
        }

    
        if (filteredText.Length > maxLength)
        {
            filteredText = filteredText.Substring(0, maxLength);
        }

      
        if (filteredText != text)
        {
            passwordInputField.text = filteredText;
            passwordInputField.caretPosition = filteredText.Length; 
        }
    }

    void OnDestroy()
    {
        passwordInputField.onValueChanged.RemoveListener(ValidateInput);
    }
}
