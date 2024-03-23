using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class ConnectVue : MonoBehaviour
{
    [DllImport("__Internal")]
    private static extern void PostScore(string currentScene);  //µ±Ç°³¡¾°

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("12345678");
        PostScore("12345678");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
