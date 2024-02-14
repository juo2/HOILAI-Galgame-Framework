using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using UnityEditor;
using UnityEngine;
using XNode;
using XNode.Story;

public class ExportStory : Editor
{
    static XmlElement root;

    static int jumpInt = 0;

    public static Node FindStartNode(StoryGraph storyGraph)
    {
        Node startNode = null;

        foreach (var node in storyGraph.nodes)
        {
            //NodePort nodePort = node.Ports[0];
            node.Ports.ToList().ForEach(port =>
            {
                //Debug.Log($"port.fieldName:{port.fieldName}");
                if (port.fieldName == "In" && port.Connection == null)
                {
                    startNode = node;
                }
            });
        }

        return startNode;
    }

    public static (bool, Node) FindNextNode(Node node,string nodeName = "Out")
    {
        bool isReture = false;

        Node nextNode = null;

        node.Ports.ToList().ForEach(port =>
        {
            //Debug.Log($"port.fieldName:{port.fieldName}");
            if (port.fieldName == nodeName)
            {
                if (port.Connection != null)
                    nextNode = port.Connection.node;
            }
        });


        //�жϸýڵ��Ƿ� �ص�����
        //�������ڵ����� In
        if (nextNode != null)
        {
            nextNode.Ports.ToList().ForEach(port =>
            {
                if (port.fieldName == "In")
                {
                    if (port.ConnectionCount == 3)
                    {
                        isReture = true;
                    }
                }
            });
        }

        return (isReture, nextNode);
    }

    /// <summary>
    /// ��ӽ�ɫ
    /// </summary>
    /// <param name="doc"></param>
    /// <param name="element"></param>
    /// <param name="node"></param>
    public static void AddCharacterXml(XmlDocument doc, XmlElement element,Node node)
    {
        if (node is StoryAddCharacterNode)
        {
            StoryAddCharacterNode storyAddCharacterNode = node as StoryAddCharacterNode;

            XmlElement addCharacter = doc.CreateElement("AddCharacter");

            addCharacter.SetAttribute("CharacterID", storyAddCharacterNode.ID);
            addCharacter.SetAttribute("CharacterName", storyAddCharacterNode.p_name);
            addCharacter.SetAttribute("CharacterImage", storyAddCharacterNode.image);
            addCharacter.SetAttribute("IsSelf", storyAddCharacterNode.isSelf.ToString());
            addCharacter.SetAttribute("SendMessage", storyAddCharacterNode.animate.ToString());
            element.AppendChild(addCharacter);
        }
    }

    /// <summary>
    /// ��ӽ�ɫ
    /// </summary>
    /// <param name="doc"></param>
    /// <param name="element"></param>
    /// <param name="node"></param>
    public static void VideoXml(XmlDocument doc, XmlElement element, Node node)
    {
        if (node is StoryVideoNode)
        {
            StoryVideoNode storyVideoNode = node as StoryVideoNode;

            XmlElement videoxml = doc.CreateElement("Video");

            videoxml.SetAttribute("Path", storyVideoNode.video);

            element.AppendChild(videoxml);
        }
    }

    public static void BackGroundXml(XmlDocument doc, XmlElement element, Node node)
    {
        if (node is StoryBackgroundNode)
        {
            StoryBackgroundNode storyBackgroundNode = node as StoryBackgroundNode;

            XmlElement backxml = doc.CreateElement("ChangeBackImg");

            backxml.SetAttribute("Path", storyBackgroundNode.background);

            element.AppendChild(backxml);
        }
    }

    /// <summary>
    /// ɾ����ɫ
    /// </summary>
    /// <param name="doc"></param>
    /// <param name="element"></param>
    /// <param name="node"></param>
    public static void DeleteCharacterXml(XmlDocument doc, XmlElement element, Node node)
    {
        if (node is StoryDeleteCharacterNode)
        {
            StoryDeleteCharacterNode storyDelCharacterNode = node as StoryDeleteCharacterNode;

            XmlElement deleteCharacter = doc.CreateElement("DeleteCharacter");

            deleteCharacter.SetAttribute("CharacterID", storyDelCharacterNode.ID);
            deleteCharacter.SetAttribute("SendMessage", storyDelCharacterNode.animate.ToString());
            element.AppendChild(deleteCharacter);
        }
    }

