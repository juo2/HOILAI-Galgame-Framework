using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using XNode;
using XNode.Story;
using System.Xml;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

public class ExportStoryFolder : EditorWindow
{
    private string path = "Assets/Plugins/xNode/Story/StoryGraph";
    private List<string> folders = new List<string>();
    private int selectedIndex = 0;

    static Dictionary<int, StoryEditorNode> s_storyEditorNodeDic = new Dictionary<int, StoryEditorNode>();

    static string s_storyGraphicPath;

    static Node s_startNode;

    static int s_index = 1;

    static HashSet<string> s_alreadyLoadGraphicSet = new HashSet<string>();

    static Dictionary<string, Node> s_startNodeDic = new Dictionary<string, Node>();

    static List<string> s_errorMessage = new List<string>();

    // 检测字符串是否包含中文
    public static bool ContainsChinese(string input)
    {
        return Regex.IsMatch(input, @"[\u4e00-\u9fff]");
    }

    public class StoryEditorNode
    {
        public StoryBaseNode baseNode;
        public int index;
    }

    [MenuItem("Tools/ExportStoryTool")]
    public static void ShowWindow()
    {
        GetWindow<ExportStoryFolder>("Export Story Folder");
    }

    private void OnEnable()
    {
        // 清除旧的文件夹列表
        folders.Clear();
       

        // 获取指定路径下所有的文件夹
        var directories = Directory.GetDirectories(path, "*", SearchOption.TopDirectoryOnly);
        foreach (var directory in directories)
        {
            folders.Add(directory.Remove(0, directory.IndexOf(path) + path.Length + 1));
        }
    }

    static void ClearAllStoryGraphic()
    {
        s_storyEditorNodeDic.Clear();
        s_alreadyLoadGraphicSet.Clear();
        s_startNodeDic.Clear();
        s_errorMessage.Clear();
        s_index = 1;
    }


    static void ErrorMessage(StoryBaseNode storyBase,string errorMessage)
    {
        storyBase.isError = true;
        Debug.LogError(errorMessage);

        s_errorMessage.Add(errorMessage);
    }

    public static Node FindStartNode(StoryGraph storyGraph)
    {
        Node startNode = null;

        int count = 0;

        foreach (var node in storyGraph.nodes)
        {
            //NodePort nodePort = node.Ports[0];
            node.Ports.ToList().ForEach(port =>
            {
                //Debug.Log($"port.fieldName:{port.fieldName}");
                if (port.fieldName == "In" && port.Connection == null)
                {
                    startNode = node;
                    count++;

                    if(count > 1)
                    {
                        ErrorMessage(startNode as StoryBaseNode, "have two enter point!!!!");
                        startNode = null;
                    }
                }
            });
        }

        return startNode;
    }

    public static void AddCharacterXml(XmlDocument doc, XmlElement element, StoryEditorNode s_node)
    {
        if (s_node.baseNode is StoryAddCharacterNode)
        {
            StoryAddCharacterNode storyAddCharacterNode = s_node.baseNode as StoryAddCharacterNode;

            XmlElement addCharacter = doc.CreateElement("AddCharacter");

            addCharacter.SetAttribute("NodeId", s_node.index.ToString());
            addCharacter.SetAttribute("CharacterID", storyAddCharacterNode.ID);
            addCharacter.SetAttribute("CharacterName", storyAddCharacterNode.p_name);

            if( ContainsChinese(storyAddCharacterNode.image))
            {
                ErrorMessage(storyAddCharacterNode, $"addCharacter node :{storyAddCharacterNode.GetInstanceID()} png is chinese");
            }

            addCharacter.SetAttribute("CharacterImage", storyAddCharacterNode.image);
            addCharacter.SetAttribute("IsSelf", storyAddCharacterNode.isSelf.ToString());

            findNextNodeXml(addCharacter, s_node.baseNode);

            element.AppendChild(addCharacter);

        }
    }


