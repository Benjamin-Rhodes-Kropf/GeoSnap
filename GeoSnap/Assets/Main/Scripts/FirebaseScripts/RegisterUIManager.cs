using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RegisterUIManager : MonoBehaviour
{
    //Register variables
    [Header("Register")]
    public TMP_InputField usernameRegisterField;
    public TMP_InputField emailRegisterField;
    public TMP_InputField passwordRegisterField;
    public TMP_InputField passwordRegisterVerifyField;
    public TMP_Text warningRegisterText;
    public TMP_Text confirmRegisterText;
    
    public void ClearRegisterFeilds()
    {
        usernameRegisterField.text = "";
        emailRegisterField.text = "";
        passwordRegisterField.text = "";
        passwordRegisterVerifyField.text = "";
    }
    
    //Function for the register button
    public void RegisterButton()
    {
        if (passwordRegisterField.text != passwordRegisterVerifyField.text)
        {
            confirmRegisterText.text = "";
            warningRegisterText.text = "Passwords do not match!";
            return;
        }
        
        //Call the register coroutine passing the email, password, and username
        StartCoroutine(FirebaseManager.instance.TryRegister(emailRegisterField.text, passwordRegisterField.text, usernameRegisterField.text,  (myReturnValue) => {
            if (myReturnValue != null)
            {
                confirmRegisterText.text = "";
                warningRegisterText.text = myReturnValue;
            }
            else
            {
                warningRegisterText.text = "";
                confirmRegisterText.text = "Confirmed, Account Created!";
            }
        }));
    }
}
