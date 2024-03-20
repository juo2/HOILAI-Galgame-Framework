using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

namespace XNode.Story
{
    public class MessageBox : MonoBehaviour
    {
        public Button sureBtn;
        public Button closeBtn;
        public Button cancelBtn;
        public Text content;

        // Start is called before the first frame update
        void Start()
        {
            sureBtn.onClick.AddListener(() => 
            {
                gameObject.SetActive(false);
            });

            closeBtn.onClick.AddListener(() => 
            { 
                gameObject.SetActive(false);
            });

            cancelBtn.onClick.AddListener(() => 
            { 
                gameObject.SetActive(false);
            });
        }

        public void SetContent(string message)
        {
            gameObject.SetActive(true);
            content.text = message;
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