    public static void VideoXml(XmlDocument doc, XmlElement element, StoryEditorNode s_node)
    {
        if (s_node.baseNode is StoryVideoNode)
        {
            StoryVideoNode storyVideoNode = s_node.baseNode as StoryVideoNode;

            XmlElement videoxml = doc.CreateElement("Video");
            videoxml.SetAttribute("NodeId", s_node.index.ToString());

            if (ContainsChinese(storyVideoNode.video))
            {
                ErrorMessage(storyVideoNode, $"video node :{storyVideoNode.GetInstanceID()} video is chinese");
            }
            videoxml.SetAttribute("Path", storyVideoNode.video);

            findNextNodeXml(videoxml, s_node.baseNode);

            element.AppendChild(videoxml);
        }
    }

    public static void NextChapterXml(XmlDocument doc, XmlElement element, StoryEditorNode s_node)
    {
        if (s_node.baseNode is StoryNextChapterNode)
        {
            StoryNextChapterNode storyNextChapter = s_node.baseNode as StoryNextChapterNode;

            XmlElement nextxml = doc.CreateElement("NextChapter");
            nextxml.SetAttribute("NodeId", s_node.index.ToString());

            if (ContainsChinese(storyNextChapter.storyGraphicName))
            {
                ErrorMessage(storyNextChapter, $"nextchapter node :{storyNextChapter.GetInstanceID()} storyGraphicName is chinese");
            }

            if (s_startNodeDic.ContainsKey(storyNextChapter.storyGraphicName))
            {
                Node nextNode = s_startNodeDic[storyNextChapter.storyGraphicName];
                StoryEditorNode s_nextNode = s_storyEditorNodeDic[nextNode.GetInstanceID()];
                nextxml.SetAttribute("JumpId", s_nextNode.index.ToString());
            }
            else
            {
                ErrorMessage(storyNextChapter, $"instanceId:{storyNextChapter.GetInstanceID()} has not next chapter");
            }

            element.AppendChild(nextxml);
        }
    }

    public static void BackGroundXml(XmlDocument doc, XmlElement element, StoryEditorNode s_node)
    {
        if (s_node.baseNode is StoryBackgroundNode)
        {
            StoryBackgroundNode storyBackgroundNode = s_node.baseNode as StoryBackgroundNode;

            XmlElement backxml = doc.CreateElement("ChangeBackImg");

            backxml.SetAttribute("NodeId", s_node.index.ToString());

            if (ContainsChinese(storyBackgroundNode.background))
            {
                ErrorMessage(storyBackgroundNode, $"background node :{storyBackgroundNode.GetInstanceID()} png is chinese");
            }

            backxml.SetAttribute("Path", storyBackgroundNode.background);

            findNextNodeXml(backxml, s_node.baseNode);

            element.AppendChild(backxml);
        }
    }

    public static void DeleteCharacterXml(XmlDocument doc, XmlElement element, StoryEditorNode s_node)
    {
        if (s_node.baseNode is StoryDeleteCharacterNode)
        {
            StoryDeleteCharacterNode storyDelCharacterNode = s_node.baseNode as StoryDeleteCharacterNode;

            XmlElement deleteCharacter = doc.CreateElement("DeleteCharacter");

            deleteCharacter.SetAttribute("NodeId", s_node.index.ToString());
            deleteCharacter.SetAttribute("CharacterID", storyDelCharacterNode.ID);

            findNextNodeXml(deleteCharacter, s_node.baseNode);

            element.AppendChild(deleteCharacter);
        }
    }

    public static void SpeakAsideXml(XmlDocument doc, XmlElement element, StoryEditorNode s_node)
    {
        if (s_node.baseNode is StorySpeakAsideNode)
        {
            StorySpeakAsideNode storySpeakAsideNode = s_node.baseNode as StorySpeakAsideNode;

            XmlElement speadxml = doc.CreateElement("SpeakAside");
            speadxml.SetAttribute("NodeId", s_node.index.ToString());
            speadxml.SetAttribute("Content", storySpeakAsideNode.content);
            if (!string.IsNullOrEmpty(storySpeakAsideNode.audio))
            {
                if (ContainsChinese(storySpeakAsideNode.audio))
                {
                    ErrorMessage(storySpeakAsideNode, $"speak aside node :{storySpeakAsideNode.GetInstanceID()} audio is chinese");
                }
                speadxml.SetAttribute("AudioPath", storySpeakAsideNode.audio);
            }

            findNextNodeXml(speadxml, s_node.baseNode);

            element.AppendChild(speadxml);
        }
    }

