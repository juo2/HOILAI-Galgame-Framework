using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestProxyView : MonoBehaviour
{
    public Button btn1;
    public Button btn2;

    public GameObject gameObject1;
    public GameObject gameObject2;

    // Start is called before the first frame update
    void Start()
    {
        btn1.onClick.AddListener(() => 
        {
            gameObject1.SetActive(true);
            gameObject2.SetActive(false);

        });

        btn2.onClick.AddListener(() =>
        {
            gameObject1.SetActive(false);
            gameObject2.SetActive(true);

        });

        gameObject1.SetActive(true);
        gameObject2.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
