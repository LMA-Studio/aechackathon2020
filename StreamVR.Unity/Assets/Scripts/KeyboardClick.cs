using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyboardClick : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void keyboardClick()
    {
        string letter = this.GetComponentInChildren<TMPro.TMP_Text>().text;

        if (letter == "SPACE")
        {
            letter = " ";
        }

        FindObjectOfType<InputFieldHandler>().typeLetter(letter);
    }

    public void deleteClick()
    {
       FindObjectOfType<InputFieldHandler>().deleteLetter(); 
    }

    public void enterClick()
    {
        FindObjectOfType<InputFieldHandler>().enterKey(); 
    }

}
