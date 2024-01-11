using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace XGUI
{
    [RequireComponent(typeof(XListView), typeof(XScrollRect))]
    public class XListViewCenter : MonoBehaviour, IBeginDragHandler, IEndDragHandler
    {
        private XListView m_XListView;
        private XScrollRect m_XScrollRect;
        private bool m_IsBeginDrag;
        //public float smoothTime = 0.1f;

        public int m_ToIndex = -1;

        public UnityAction<int> onFinish;

        public bool isInertia = true;  //是否带有惯性滑动
        private bool m_isInertiaDrag = false;  //惯性滑动开始
        private Vector2 m_scrollVect = Vector2.zero;

        public float m_Crossvalue = 0.5f;//左手边的过界点         

        public UnityAction<int> OnCenterCallBack;

        private float HorizontalScrollValue;

        private void Awake()
        {
            m_XListView = GetComponent<XListView>();
            m_XScrollRect = GetComponent<XScrollRect>();
            if (m_XScrollRect != null)
            {
                m_XScrollRect.checkScrollValid = isInertia;
                m_XScrollRect.onValueChanged.AddListener(OnScroll);
            }
        }               

        private void OnScroll(Vector2 value)
        {
            HorizontalScrollValue = Input.GetAxis("Mouse X");                 
            if (m_isInertiaDrag && isInertia)
            {
                float offsetY = Mathf.Abs(m_scrollVect.y - value.y);
                float offsetX = Mathf.Abs(m_scrollVect.x - value.x);
                if ((offsetY > 0 && offsetY < 0.0005f) || (offsetX > 0 && offsetX < 0.0005f))
                {
                    m_isInertiaDrag = false;
                    Recenter();
                }
            }
            m_scrollVect = value;
            //if (value.sqrMagnitude < 0.01f)
            //{
            //    if (onFinish != null && m_ToIndex != -1) onFinish.Invoke(m_ToIndex);
            //    m_ToIndex = -1;
            //    Debug.Log("--------------->");
            //}
        }

        private void Recenter()
        {
            if (m_XScrollRect == null) return;
            if (m_XListView == null) return;
            if (m_XListView.listItems == null) return;
            if (m_XListView.viewRect == null) return;

            Vector2 velocity = m_XScrollRect.velocity * 0.1f;
            

            Vector2 contentHalfSize = m_XListView.viewRect.rect.size * 0.5f;

            float min = float.MaxValue;
            int index = -1;
            Vector2 offset = new Vector2(-m_XListView.scrollOffset.x, m_XListView.scrollOffset.y);
            offset -= velocity;
            float scrollValue = m_Crossvalue;
            if (HorizontalScrollValue > 0)
                scrollValue = 1 - m_Crossvalue;
            else
                scrollValue = m_Crossvalue;
            foreach (var item in m_XListView.listItems)
            {                                                    
                Vector2 size = item.Value.transform.rect.size;

              
                Vector2 pos = new Vector2(item.Value.transform.localPosition.x + size.x * scrollValue, item.Value.transform.localPosition.y - size.y * 0.5f);
                pos.x -= offset.x;
                pos.y = Mathf.Abs(pos.y - offset.y);

                float sqr = (pos - contentHalfSize).sqrMagnitude;

                if (sqr < min)
                {
                    min = sqr;
                    index = item.Key;
                }
            }
            m_XScrollRect.StopMovement();
            ScrollToIndex(index);
        }


        public void ScrollToIndex(int index,float smoothTime = 0.1f)
        {
            m_ToIndex = index;
            if (m_XListView != null)
            {
                m_XListView.ScrollToIndex(index, smoothTime, true);
            }
            if (OnCenterCallBack != null)
            {
                OnCenterCallBack.Invoke(m_ToIndex);
            }
        }

        public int Getm_ToIndex()
        {
            return m_ToIndex;
        }

        private void OnDestroy()
        {
            if (m_XScrollRect != null && m_XScrollRect.onValueChanged != null)
            {
                m_XScrollRect.onValueChanged.RemoveListener(OnScroll);
            }
            if (OnCenterCallBack != null)
            {
                OnCenterCallBack = null;
            }
            onFinish = null;
        }

        private void OnEnable()
        {
            Recenter();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            m_IsBeginDrag = true;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (isInertia)
            {
                m_isInertiaDrag = true;
                return;
            }
            if (m_IsBeginDrag)
            {
                Recenter();
                m_IsBeginDrag = false;
            }
        }
    }
}

