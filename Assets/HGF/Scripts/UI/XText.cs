using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;

namespace XGUI
{
	public class XText : Text
	{
        private static Color grayColor = new Color(58 / 255.0f, 69 / 255.0f, 92 / 255.0f, 1);
        private static Color grayOutlineColor = new Color(122 / 255.0f, 122 / 255.0f, 122 / 255.0f, 1);
        private static Color VoidColor = new Color(0, 0, 0, 0);
        /// <summary>
        /// 是否静态字体
        /// </summary>
        [SerializeField]
        public bool isStatic = false;
        [SerializeField]
        public int languageId = 0;

        [HideInInspector]
        public Color defaultColor = Color.white;



        private Outline _outline;
        public Outline OutLine
        {
            get
            {
                if (this._outline == null)
                    this._outline = gameObject.GetComponent<Outline>();
                return _outline;
            }
        }

        private Color m_CacheColor;
        private Color _outlineCacheColor;
        public override Color color
        {
            get
            {

                return base.color;
            }
            set
            {
                this.m_CacheColor = value;
                base.color = value;
            }
        }
        public override string text
        {
            get
            {
                return m_Text;
            }
            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    if (String.IsNullOrEmpty(m_Text))
                        return;
                    m_Text = "";
                }
                else if (m_Text != value)
                {
                    m_Text = value;
                }
                SetVerticesDirty();
                SetLayoutDirty();
            }
        }

        protected override void Awake()
        {
            base.Awake();
            //如果是静态则替换为zh_cn文件对应字符串
            //如果是动态则删除

            if(Application.isPlaying && isStatic && languageId != 0)
            {
                //m_Text = CSharpLuaInterface.GetLanguage(languageId);
            }
            m_CacheColor = this.color;
            if (OutLine)
            {
                _outlineCacheColor = OutLine.effectColor;
            }
        }

        public override void SetVerticesDirty()
        {
            base.SetVerticesDirty();

            if (!IsActive() || string.IsNullOrEmpty(m_Text))
                return;

            UnityEngine.Profiling.Profiler.BeginSample("XText.Space");
            m_Text = Space(m_Text);
            UnityEngine.Profiling.Profiler.EndSample();
        }


        static string Space(string str)
        {
            bool isSpace = false;
            for (int i = 0; i < str.Length; i++)
                if (str[i] == ' ')
                {
                    isSpace = true;
                    break;
                }
            return isSpace ? str.Replace(" ", "\u00A0") : str;
        }

        public void SetGray(bool res)
        {
            if(this.OutLine != null)
            {   
                if (res)
                {
                    if (this.OutLine.effectColor != XText.grayOutlineColor) 
                    this._outlineCacheColor = this.OutLine.effectColor;
                }
                if (this._outlineCacheColor == VoidColor)
                {
                    this._outlineCacheColor = this.OutLine.effectColor;
                }

                this.OutLine.effectColor = res ? XText.grayOutlineColor : this._outlineCacheColor;
            }

            if (this.m_CacheColor == VoidColor)
            {
                this.m_CacheColor = this.color;
            }
            base.color = res ? XText.grayColor : this.m_CacheColor;
        }
    }
}
