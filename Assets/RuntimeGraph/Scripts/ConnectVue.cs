using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

public class ConnectVue : MonoBehaviour
{
    [DllImport("__Internal")]
    private static extern void Vue_Upload_Unity(string json);  //´«json

    public Text label;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("12345678");
        Vue_Upload_Unity("12345678");
    }


    public class temp
    {
        public string version;
    }

    public void OnCallBack(string msg)
    {
        temp t = JsonUtility.FromJson<temp>(msg);

        label.text = t.version;



    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