    public static void MessageXml(XmlDocument doc, XmlElement element, StoryEditorNode s_node)
    {
        if (s_node.baseNode is StoryMessageNode)
        {
            StoryMessageNode storyMessageNode = s_node.baseNode as StoryMessageNode;

            XmlElement speadxml = doc.CreateElement("Message");
            speadxml.SetAttribute("NodeId", s_node.index.ToString());

            if (!string.IsNullOrEmpty(storyMessageNode.opt1))
            {
                XmlElement choice1 = doc.CreateElement("Choice");

                findNextNodeXml(choice1, s_node.baseNode, "outOpt1");

                choice1.InnerText = storyMessageNode.opt1;
                speadxml.AppendChild(choice1);
            }


            if (!string.IsNullOrEmpty(storyMessageNode.opt2))
            {
                XmlElement choice2 = doc.CreateElement("Choice");

                findNextNodeXml(choice2, s_node.baseNode, "outOpt2");

                choice2.InnerText = storyMessageNode.opt2;
                speadxml.AppendChild(choice2);

            }

            if (!string.IsNullOrEmpty(storyMessageNode.opt3))
            {
                XmlElement choice3 = doc.CreateElement("Choice");

                findNextNodeXml(choice3, s_node.baseNode, "outOpt3");

                choice3.InnerText = storyMessageNode.opt3;
                speadxml.AppendChild(choice3);
            }

            if (!string.IsNullOrEmpty(storyMessageNode.opt4))
            {

                XmlElement choice4 = doc.CreateElement("Choice");

                findNextNodeXml(choice4, s_node.baseNode, "outOpt4");

                choice4.InnerText = storyMessageNode.opt4;
                speadxml.AppendChild(choice4);
            }

            element.AppendChild(speadxml);
        }
    }

    public static bool CheckHasCharacterID(string characterID)
    {
        foreach (var item in s_storyEditorNodeDic.Values)
        {
            if(item.baseNode is StoryAddCharacterNode)
            {
                StoryAddCharacterNode addNode = item.baseNode as StoryAddCharacterNode;
                if (addNode.ID == characterID)
                {
                    return true;
                }
            }
        }

        return false;
    }

    public static void SpeakXml(XmlDocument doc, XmlElement element, StoryEditorNode s_node)
    {

        if (s_node.baseNode is StorySpeakNode)
        {
            StorySpeakNode storySpeakNode = s_node.baseNode as StorySpeakNode;

            XmlElement speak = doc.CreateElement("Speak");

            speak.SetAttribute("NodeId", s_node.index.ToString());

            if(!CheckHasCharacterID(storySpeakNode.ID))
            {
                ErrorMessage(storySpeakNode, $"speak node :{storySpeakNode.GetInstanceID()} has not CharacterID:{storySpeakNode.ID}");
            }

            speak.SetAttribute("CharacterID", storySpeakNode.ID);

            if (!string.IsNullOrEmpty(storySpeakNode.image))
            {
                if (ContainsChinese(storySpeakNode.audio))
                {
                    ErrorMessage(storySpeakNode, $"speak node :{storySpeakNode.GetInstanceID()} png is chinese");
                }

                speak.SetAttribute("CharacterImage", storySpeakNode.image);
            }

            if (!string.IsNullOrEmpty(storySpeakNode.audio))
            {
                speak.SetAttribute("AudioPath", storySpeakNode.audio);
            }

            speak.SetAttribute("Content", storySpeakNode.content);

            if (storySpeakNode.isJump)
            {
                if (!string.IsNullOrEmpty(storySpeakNode.opt1))
                {
                    XmlElement choice1 = doc.CreateElement("Choice");
                    findNextNodeXml(choice1, s_node.baseNode, "outOpt1");
                    choice1.InnerText = storySpeakNode.opt1;
                    speak.AppendChild(choice1);
                }

                if (!string.IsNullOrEmpty(storySpeakNode.opt2))
                {
                    XmlElement choice2 = doc.CreateElement("Choice");

                    findNextNodeXml(choice2, s_node.baseNode, "outOpt2");

                    choice2.InnerText = storySpeakNode.opt2;
                    speak.AppendChild(choice2);
                }

                if (!string.IsNullOrEmpty(storySpeakNode.opt3))
                {
                    XmlElement choice3 = doc.CreateElement("Choice");

                    findNextNodeXml(choice3, s_node.baseNode, "outOpt3");

                    choice3.InnerText = storySpeakNode.opt3;
                    speak.AppendChild(choice3);
                }

                if (!string.IsNullOrEmpty(storySpeakNode.opt4))
                {
                    XmlElement choice4 = doc.CreateElement("Choice");

                    findNextNodeXml(choice4, s_node.baseNode, "outOpt4");

                    choice4.InnerText = storySpeakNode.opt4;
                    speak.AppendChild(choice4);
                }
            }
            else
            {
                findNextNodeXml(speak, s_node.baseNode);
            }

            element.AppendChild(speak);
        }

    }

