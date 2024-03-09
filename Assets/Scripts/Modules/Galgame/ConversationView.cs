using AssetManagement;
using Common.Game;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using TetraCreations.Attributes;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using XGUI;
using static XModules.GalManager.Struct_PlotData;
namespace XModules.GalManager
{
    /// <summary>
    /// 存储整个剧本的XML文档
    /// </summary>
    
    [Serializable]
    public class Struct_PlotData
    {
        public List<XElement> ListMainPlot = new();
        public class Struct_Choice
        {
            public string Title;
            public int JumpID;
        }
        public class Struct_CharacterInfo
        {
            public string characterID;
            public string name;
            public string image;
            public bool isSelf = false;
        }
        public List<Struct_CharacterInfo> CharacterInfoList = new();
        public List<Struct_Choice> ChoiceTextList = new();
        /// <summary>
        /// 当前的剧情节点
        /// </summary>
        public XElement NowPlotDataNode;

        /// <summary>
        /// 当前是否为分支剧情节点
        /// </summary>
        public int NextJumpID;

    }

    public class ConversationView : XBaseView
    {
        //旁白
        public GalManager_AsideText Gal_AsideText;

        //自己对话
        public GalManager_Text Gal_SelfText;

        //别人对话
        public GalManager_Text Gal_OtherText;

        //对话选择
        public GalManager_Choice Gal_Choice;

        //有输入框的选择
        public GalManager_Message Gal_Message;

        //角色
        public GalManager_CharacterImg character_img;

        //角色动画
        public GalManager_CharacterAnimate character_animate;

        //背景
        public GalManager_BackImg Gal_BackImg;

        Struct_CharacterInfo SelfCharacterInfo = null;

        [SerializeField]
        XButton TouchBack;

        [SerializeField]
        XButton ButtonReturn;

        bool loadXmlData = false;

        //string _CharacterInfoText;
        //string _DepartmentText;

        /// <summary>
        /// 当前场景角色数量
        /// </summary>
        [Title("当前场景角色数量")]
        public int CharacterNum;

        private XDocument PlotxDoc;
        public static Struct_PlotData PlotData = new();
        private void Awake ()
        {

            TouchBack.onClick.AddListener(() =>
            {
                Button_Click_NextPlot();
            });

            ButtonReturn.onClick.AddListener(() => {

                XGUIManager.Instance.CloseView("ConversationView");
                XGUIManager.Instance.OpenView("MainView");
            });
        }

        public override void OnEnableView()
        {
            base.OnEnableView();

            ClearGame();

            ResetPlotData();

            string storyName = viewArgs[0] as string;

            StartCoroutine(LoadPlot(storyName));

            XEvent.EventDispatcher.AddEventListener("NEXT_STEP", Button_Click_NextPlot,this);
            XEvent.EventDispatcher.AddEventListener("CHOICE_COMPLETE", ChoiceComplete, this);

            //if (!loadXmlData)
            //    return;

            ////开始游戏
            //Button_Click_NextPlot();
        }

        public override void OnDisableView()
        {
            base.OnDisableView();
            XEvent.EventDispatcher.RemoveEventListener("NEXT_STEP", Button_Click_NextPlot, this);
            XEvent.EventDispatcher.RemoveEventListener("CHOICE_COMPLETE", ChoiceComplete, this);
        }

        void ClearGame()
        {
            //foreach (var item in PlotData.CharacterInfoList)
            //{
            //    DestroyCharacterByID(item.characterID);
            //}

            PlotData.CharacterInfoList.Clear();
        }

        void ChoiceComplete()
        {
            Gal_Choice.SetActive(false);
            Gal_Message.SetActive(false);
        }


        /// <summary>
        /// 重置
        /// </summary>
        private void ResetPlotData ()
        {
            PlotData = new Struct_PlotData();
        }
        /// <summary>
        /// 解析框架文本
        /// </summary>
        /// <returns></returns>
        public IEnumerator LoadPlot (string storyName)
        {
            yield return null;

            string _PlotText = string.Empty;
            //string filePath = Path.Combine(AssetDefine.BuildinAssetPath, "HGF/Test.xml");
            string filePath = Path.Combine(AssetDefine.BuildinAssetPath, $"HGF/{storyName}.xml");

#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
            filePath = "file://" + filePath;
#endif
            //            if (Application.platform == RuntimePlatform.Android)
            //            {
            //                filePath = "jar:file://" + Application.dataPath + "!/assets/HGF/Test.xml";
            //            }

            UnityWebRequest www = UnityWebRequest.Get(filePath);
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                _PlotText = www.downloadHandler.text;
            }
            else
            {
                Debug.Log("Error: " + www.error);
            }
            //try
            {

                //GameAPI.Print($"游戏剧本：{_PlotText}");
                PlotxDoc = XDocument.Parse(_PlotText);

                //-----开始读取数据

                foreach (var item in PlotxDoc.Root.Elements())
                {
                    switch (item.Name.ToString())
                    {
                        case "Plot":
                            {
                                foreach (var MainPlotItem in item.Elements())
                                {
                                    PlotData.ListMainPlot.Add(MainPlotItem);
                                }
                                break;
                            }
                        default:
                            {
                                throw new Exception("无法识别的根标签");

                            }
                    }
                }
            }
 
