using Common.Game;
using NativeWebSocket;
using System;
using System.Collections;
using System.Linq;
using System.Xml.Linq;
using TetraCreations.Attributes;
using UnityEngine;
using UnityEngine.Networking;
using XGUI;
using XModules.Data;
using XModules.Proxy;
using static XModules.Data.ConversationData;

namespace XModules.GalManager
{
    public partial class ConversationView : XBaseView
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


        [SerializeField]
        XButton TouchBack;

        [SerializeField]
        XButton MessageTouchBack;

        [SerializeField]
        XButton ButtonReturn;

        //string _CharacterInfoText;
        //string _DepartmentText;

        /// <summary>
        /// 当前场景角色数量
        /// </summary>
        [Title("当前场景角色数量")]
        public int CharacterNum;

        WebSocket websocket = null;
        bool isConnecting = false;

        private XDocument PlotxDoc;
        private void Awake ()
        {

            TouchBack.onClick.AddListener(() =>
            {
                Button_Click_NextPlot();
            });

            MessageTouchBack.onClick.AddListener(() => 
            {
                if (messageStatus == MessageStatus.SendingMessage)
                {
                    Button_Click_isRequestChating();
                }
                else if(messageStatus == MessageStatus.ReceiveMessage)
                {
                    Button_Click_Message();
                }
                else if (messageStatus == MessageStatus.CompleteMessage)
                {
                    Button_Click_Finish();
                }
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

            ConversationData.ResetPlotData();

            string storyId = viewArgs[0] as string;
            
            ConversationData.currentStory = storyId;

            StartCoroutine(LoadPlot(storyId));

            XEvent.EventDispatcher.AddEventListener("NEXT_STEP", Button_Click_NextPlot,this);
            XEvent.EventDispatcher.AddEventListener("ONESHOTCHAT", OneShotChat, this);
            XEvent.EventDispatcher.AddEventListener("CHOICE_COMPLETE", ChoiceComplete, this);
            XEvent.EventDispatcher.AddEventListener("STREAM_FINISH", StreamFinish, this);

            //if (!loadXmlData)
            //    return;

            ////开始游戏
            //Button_Click_NextPlot();
        }

        public override void OnDisableView()
        {
            XAudio.XAudioManager.instance.StopBgmMusic();
            base.OnDisableView();
            XEvent.EventDispatcher.RemoveEventListener("NEXT_STEP", Button_Click_NextPlot, this);
            XEvent.EventDispatcher.RemoveEventListener("ONESHOTCHAT", OneShotChat, this);
            XEvent.EventDispatcher.RemoveEventListener("CHOICE_COMPLETE", ChoiceComplete, this);
            XEvent.EventDispatcher.RemoveEventListener("STREAM_FINISH", StreamFinish, this);

        }

        void ClearGame()
        {
            //foreach (var item in PlotData.CharacterInfoList)
            //{
            //    DestroyCharacterByID(item.characterID);
            //}
            MessageTouchBack.SetActive(false);

            ChoiceComplete();
            DisableAllText();
            PlotData.CharacterInfoList.Clear();
            ClearHistoryContent();

            Gal_OtherText.KillTween();
            Gal_SelfText.KillTween();
        }

        void ChoiceComplete()
        {
            Gal_Choice.SetActive(false);
            Gal_Message.SetActive(false);
        }

        /// <summary>
        /// 解析框架文本
        /// </summary>
        /// <returns></returns>
        public IEnumerator LoadPlot (string storyId)
        {
            yield return null;

            string _PlotText = string.Empty;
            //string filePath = Path.Combine(AssetDefine.BuildinAssetPath, "HGF/Test.xml");

            string random = DateTime.Now.ToString("yyyymmddhhmmss");
            string url = $"http://appcdn.calfchat.top/story/{storyId}.xml?v={random}";

            Debug.Log($"url:{url}");

//#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
//            filePath = "file://" + filePath;
//#endif
            //            if (Application.platform == RuntimePlatform.Android)
            //            {
            //                filePath = "jar:file://" + Application.dataPath + "!/assets/HGF/Test.xml";
            //            }

            UnityWebRequest www = UnityWebRequest.Get(url);
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

            ProxyManager.SaveStoryRecord(storyId);

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
            //IsCanJump这里有问题，如果一直点击会为false，而不是说true，这是因为没有点击按钮 ，没有添加按钮
            if (ConversationData.IsSpeak || !ConversationData.IsCanJump) { return; }

            DisableAllText();

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

            switch (PlotData.NowPlotDataNode.Name.ToString())
            {
                case "NextChapter"://空节点
                    {
                        PlotData.NextJumpID = int.Parse(PlotData.NowPlotDataNode.Attribute("JumpId").Value);
                        Button_Click_NextPlot();

                        break;
                    }
                case "Bgm"://空节点
                    {
                        var _Path = PlotData.NowPlotDataNode.Attribute("Path").Value;
                        PlotData.NextJumpID = int.Parse(PlotData.NowPlotDataNode.Attribute("JumpId").Value);
                        PlayBgm(_Path);

                        Button_Click_NextPlot();

                        break;
                    }
                case "AddCharacter"://处理添加角色信息的东西
                    {

                        var characterInfo =  ConversationData.AddCharacter();
                        //character_img.SetImage(characterInfo.image);

                        PlotData.CharacterInfoList.Add(characterInfo);
                        PlotData.NextJumpID = int.Parse(PlotData.NowPlotDataNode.Attribute("JumpId").Value);

                        Button_Click_NextPlot();

                        break;
                    }
                case "SpeakAside": //处理旁白
                    {
                        string content = PlotData.NowPlotDataNode.Attribute("Content").Value;
                        Gal_AsideText.SetActive(true);
                        Gal_AsideText.StartTextContent(content);

                        if (PlotData.NowPlotDataNode.Attributes("AudioPath").Count() != 0)
                            PlayAudio(PlotData.NowPlotDataNode.Attribute("AudioPath").Value);

                        PlotData.NextJumpID = int.Parse(PlotData.NowPlotDataNode.Attribute("JumpId").Value);

                        AddHistoryContent("", "旁白", content);

                        break;
                    }
                case "MessageLoop": //哄哄模拟器
                    {
                        currentLoop = 0;

                        if (PlotData.NowPlotDataNode.Attributes("CharacterImage").Count() != 0)
                            SelfCharacterInfo.image = PlotData.NowPlotDataNode.Attribute("CharacterImage").Value;

                        int loop = int.Parse(PlotData.NowPlotDataNode.Attribute("Loop").Value);
                        //int success = int.Parse(PlotData.NowPlotDataNode.Attribute("Success").Value);
                        //int fail = int.Parse(PlotData.NowPlotDataNode.Attribute("Fail").Value);
                        //int value = int.Parse(PlotData.NowPlotDataNode.Attribute("Value").Value);

                        //currentScore = value;

                        character_img.SetActive(true);
                        character_img.SetImage(SelfCharacterInfo.image);

                        ConversationData.IsCanJump = false;

                        Gal_Message.SetActive(true);
                        Gal_Message.BeginMessageLoop(loop);
                        SendCharMessage("", "", true);

                        PlotData.NextJumpID = int.Parse(PlotData.NowPlotDataNode.Attribute("JumpId").Value);
                        EnableWebSocket();

                        break;
                    }
                case "Message": //有对话框选项
                    {

                        if (PlotData.NowPlotDataNode.Elements().Count() != 0) //有选项，因为他有子节点数目了
                        {
                            if (PlotData.NowPlotDataNode.Attributes("CharacterImage").Count() != 0)
                                SelfCharacterInfo.image = PlotData.NowPlotDataNode.Attribute("CharacterImage").Value;

                            character_img.SetActive(true);
                            character_img.SetImage(SelfCharacterInfo.image);

                            ConversationData.IsCanJump = false;
                            foreach (var ClildItem in PlotData.NowPlotDataNode.Elements())
                            {
                                if (ClildItem.Name.ToString() == "Choice")
                                    PlotData.ChoiceTextList.Add(new Struct_PlotData.Struct_Choice { Title = ClildItem.Value, JumpID = int.Parse(ClildItem.Attribute("JumpId").Value) });
                            }


                            EnableWebSocket();
                            Gal_Message.SetActive(true);
                            Gal_Message.CreatNewChoice(PlotData.ChoiceTextList);

                            SendCharMessage("","", true);
                        }

                        break;
                    }
                case "Speak":  //处理发言
                    {
                        character_img.SetActive(true);


                        var characterInfo = GetCharacterObjectByName(PlotData.NowPlotDataNode.Attribute("CharacterID").Value);

                        if (!characterInfo.isSelf)
                            ProxyManager.SaveUserSession(characterInfo.characterID);

                        var content = PlotData.NowPlotDataNode.Attribute("Content").Value;

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
                            Gal_SelfText.transform.Find("element1").SetActive(true);
                            Gal_SelfText.transform.Find("element2").SetActive(false);

                            if (PlotData.NowPlotDataNode.Elements().Count() != 0) //有选项，因为他有子节点数目了
                            {
                                ConversationData.IsCanJump = false;
                                foreach (var ClildItem in PlotData.NowPlotDataNode.Elements())
                                {
                                    if (ClildItem.Name.ToString() == "Choice")
                                        PlotData.ChoiceTextList.Add(new Struct_PlotData.Struct_Choice { Title = ClildItem.Value, JumpID = int.Parse(ClildItem.Attribute("JumpId").Value) });

                                }
                                Gal_SelfText.StartTextContent(content, characterInfo.name, () =>
                                {
                                    Gal_Choice.SetActive(true);

                                    Gal_SelfText.transform.Find("element1").SetActive(false);
                                    Gal_SelfText.transform.Find("element2").SetActive(true);

                                    Gal_Choice.CreatNewChoice(PlotData.ChoiceTextList);
                                });
                            }
                            else
                            {
                                Gal_SelfText.StartTextContent(content, characterInfo.name);
                                PlotData.NextJumpID = int.Parse(PlotData.NowPlotDataNode.Attribute("JumpId").Value);
                            }
                        }
                        else
                        {
                            Gal_OtherText.SetActive(true);
                            Gal_OtherText.StartTextContent(content, characterInfo.name);
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

                        AddHistoryContent(characterInfo.characterID, characterInfo.name, content);

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

        private void PlayBgm(string fileName)
        {
            Debug.Log("播放了BGM:" + fileName);
            XAudio.XAudioManager.instance.PlayBgmMusic(fileName);
        }

        private void FixedUpdate ()
        {
            CharacterNum = PlotData.CharacterInfoList.Count;
        }
    }
}