    public static void ExitGameXml(XmlDocument doc, XmlElement element, StoryEditorNode s_node)
    {
        if (s_node.baseNode is StoryExitGameNode)
        {
            StoryExitGameNode storyExitGameNode = s_node.baseNode as StoryExitGameNode;

            XmlElement videoxml = doc.CreateElement("ExitGame");
            videoxml.SetAttribute("NodeId", s_node.index.ToString());
            element.AppendChild(videoxml);
        }
    }

    public static void BgmXml(XmlDocument doc, XmlElement element, StoryEditorNode s_node)
    {
        if (s_node.baseNode is StoryBgmNode)
        {
            StoryBgmNode storyBgmNode = s_node.baseNode as StoryBgmNode;

            XmlElement bgmxml = doc.CreateElement("Bgm");
            bgmxml.SetAttribute("NodeId", s_node.index.ToString());

            if (ContainsChinese(storyBgmNode.bgm))
            {
                ErrorMessage(storyBgmNode, $"bgm node :{storyBgmNode.GetInstanceID()} bgm is chinese");
            }
            bgmxml.SetAttribute("Path", storyBgmNode.bgm);

            findNextNodeXml(bgmxml, s_node.baseNode);

            element.AppendChild(bgmxml);
        }
    }


    static void SaveXmlFile(XmlDocument doc, string filePath)
    {
        XmlWriterSettings settings = new XmlWriterSettings();
        settings.Encoding = new UTF8Encoding(false);
        settings.Indent = true;

        using (XmlWriter writer = XmlWriter.Create(filePath, settings))
        {
            doc.Save(writer);
        }
    }

    static void findNextNodeXml(XmlElement element,Node node, string nodeName = "Out")
    {
        int resIndex = findNextNode(node,nodeName);

        if (resIndex != -1)
            element.SetAttribute("JumpId", resIndex.ToString());
        else
        {
            ErrorMessage(node as StoryBaseNode, $"node : {node.GetInstanceID()} can not find nextnode");
        }

    }

    static int findNextNode(Node node, string nodeName = "Out")
    {
        Node nextNode = null;

        int resIndex = -1;

        node.Ports.ToList().ForEach(port =>
        {
            //Debug.Log($"port.fieldName:{port.fieldName}");
            if (port.fieldName == nodeName)
            {
                if(port.ConnectionCount > 1)
                {
                    resIndex = -2;
                }
                else if(port.Connection != null)
                {
                    nextNode = port.Connection.node;

                    if (s_storyEditorNodeDic.TryGetValue(nextNode.GetInstanceID(), out StoryEditorNode storyEditorNode))
                    {
                        resIndex = storyEditorNode.index;
                    }
                }
            }
        });

        if (resIndex == -2)
        {
            StoryBaseNode storyBase = node as StoryBaseNode;
            ErrorMessage(storyBase, $"instanceId:{node.GetInstanceID()} has two outNode");
        }

        return resIndex;
    }

    static void buildNode(StoryGraph storyGraph)
    {
        List<string> needToLoadChapterList = new List<string>();

        foreach (var node in storyGraph.nodes)
        {
            if (s_startNode.GetInstanceID() != node.GetInstanceID())
            {
                addNode(node);
            }

            if (node is StoryNextChapterNode)
            {
                StoryNextChapterNode nextChapter = node as StoryNextChapterNode;
                needToLoadChapterList.Add(nextChapter.storyGraphicName);
            }
        }

        foreach(var chapterName in needToLoadChapterList)
        {
            string storyGraphicPath = s_storyGraphicPath + $"/{chapterName}.asset";
            ExportStoryNode(storyGraphicPath);
        }
    }

