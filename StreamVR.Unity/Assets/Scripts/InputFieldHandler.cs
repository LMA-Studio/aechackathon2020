using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InputFieldHandler : MonoBehaviour
{
    public GameObject keyboard;
    public TMPro.TMP_InputField url;
    public TMPro.TMP_InputField username;
    public TMPro.TMP_InputField roomcode;
    
    private TMPro.TMP_InputField currentActiveInputField;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void setURLActive()
    {
        currentActiveInputField = url;
        keyboard.SetActive(true);
        currentActiveInputField.text = "";
    }

    public void setRoomcodeActive()
    {
        currentActiveInputField = roomcode;
        keyboard.SetActive(true);
        currentActiveInputField.text = "";
    }

    public void setUserNameActive()
    {
        currentActiveInputField = username;
        keyboard.SetActive(true);
        currentActiveInputField.text = "";
    }

    public void typeLetter(string letter)
    {
        currentActiveInputField.text += letter;
    }

    public void deleteLetter()
    {
        string currenttext = currentActiveInputField.text;
        currentActiveInputField.text = currenttext.Substring(0, currenttext.Length - 1);
    }

    public void enterKey()
    {
        keyboard.SetActive(false);
    }
}
