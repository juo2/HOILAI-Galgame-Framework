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


        //判断该节点是否 回到主线
        //有两个节点连接 In
        if (nextNode != null)
        {
            nextNode.Ports.ToList().ForEach(port =>
            {
                if (port.fieldName == "In")
                {
                    if (port.ConnectionCount == 2)
                    {
                        isReture = true;
                    }
                }
            });
        }

        return (isReture, nextNode);
    }

    /// <summary>
    /// 添加角色
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
            addCharacter.SetAttribute("From", "C2");
            addCharacter.SetAttribute("SendMessage", storyAddCharacterNode.Animate.ToString());
            element.AppendChild(addCharacter);
        }
    }

    /// <summary>
    /// 删除角色
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
            deleteCharacter.SetAttribute("SendMessage", storyDelCharacterNode.Animate.ToString());
            element.AppendChild(deleteCharacter);
        }
    }

    public static (string,string) SpeakXml(XmlDocument doc, XmlElement element, Node node)
    {
        string jumpId1 = null;
        string jumpId2 = null;

        if (node is StorySpeakNode)
        {
            StorySpeakNode storySpeakNode = node as StorySpeakNode;

            XmlElement speak = doc.CreateElement("Speak");
            speak.SetAttribute("CharacterID", "1");

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
            }

            element.AppendChild(speak);
        }

        return (jumpId1,jumpId2);
    }

    static void SaveXmlFile(XmlDocument doc, string filePath)
    {
        // 创建XmlWriterSettings来指定编码格式
        XmlWriterSettings settings = new XmlWriterSettings();
        settings.Encoding = new UTF8Encoding(false); // 禁用BOM
        settings.Indent = true;

        // 使用XmlWriter保存XML文件
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
            AddCharacterXml(doc, element, currentNode);
            (string jumpId1,string jumpId2) = SpeakXml(doc, element, currentNode);
            DeleteCharacterXml(doc, element, currentNode);

            if (!string.IsNullOrEmpty(jumpId1))
            {
                Node nextNode1 = null;
                Node nextNode2 = null;
                
                bool _;
                (_, nextNode1) = FindNextNode(currentNode, "outOpt1");
                (_, nextNode2) = FindNextNode(currentNode, "outOpt2");

                //建立分支
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

        StoryGraph storyGraph = AssetDatabase.LoadAssetAtPath<StoryGraph>("Assets/Plugins/xNode/Story/Story Graph.asset");

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
        Debug.Log("XML文件已生成。");
    }

    
}
