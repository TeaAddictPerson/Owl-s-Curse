using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PasswordToggle : MonoBehaviour
{
    public TMP_InputField passwordField; 
    public Image eyeButtonImage;       
    public Sprite eyeOpenSprite;        
    public Sprite eyeClosedSprite;      

    private bool isPasswordVisible = false;

    public void TogglePasswordVisibility()
    {
        isPasswordVisible = !isPasswordVisible;

        if (isPasswordVisible)
        {
            passwordField.contentType = TMP_InputField.ContentType.Standard;
            eyeButtonImage.sprite = eyeOpenSprite; 
        }
        else
        {

            passwordField.contentType = TMP_InputField.ContentType.Password;
            eyeButtonImage.sprite = eyeClosedSprite; 
        }
        passwordField.ForceLabelUpdate();
    }
}
