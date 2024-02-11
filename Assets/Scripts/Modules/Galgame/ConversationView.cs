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
        public string Title;
        public string Synopsis;
        public List<XElement> BranchPlot = new();
        public Queue<XElement> BranchPlotInfo = new();
        public Queue<XElement> MainPlot = new();
        public List<XElement> ListMainPlot = new();
        public class Struct_Choice
        {
            public string Title;
            public string JumpID;
        }
        public class Struct_CharacterInfo
        {
            public string CharacterID;
            public GalManager_CharacterLoader CharacterLoader;
            public string Name;
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
        public bool IsBranch = false;
        public string NowJumpID;

    }

    public class ConversationView : XBaseView
    {
        public GalManager_Text Gal_Text;

        public Transform character_parent;

        public GalManager_CharacterLoader character_loader;

        public GalManager_Choice Gal_Choice;

        public GalManager_BackImg Gal_BackImg;

        [SerializeField]
        XButton TouchBack;

        [SerializeField]
        XButton ButtonReturn;

        //string _CharacterInfoText;
        //string _DepartmentText;

        /// <summary>
        /// 当前场景角色数量
        /// </summary>
        [Title("当前场景角色数量")]
        public int CharacterNum;
        private class CharacterConfig
        {
            public static GameConfig CharacterInfo;
        }

        private XDocument PlotxDoc;
        public static Struct_PlotData PlotData = new();
        private void Awake ()
        {
            ResetPlotData();

            //StartCoroutine(LoadCharacterInfo(() =>
            //{
            //    CharacterConfig.CharacterInfo = new GameConfig(_CharacterInfoText);
            //    StartCoroutine(LoadDepartment(() => {
            //        CharacterConfig.Department = new GameConfig(_DepartmentText);
            //        StartCoroutine(LoadPlot());
            //    }));
            //}));

            StartCoroutine(LoadPlot());

            TouchBack.onClick.AddListener(() =>
            {
                Button_Click_NextPlot();
            });

           
        }

        public override void OnEnableView()
        {
            base.OnEnableView();

            ClearGame();

            foreach (var item in PlotData.ListMainPlot)
            {
                PlotData.MainPlot.Enqueue(item);
            }

            XEvent.EventDispatcher.AddEventListener("NEXT_STEP", Button_Click_NextPlot,this);

            //开始游戏
            Button_Click_NextPlot();
        }

        public override void OnDisableView()
        {
            base.OnDisableView();
            XEvent.EventDispatcher.RemoveEventListener("NEXT_STEP", Button_Click_NextPlot, this);

        }

        void ClearGame()
        {
            foreach (var item in PlotData.CharacterInfoList)
            {
                DestroyCharacterByID(item.CharacterID);
            }
            PlotData.MainPlot.Clear();
            //PlotData.BranchPlot.Clear();
            PlotData.BranchPlotInfo.Clear();
            PlotData.IsBranch = false;
        }

//        IEnumerator LoadCharacterInfo(Action action)
//        {
//            string filePath = Path.Combine(AssetDefine.BuildinAssetPath,"HGF/CharacterInfo.ini");

//#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
//            filePath = "file://" + filePath;
//#endif
//            UnityWebRequest www = UnityWebRequest.Get(filePath);
//            yield return www.SendWebRequest();
//            if (www.result == UnityWebRequest.Result.Success)
//            {
//                _CharacterInfoText = www.downloadHandler.text;
//            }
//            else
//            {
//                Debug.Log("Error: " + www.error);
//            }

//            action?.Invoke();
//        }

//        IEnumerator LoadDepartment(Action action)
//        {
//            string filePath = Path.Combine(AssetDefine.BuildinAssetPath, "HGF/Department.ini");

//#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
//            filePath = "file://" + filePath;
//#endif
//            UnityWebRequest www = UnityWebRequest.Get(filePath);
//            yield return www.SendWebRequest();
//            if (www.result == UnityWebRequest.Result.Success)
//            {
//                _DepartmentText = www.downloadHandler.text;
//            }
//            else
//            {
//                Debug.Log("Error: " + www.error);
//            }

//            action?.Invoke();
//        }

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
        public IEnumerator LoadPlot ()
        {
            yield return null;

            string _PlotText = string.Empty;
            //string filePath = Path.Combine(AssetDefine.BuildinAssetPath, "HGF/Test.xml");
            string filePath = Path.Combine(AssetDefine.BuildinAssetPath, "HGF/story.xml");

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
                        case "title":
                            {
                                PlotData.Title = item.Value;
                                break;
                            }
                        case "Synopsis":
                            {
                                PlotData.Synopsis = item.Value;
                                break;
                            }
                        case "BranchPlot":
                            {
                                foreach (var BranchItem in item.Elements())
                                {
                                    PlotData.BranchPlot.Add(BranchItem);
                                }
                                break;
                            }
                        case "MainPlot":
                        {
                            foreach (var MainPlotItem in item.Elements())
                            {
                                    //PlotData.MainPlot.Enqueue(MainPlotItem);
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
            //catch (Exception ex)
            //{
            //    if (ex.Message != "无法识别的根标签")
            //    {

            //        GameAPI.Print(ex.Message, "error");
            //    }
            //}
            GameAPI.Print(Newtonsoft.Json.JsonConvert.SerializeObject(PlotData));
            //Button_Click_NextPlot();

             foreach (var item in PlotData.ListMainPlot)
            {
                PlotData.MainPlot.Enqueue(item);
            }

            //开始游戏
            Button_Click_NextPlot();
        }


        /// <summary>
        /// 点击屏幕 下一句
        /// </summary>
        public void Button_Click_NextPlot ()
        {

            if (PlotData.MainPlot.Count == 0)
            {
                GameAPI.Print("游戏结束!");
                return;
            }

            //IsCanJump这里有问题，如果一直点击会为false，而不是说true，这是因为没有点击按钮 ，没有添加按钮
            if (GalManager_Text.IsSpeak || !GalManager_Text.IsCanJump) { return; }

            if (!PlotData.IsBranch)
            {
                PlotData.MainPlot.TryDequeue(out PlotData.NowPlotDataNode);//队列出队+内联 出一个temp节点
                PlotData.BranchPlotInfo.Clear();
            }
            else//当前为分支节点
            {
                //这块得妥善处理
                PlotData.NowPlotDataNode = GetBranchByID(PlotData.NowJumpID);
            }

            PlotData.ChoiceTextList.Clear();
            if (PlotData.NowPlotDataNode == null)
            {
                GameAPI.Print("无效的剧情结点", "error");
                return;
            }
            switch (PlotData.NowPlotDataNode.Name.ToString())
            {
                case "AddCharacter"://处理添加角色信息的东西
                    {
                        var characterInfo = new Struct_CharacterInfo();
                        var _CharacterId = PlotData.NowPlotDataNode.Attribute("CharacterID").Value;
                        characterInfo.Name = PlotData.NowPlotDataNode.Attribute("CharacterName").Value;
                        string imagePath = PlotData.NowPlotDataNode.Attribute("CharacterImage").Value;

                        characterInfo.CharacterID = _CharacterId;

                        //var _CameObj = Resources.Load<GameObject>("HGF/Img-Character");
                        //_CameObj.GetComponent<Image>().sprite = GameAPI.LoadTextureByIO($"{GameAPI.GetWritePath()}/HGF/Texture2D/Portrait/{CharacterConfig.CharacterInfo.GetValue(_From, "ResourcesPath")}/{CharacterConfig.CharacterInfo.GetValue(_From, "Portrait-Normall")}");
                        characterInfo.CharacterLoader = Instantiate(character_loader, character_parent);
                        characterInfo.CharacterLoader.SetActive(true);

                        //characterInfo.CharacterLoader.SetImage(CharacterConfig.CharacterInfo.GetValue(_From, "Portrait-Normall"));
                        characterInfo.CharacterLoader.SetImage(imagePath);

                        if (PlotData.NowPlotDataNode.Attributes("SendMessage").Count() != 0)
                        {
                            characterInfo.CharacterLoader.Set_Animate_StartOrOutside( PlotData.NowPlotDataNode.Attribute("SendMessage").Value);
                        }

                        PlotData.CharacterInfoList.Add(characterInfo);

                        Button_Click_NextPlot();
                        break;
                    }
                case "Speak":  //处理发言
                    {
                        var characterInfo = GetCharacterObjectByName(PlotData.NowPlotDataNode.Attribute("CharacterID").Value);

                        var imagePathNode = PlotData.NowPlotDataNode.Attribute("CharacterImage");
                        if (imagePathNode != null)
                        {
                            //var _Status = _StatusNode.Value;
                            //var _t = GetCharacterObjectByName(_nodeinfo.CharacterID);
                            characterInfo.CharacterLoader.SetImage(imagePathNode.Value);
                        }

                        if (PlotData.NowPlotDataNode.Elements().Count() != 0) //有选项，因为他有子节点数目了
                        {
                            GalManager_Text.IsCanJump = false;
                            foreach (var ClildItem in PlotData.NowPlotDataNode.Elements())
                            {
                                if (ClildItem.Name.ToString() == "Choice")
                                    PlotData.ChoiceTextList.Add(new Struct_Choice { Title = ClildItem.Value, JumpID = ClildItem.Attribute("JumpID").Value });

                            }
                            Gal_Text.StartTextContent(PlotData.NowPlotDataNode.Attribute("Content").Value, characterInfo.Name, () =>
                            {
                                //foreach (var ClildItem in GalManager.PlotData.ChoiceText)
                                //{
                                //    Gal_Choice.CreatNewChoice(ClildItem.JumpID, ClildItem.Title);
                                //}
                                Gal_Choice.SetActive(true);
                                Gal_Choice.CreatNewChoice(ConversationView.PlotData.ChoiceTextList);
                            });
                        }
                        else Gal_Text.StartTextContent(PlotData.NowPlotDataNode.Attribute("Content").Value, characterInfo.Name);

                        //处理消息
                        if (PlotData.NowPlotDataNode.Attributes("SendMessage").Count() != 0)
                            SendCharMessage(characterInfo.CharacterID, PlotData.NowPlotDataNode.Attribute("SendMessage").Value);
                        if (PlotData.NowPlotDataNode.Attributes("AudioPath").Count() != 0)
                            PlayAudio(PlotData.NowPlotDataNode.Attribute("AudioPath").Value);
                        break;
                    }
                case "ChangeBackImg"://更换背景图片
                    {
                        var _Path = PlotData.NowPlotDataNode.Attribute("Path").Value;

                    
                        Gal_BackImg.SetImage(_Path);
                        Button_Click_NextPlot();
                        break;
                    }
                case "DeleteCharacter":
                    {
                        DestroyCharacterByID(PlotData.NowPlotDataNode.Attribute("CharacterID").Value);
                        break;
                    }
                case "Video":
                    {
                        var _Path = PlotData.NowPlotDataNode.Attribute("Path").Value;

                        XGUIManager.Instance.OpenView("VideoView",UILayer.VideoLayer, Button_Click_NextPlot, _Path);

                        //Gal_Video.SetActive(true);
                        //Gal_Video.Play(_Path);
                        //Gal_Video.onFinish = () =>
                        //{
                        //    Button_Click_NextPlot();
                        //};

                        break;
                    }
                case "ExitGame":
                {
                        ClearGame();
                        break;
                }
            }
            if (PlotData.BranchPlotInfo.Count == 0)
            {
                PlotData.IsBranch = false;
            }
            return;
        }
        public void Button_Click_FastMode ()
        {
            GalManager_Text.IsFastMode = true;
            return;
        }
        public Struct_CharacterInfo GetCharacterObjectByName (string ID)
        {
            return PlotData.CharacterInfoList.Find(t => t.CharacterID == ID);
        }
        public XElement GetBranchByID (string ID)
        {
            if (PlotData.BranchPlotInfo.Count == 0)
                foreach (var item in PlotData.BranchPlot.Find(t => t.Attribute("ID").Value == ID).Elements())
                {
                    PlotData.BranchPlotInfo.Enqueue(item);

                }
            PlotData.BranchPlotInfo.TryDequeue(out XElement t);
            return t;
        }
        /// <summary>
        /// 销毁一个角色
        /// </summary>
        /// <param name="ID"></param>
        public void DestroyCharacterByID (string ID)
        {
            var _ = PlotData.CharacterInfoList.Find(t => t.CharacterID == ID);
            SendCharMessage(ID, "Quit");
            PlotData.CharacterInfoList.Remove(_);
        }
        
        public void SendCharMessage (string CharacterID, string Message)
        {
            var _t = GetCharacterObjectByName(CharacterID);
            _t.CharacterLoader.HandleMessage(Message);
        }

        private void PlayAudio (string fileName)
        {
            ////获取.wav文件，并转成AudioClip
            //GameAPI.Print($"{GameAPI.GetWritePath()}/{fileName}");
            //UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip($"{GameAPI.GetWritePath()}/HGF/Audio/Plot/{fileName}", AudioType.MPEG);
            ////等待转换完成
            //yield return www.SendWebRequest();
            ////获取AudioClip
            //AudioClip audioClip = DownloadHandlerAudioClip.GetContent(www);
            ////设置当前AudioSource组件的AudioClip
            //audioSource.clip = audioClip;
            ////播放声音
            //audioSource.Play();

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