            GameAPI.Print(Newtonsoft.Json.JsonConvert.SerializeObject(PlotData));

            loadXmlData = true;

            //开始游戏
            Button_Click_NextPlot();
        }


        void DisableAllText()
        {
            Gal_AsideText.SetActive(false);
            Gal_SelfText.SetActive(false);
            Gal_OtherText.SetActive(false);
            Gal_Message.SetActive(false);
            character_img.SetActive(false);
        }

        /// <summary>
        /// 点击屏幕 下一句
        /// </summary>
        public void Button_Click_NextPlot ()
        {

            //if (PlotData.MainPlot.Count == 0)
            //{
            //    GameAPI.Print("游戏结束!");
            //    return;
            //}
            //IsCanJump这里有问题，如果一直点击会为false，而不是说true，这是因为没有点击按钮 ，没有添加按钮
            if (GalManager_Text.IsSpeak || !GalManager_Text.IsCanJump) { return; }

            PlotData.ChoiceTextList.Clear();

            if (PlotData.NowPlotDataNode == null)
            {
                PlotData.NowPlotDataNode = PlotData.ListMainPlot[0];
                Debug.Log("NowPlotDataNode 是空节点，从头开始");
            }
            else 
            {
                PlotData.NowPlotDataNode = PlotData.ListMainPlot[PlotData.NextJumpID-1];
                Debug.Log($"正在运行 {PlotData.NextJumpID} 节点");
            }

            DisableAllText();

            switch (PlotData.NowPlotDataNode.Name.ToString())
            {
                case "NextChapter"://空节点
                    {
                        PlotData.NextJumpID = int.Parse(PlotData.NowPlotDataNode.Attribute("JumpId").Value);
                        Button_Click_NextPlot();

                        break;
                    }
                case "AddCharacter"://处理添加角色信息的东西
                    {
                        var characterInfo = new Struct_CharacterInfo();
                        var _CharacterId = PlotData.NowPlotDataNode.Attribute("CharacterID").Value;
                        characterInfo.name = PlotData.NowPlotDataNode.Attribute("CharacterName").Value;
                        characterInfo.image = PlotData.NowPlotDataNode.Attribute("CharacterImage").Value;
                        characterInfo.characterID = _CharacterId;
                        characterInfo.isSelf = PlotData.NowPlotDataNode.Attribute("IsSelf").Value == "True";

                        if(characterInfo.isSelf)
                        {
                            SelfCharacterInfo = characterInfo;
                        }
                        
                        character_img.SetImage(characterInfo.image);

                        if (PlotData.NowPlotDataNode.Attributes("SendMessage").Count() != 0)
                        {
                            character_animate.Animate_StartOrOutside = PlotData.NowPlotDataNode.Attribute("SendMessage").Value;
                        }

                        PlotData.CharacterInfoList.Add(characterInfo);
                        PlotData.NextJumpID = int.Parse(PlotData.NowPlotDataNode.Attribute("JumpId").Value);

                        Button_Click_NextPlot();

                        break;
                    }
                case "SpeakAside": //处理旁白
                    { 
                        Gal_AsideText.SetActive(true);
                        Gal_AsideText.StartTextContent(PlotData.NowPlotDataNode.Attribute("Content").Value);

                        if (PlotData.NowPlotDataNode.Attributes("AudioPath").Count() != 0)
                            PlayAudio(PlotData.NowPlotDataNode.Attribute("AudioPath").Value);

                        PlotData.NextJumpID = int.Parse(PlotData.NowPlotDataNode.Attribute("JumpId").Value);

                        break;
                    }
                case "Message": //有对话框选项
                    {

                        if (PlotData.NowPlotDataNode.Elements().Count() != 0) //有选项，因为他有子节点数目了
                        {
                            character_img.SetActive(true);
                            character_img.SetImage(SelfCharacterInfo.image);

                            GalManager_Text.IsCanJump = false;
                            foreach (var ClildItem in PlotData.NowPlotDataNode.Elements())
                            {
                                if (ClildItem.Name.ToString() == "Choice")
                                    PlotData.ChoiceTextList.Add(new Struct_Choice { Title = ClildItem.Value, JumpID = int.Parse(ClildItem.Attribute("JumpId").Value) });
                            }

                            Gal_Message.SetActive(true);
                            Gal_Message.CreatNewChoice(ConversationView.PlotData.ChoiceTextList);

                            SendCharMessage("","", true);
                        }

                        break;
                    }
                case "Speak":  //处理发言
                    {
                        character_img.SetActive(true);
                        var characterInfo = GetCharacterObjectByName(PlotData.NowPlotDataNode.Attribute("CharacterID").Value);

                        var imagePathNode = PlotData.NowPlotDataNode.Attribute("CharacterImage");
                        if (imagePathNode != null)
                        {
                            character_img.SetImage(imagePathNode.Value);
                            characterInfo.image = imagePathNode.Value;
                        }
                        else
                        {
                            character_img.SetImage(characterInfo.image);
                        }

                        if (characterInfo.isSelf)
                        {
                            Gal_SelfText.SetActive(true);
                            if (PlotData.NowPlotDataNode.Elements().Count() != 0) //有选项，因为他有子节点数目了
                            {
                                
                                GalManager_Text.IsCanJump = false;
                                foreach (var ClildItem in PlotData.NowPlotDataNode.Elements())
                                {
                                    if (ClildItem.Name.ToString() == "Choice")
                                        PlotData.ChoiceTextList.Add(new Struct_Choice { Title = ClildItem.Value, JumpID = int.Parse(ClildItem.Attribute("JumpId").Value) });

                                }
                                Gal_SelfText.StartTextContent(PlotData.NowPlotDataNode.Attribute("Content").Value, characterInfo.name, () =>
                                {
                                    Gal_Choice.SetActive(true);
                                    Gal_Choice.CreatNewChoice(ConversationView.PlotData.ChoiceTextList);
                                });
                            }
                            else
                            {
                                Gal_SelfText.StartTextContent(PlotData.NowPlotDataNode.Attribute("Content").Value, characterInfo.name);
                                PlotData.NextJumpID = int.Parse(PlotData.NowPlotDataNode.Attribute("JumpId").Value);
                            }
                        }
                        else
                        {
                            Gal_OtherText.SetActive(true);
                            Gal_OtherText.StartTextContent(PlotData.NowPlotDataNode.Attribute("Content").Value, characterInfo.name);
                            PlotData.NextJumpID = int.Parse(PlotData.NowPlotDataNode.Attribute("JumpId").Value);
                        }

                        //处理消息
                        //if (PlotData.NowPlotDataNode.Attributes("SendMessage").Count() != 0)
                        //{
                        //    SendCharMessage(characterInfo.characterID, PlotData.NowPlotDataNode.Attribute("SendMessage").Value, characterInfo.isSelf);
                        //}
                        SendCharMessage(characterInfo.characterID, "", characterInfo.isSelf);

                        if (PlotData.NowPlotDataNode.Attributes("AudioPath").Count() != 0)
                            PlayAudio(PlotData.NowPlotDataNode.Attribute("AudioPath").Value);
                        break;
                    }
                case "ChangeBackImg"://更换背景图片
                    {
                        var _Path = PlotData.NowPlotDataNode.Attribute("Path").Value;
                        Gal_BackImg.SetImage(_Path);

                        PlotData.NextJumpID = int.Parse(PlotData.NowPlotDataNode.Attribute("JumpId").Value);
                        Button_Click_NextPlot();
                        break;
                    }
                case "DeleteCharacter":
                    {
                        character_img.SetActive(true);
                        DestroyCharacterByID(PlotData.NowPlotDataNode.Attribute("CharacterID").Value);
                        PlotData.NextJumpID = int.Parse(PlotData.NowPlotDataNode.Attribute("JumpId").Value);

                        break;
                    }
                case "Video":
                    {
                        var _Path = PlotData.NowPlotDataNode.Attribute("Path").Value;
                        PlotData.NextJumpID = int.Parse(PlotData.NowPlotDataNode.Attribute("JumpId").Value);

                        XGUIManager.Instance.OpenView("VideoView",UILayer.VideoLayer, Button_Click_NextPlot, _Path);
                        break;
                    }
                case "ExitGame":
                    {
                        ClearGame();
                        StartCoroutine(closeGameEndOfFrame());
                        break;
                    }
            }

            return;
        }