    public static void SpeakAsideXml(XmlDocument doc, XmlElement element, Node node)
    {
        if (node is StorySpeakAsideNode)
        {
            StorySpeakAsideNode storySpeakAsideNode = node as StorySpeakAsideNode;

            XmlElement speadxml = doc.CreateElement("SpeakAside");

            speadxml.SetAttribute("Content", storySpeakAsideNode.content);
            if (!string.IsNullOrEmpty(storySpeakAsideNode.audio))
            {
                speadxml.SetAttribute("AudioPath", storySpeakAsideNode.audio);
            }
            element.AppendChild(speadxml);
        }
    }

    public static (string, string, string) MessageXml(XmlDocument doc, XmlElement element, Node node)
    {
        string jumpId1 = null;
        string jumpId2 = null;
        string jumpId3 = null;

        if (node is StoryMessageNode)
        {
            StoryMessageNode storyMessageNode = node as StoryMessageNode;

            XmlElement speadxml = doc.CreateElement("Message");

            jumpId1 = $"J{jumpInt}";

            XmlElement choice1 = doc.CreateElement("Choice");
            choice1.SetAttribute("JumpID", jumpId1);
            choice1.InnerText = storyMessageNode.opt1;
            speadxml.AppendChild(choice1);

            jumpInt++;

            jumpId2 = $"J{jumpInt}";

            XmlElement choice2 = doc.CreateElement("Choice");
            choice2.SetAttribute("JumpID", jumpId2);
            choice2.InnerText = storyMessageNode.opt2;
            speadxml.AppendChild(choice2);

            jumpInt++;

            jumpId3 = $"J{jumpInt}";

            XmlElement choice3 = doc.CreateElement("Choice");
            choice3.SetAttribute("JumpID", jumpId3);
            choice3.InnerText = storyMessageNode.opt3;
            speadxml.AppendChild(choice3);

            jumpInt++;

            element.AppendChild(speadxml);
        }

        return (jumpId1, jumpId2, jumpId3);
    }

    public static (string,string,string) SpeakXml(XmlDocument doc, XmlElement element, Node node)
    {
        string jumpId1 = null;
        string jumpId2 = null;
        string jumpId3 = null;

        if (node is StorySpeakNode)
        {
            StorySpeakNode storySpeakNode = node as StorySpeakNode;

            XmlElement speak = doc.CreateElement("Speak");
            speak.SetAttribute("CharacterID", storySpeakNode.ID);

            if(!string.IsNullOrEmpty(storySpeakNode.image))
            {
                speak.SetAttribute("CharacterImage", storySpeakNode.image);
            }

            if(!string.IsNullOrEmpty(storySpeakNode.audio))
            {
                speak.SetAttribute("AudioPath", storySpeakNode.audio);
            }
            if (storySpeakNode.animate != Animate_type.None)
            {
                speak.SetAttribute("SendMessage", storySpeakNode.animate.ToString());
            }

            speak.SetAttribute("Content",storySpeakNode.content);

            if (storySpeakNode.isJump)
            {
                jumpId1 = $"J{jumpInt}";

                XmlElement choice1 = doc.CreateElement("Choice");
                choice1.SetAttribute("JumpID", jumpId1);
                choice1.InnerText = storySpeakNode.opt1;
                speak.AppendChild(choice1);

                jumpInt++;

                jumpId2 = $"J{jumpInt}";

                XmlElement choice2 = doc.CreateElement("Choice");
                choice2.SetAttribute("JumpID", jumpId2);
                choice2.InnerText = storySpeakNode.opt2;
                speak.AppendChild(choice2);

                jumpInt++;

                jumpId3 = $"J{jumpInt}";

                XmlElement choice3 = doc.CreateElement("Choice");
                choice3.SetAttribute("JumpID", jumpId3);
                choice3.InnerText = storySpeakNode.opt3;
                speak.AppendChild(choice3);

                jumpInt++;
            }

            element.AppendChild(speak);
        }

        return (jumpId1,jumpId2,jumpId3);
    }

