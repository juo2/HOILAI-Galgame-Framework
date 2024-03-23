using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

public class ConnectVue : MonoBehaviour
{
    [DllImport("__Internal")]
    private static extern void PostScore(string currentScene);  //µ±Ç°³¡¾°

    public Text label;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("12345678");
        PostScore("12345678");
    }

    public void OnCallBack(string msg)
    {
        label.text = msg;
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
