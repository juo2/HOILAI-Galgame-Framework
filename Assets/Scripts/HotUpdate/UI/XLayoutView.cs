using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace XGUI
{
    public class XLayoutView : XView, XIEvent
    {
        public int dataCount = 0;

        public float space = 0f;

        [SerializeField]
        RectTransform content;

        [SerializeField]
        XLayoutItem layoutItem;

        List<XLayoutItem> xLayoutItemList = new List<XLayoutItem>();

        [System.Serializable]
        public class layoutEvent : UnityEvent<XLayoutItem> { }

        [SerializeField]
        private layoutEvent m_OnUpdateRenderer = new layoutEvent();
        private layoutEvent m_OnCreateRenderer = new layoutEvent();

        public layoutEvent onUpdateRenderer { get { return m_OnUpdateRenderer; } }
        public layoutEvent onCreateRenderer { get { return m_OnCreateRenderer; } }

        // Start is called before the first frame update
        public override void Start()
        {
            base.Start();
            initialize();
        }

        void createItem(int index)
        {
            float height = layoutItem.height;
            float width = layoutItem.width;

            XLayoutItem newItem = Instantiate(layoutItem, content);
            newItem.SetActive(true);
            newItem.rectTransform.localPosition = Vector3.zero;
            newItem.rectTransform.localRotation = Quaternion.identity;
            newItem.rectTransform.localScale = Vector3.one;
            newItem.rectTransform.anchoredPosition = new Vector2(index * width + index * space, 0);
            newItem.index = index;
            content.sizeDelta = new Vector2(dataCount * width + space * (dataCount-1), height);

            m_OnCreateRenderer.Invoke(newItem);

            xLayoutItemList.Add(newItem);
        }

        void initialize()
        {
            layoutItem.SetActive(false);
        }

        public void CreateItem()
        {
            for (int i = 0; i < dataCount; i++)
            {
                createItem(i);
            }
        }

        public void AddItem()
        {
            dataCount++;
            createItem(dataCount-1);
        }

        public XLayoutItem GetItem(int index)
        {
            if (index <= xLayoutItemList.Count)
            {
                return xLayoutItemList[index];
            }

            return null;
        }

        // Update is called once per frame
        void Update()
        {

        }

        public override void ClearEvent()
        {
            base.ClearEvent();
        }
    }
}