    static void SaveXmlFile(XmlDocument doc, string filePath)
    {
        // ����XmlWriterSettings��ָ�������ʽ
        XmlWriterSettings settings = new XmlWriterSettings();
        settings.Encoding = new UTF8Encoding(false); // ����BOM
        settings.Indent = true;

        // ʹ��XmlWriter����XML�ļ�
        using (XmlWriter writer = XmlWriter.Create(filePath, settings))
        {
            doc.Save(writer);
        }
    }

    static (bool,Node) buildNodeXml(XmlDocument doc, XmlElement element, Node currentNode)
    {

        Node nextNode = null;
        bool isReture = false;

        while (currentNode != null && isReture == false)
        {
            string jumpId1 = null;
            string jumpId2 = null;
            string jumpId3 = null;

            AddCharacterXml(doc, element, currentNode);
            SpeakAsideXml(doc, element, currentNode);
            (jumpId1,jumpId2,jumpId3) = SpeakXml(doc, element, currentNode);

            if (string.IsNullOrEmpty(jumpId1))
                (jumpId1,jumpId2,jumpId3) = MessageXml(doc, element, currentNode);

            DeleteCharacterXml(doc, element, currentNode);
            VideoXml(doc, element, currentNode);
            BackGroundXml(doc, element, currentNode);

            if (!string.IsNullOrEmpty(jumpId1))
            {
                Node nextNode1 = null;
                Node nextNode2 = null;
                Node nextNode3 = null;
                bool _;
                (_, nextNode1) = FindNextNode(currentNode, "outOpt1");
                (_, nextNode2) = FindNextNode(currentNode, "outOpt2");
                (_, nextNode3) = FindNextNode(currentNode, "outOpt3");

                //������֧
                XmlElement branchPlot = doc.CreateElement("BranchPlot");
                root.AppendChild(branchPlot);

                XmlElement branchPlotNode1 = doc.CreateElement("BranchPlotNode");
                branchPlotNode1.SetAttribute("ID", jumpId1);
                branchPlot.AppendChild(branchPlotNode1);
                
                (_, nextNode) = buildNodeXml(doc, branchPlotNode1, nextNode1);

                XmlElement branchPlotNode2 = doc.CreateElement("BranchPlotNode");
                branchPlotNode2.SetAttribute("ID", jumpId2);
                branchPlot.AppendChild(branchPlotNode2);

                (_, nextNode) = buildNodeXml(doc, branchPlotNode2, nextNode2);

                XmlElement branchPlotNode3 = doc.CreateElement("BranchPlotNode");
                branchPlotNode3.SetAttribute("ID", jumpId3);
                branchPlot.AppendChild(branchPlotNode3);

                (_, nextNode) = buildNodeXml(doc, branchPlotNode3, nextNode3);
            }
            else
            {
                (isReture, nextNode) = FindNextNode(currentNode);
            }

            currentNode = nextNode;
        }

        return (isReture, currentNode);
    }

    [MenuItem("Tools/ExportStoryTool")]
    public static void ExportStoryTool()
    {

        Debug.Log("Custom Tool Clicked!");

        StoryGraph storyGraph = AssetDatabase.LoadAssetAtPath<StoryGraph>("Assets/Plugins/xNode/Story/New Story Graph.asset");

        Node startNode = FindStartNode(storyGraph);

        XmlDocument doc = new XmlDocument();

        XmlDeclaration xmlDeclaration = doc.CreateXmlDeclaration("1.0", "utf-8", null);
        doc.AppendChild(xmlDeclaration);

        root = doc.CreateElement("data");
        doc.AppendChild(root);

        XmlElement mainPlot = doc.CreateElement("MainPlot");
        root.AppendChild(mainPlot);

        buildNodeXml(doc, mainPlot, startNode);

        SaveXmlFile(doc,"Assets/StreamingAssets/A_AssetBundles/HGF/story.xml");
        Debug.Log("XML�ļ������ɡ�");
    }

    
}