        IEnumerator closeGameEndOfFrame()
        {
            yield return new WaitForEndOfFrame();

            XGUIManager.Instance.CloseView("ConversationView");
            XGUIManager.Instance.OpenView("MainView");
        }

        public void Button_Click_FastMode ()
        {
            GalManager_Text.IsFastMode = true;
            return;
        }

        public Struct_CharacterInfo GetCharacterObjectByName (string ID)
        {
            return PlotData.CharacterInfoList.Find(t => t.characterID == ID);
        }

        /// <summary>
        /// 销毁一个角色
        /// </summary>
        /// <param name="ID"></param>
        public void DestroyCharacterByID (string ID)
        {
            var _ = PlotData.CharacterInfoList.Find(t => t.characterID == ID);
            //SendCharMessage(ID, "Quit");
            PlotData.CharacterInfoList.Remove(_);
        }
        
        public void SendCharMessage (string CharacterID, string Message,bool isSelf)
        {
            //var _t = GetCharacterObjectByName(CharacterID);
            //_t.CharacterLoader.HandleMessage(Message);

            //character_animate.Animate_type = Message;
            character_animate.HandleMessgaeTemp(isSelf);
        }

        private void PlayAudio (string fileName)
        {
            Debug.Log("播放了声音:" + fileName);

            XAudio.XAudioManager.instance.PlayGameMusic(fileName);
        }

        private void FixedUpdate ()
        {
            CharacterNum = PlotData.CharacterInfoList.Count;
        }
        private void Update ()
        {

        }
    }
}