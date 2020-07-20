using System.Collections;
using System.Collections.Generic;
using LMAStudio.StreamVR.Unity.Scripts;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WaitForLoad : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void BeginWait()
    {
        StartCoroutine(WaitForLoadAsync());
    }

    private IEnumerator WaitForLoadAsync()
    {
        Debug.Log("StreamVR Waiting...");
        while (!StreamVR.Instance.IsLoaded)
        {
            Debug.Log("StreamVR NOt yet...");
            yield return new WaitForSeconds(0.5f);
        }
        Debug.Log("StreamVR LOADED!!");
        EnterStreamVR();
    }

    private void EnterStreamVR()
    {
        SceneManager.LoadScene("VRGame");
    }
}
