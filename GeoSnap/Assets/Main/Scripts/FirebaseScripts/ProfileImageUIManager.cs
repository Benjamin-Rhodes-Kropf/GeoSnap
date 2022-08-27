using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProfileImageUIManager : MonoBehaviour
{
    [Header("ProfileImage")]
    [SerializeField]private Image _profileImage;
    public TMP_InputField profileName;
    
    
    [Header("Test")]
    [SerializeField]private Sprite profileImageOne;
    [SerializeField]private Sprite profileImageTwo;
    [SerializeField]private Sprite profileImageThree;


    private void Awake()
    {
        _profileImage.sprite = profileImageOne;
    }

    public void SelectPhoto()
    {
        if (_profileImage.sprite == profileImageOne)
        {
            _profileImage.sprite = profileImageTwo;
        }
        else if (_profileImage.sprite == profileImageTwo)
        {
            _profileImage.sprite= profileImageThree;
        }
        else if (_profileImage.sprite == profileImageThree)
        {
            _profileImage.sprite = profileImageOne;
        }
    }
    
    public void SaveToDatabase()
    {
        
    }
}