    static void addNode(Node node)
    {
        StoryEditorNode storyEditorNode = new StoryEditorNode();
        storyEditorNode.baseNode = node as StoryBaseNode;
        storyEditorNode.baseNode.isError = false;
        storyEditorNode.index = s_index;
        s_index++;

        int instanceId = node.GetInstanceID();

        if (!s_storyEditorNodeDic.ContainsKey(instanceId))
        {
            s_storyEditorNodeDic[instanceId] = storyEditorNode;
        }
        else
        {
            Debug.LogError($"instanceId:{instanceId} is exit");
        }
    }

    static XmlDocument buildNodeXml()
    {
        XmlDocument doc = new XmlDocument();

        XmlDeclaration xmlDeclaration = doc.CreateXmlDeclaration("1.0", "utf-8", null);
        doc.AppendChild(xmlDeclaration);

        XmlElement root = doc.CreateElement("data");
        doc.AppendChild(root);

        XmlElement element = doc.CreateElement("Plot");
        root.AppendChild(element);

        foreach (var item in s_storyEditorNodeDic.Values)
        {
            AddCharacterXml(doc, element, item);
            SpeakAsideXml(doc, element, item);
            SpeakXml(doc, element, item);
            MessageXml(doc, element, item);
            DeleteCharacterXml(doc, element, item);
            VideoXml(doc, element, item);
            BackGroundXml(doc, element, item);
            NextChapterXml(doc, element, item);
            ExitGameXml(doc, element, item);
            BgmXml(doc, element, item);
        }

        

        return doc;
    }

    public static void ExportStoryNode(string storyPath,bool addFirst = false)
    {
        if (s_alreadyLoadGraphicSet.Contains(storyPath))
        {
            Debug.Log($"already load {storyPath}");
            return;
        }

        s_alreadyLoadGraphicSet.Add(storyPath);

        StoryGraph storyGraph = AssetDatabase.LoadAssetAtPath<StoryGraph>(storyPath);

        Node startNode = FindStartNode(storyGraph);
        if (startNode == null)
        {
            Debug.LogError("generate fail!!!");
            return;
        }

        string pathName = Path.GetFileNameWithoutExtension(storyPath);

        if (!s_startNodeDic.ContainsKey(pathName))
        {
            s_startNodeDic[pathName] = startNode;
        }
        else
        {
            Debug.LogError("s_startNodeDic is error！！！！");
        }

        if (addFirst)
        {
            s_startNode = startNode;
            addNode(s_startNode);
        }

        buildNode(storyGraph);
    }

    public static void ExportStoryTool(string storyPath,string name)
    {
        Debug.Log("Custom Tool Clicked!");

        ExportStoryNode(storyPath,true);

        XmlDocument doc  = buildNodeXml();

        SaveXmlFile(doc, $"Assets/StreamingAssets/A_AssetBundles/HGF/{name}.xml");

        Debug.Log("XML generate finish !!!!");
    }

    private void OnGUI()
    {
        GUILayout.Label("Select a Folder", EditorStyles.boldLabel);

        // 如果没有找到文件夹，显示提示信息
        if (folders.Count == 0)
        {
            GUILayout.Label("No folders found in the specified path.");
        }
        else
        {
            // 创建一个下拉菜单供用户选择文件夹
            selectedIndex = EditorGUILayout.Popup("Folders", selectedIndex, folders.ToArray());

            // 创建一个按钮，当点击时输出选中的文件夹
            if (GUILayout.Button("Export"))
            {
                ClearAllStoryGraphic();
               
                s_storyGraphicPath = $"{path}/{ folders[selectedIndex]}";

                ExportStoryTool($"{s_storyGraphicPath}/Enter.asset", folders[selectedIndex]);

                if(s_errorMessage.Count > 0)
                    EditorUtility.DisplayDialog("Error Title", "Error See Unity Console For Detail", "OK");
            }
        }
    }
}