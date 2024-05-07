using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using UnityEngine;
using UnityEngine.EventSystems;

namespace XNode.Story
{
    public partial class RuntimeStoryGraph : MonoBehaviour, IPointerClickHandler
    {
        Dictionary<int, StoryEditorNode> s_storyEditorNodeDic = new Dictionary<int, StoryEditorNode>();
        Node s_startNode;
        int s_index = 1;
        List<string> s_errorMessage = new List<string>();

        public bool ContainsChinese(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return false;
            }
            return Regex.IsMatch(input, @"[\u4e00-\u9fff]");
        }

        public class StoryEditorNode
        {
            public StoryBaseNode baseNode;
            public int index;
        }

        void ClearAllStoryGraphic()
        {
            s_storyEditorNodeDic.Clear();
            s_errorMessage.Clear();
            s_index = 1;
        }


        void ErrorMessage(StoryBaseNode storyBase, string errorMessage)
        {
            if(storyBase != null)
            {
                storyBase.isError = true;
                storyBase.errorMessage = errorMessage;
            }
         
            Debug.LogError(errorMessage);

            s_errorMessage.Add(errorMessage);
        }

        public Node FindStartNode(StoryGraph storyGraph)
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

                        if (count > 1)
                        {
                            ErrorMessage(startNode as StoryBaseNode, "have two enter point!!!!");
                            startNode = null;
                        }
                    }
                });
            }

            return startNode;
        }

        public void RebornXml(XmlDocument doc, XmlElement element, StoryEditorNode s_node)
        {
            if (s_node.baseNode is StoryRebornNode)
            {
                StoryRebornNode storyReborn = s_node.baseNode as StoryRebornNode;

                XmlElement nextxml = doc.CreateElement("Reborn");
                nextxml.SetAttribute("NodeId", s_node.index.ToString());
                nextxml.SetAttribute("Position", storyReborn.position.ToString());

                //if (ContainsChinese(storyNextChapter.storyGraphicName))
                //{
                //    ErrorMessage(storyNextChapter, $"nextchapter node :{storyNextChapter.GetInstanceID()} storyGraphicName is chinese");
                //}

                findNextNodeXml(nextxml, s_node.baseNode);

                element.AppendChild(nextxml);
            }
        }

        public void AddCharacterXml(XmlDocument doc, XmlElement element, StoryEditorNode s_node)
        {
            if (s_node.baseNode is StoryAddCharacterNode)
            {
                StoryAddCharacterNode storyAddCharacterNode = s_node.baseNode as StoryAddCharacterNode;

                XmlElement addCharacter = doc.CreateElement("AddCharacter");

                addCharacter.SetAttribute("NodeId", s_node.index.ToString());
                addCharacter.SetAttribute("CharacterID", storyAddCharacterNode.ID);
                addCharacter.SetAttribute("CharacterName", storyAddCharacterNode.p_name);

                if (ContainsChinese(storyAddCharacterNode.image))
                {
                    ErrorMessage(storyAddCharacterNode, $"addCharacter node :{storyAddCharacterNode.GetInstanceID()} png is chinese");
                }

                addCharacter.SetAttribute("CharacterImage", storyAddCharacterNode.image);
                addCharacter.SetAttribute("IsSelf", storyAddCharacterNode.isSelf.ToString());
                addCharacter.SetAttribute("Position", storyAddCharacterNode.position.ToString());

                findNextNodeXml(addCharacter, s_node.baseNode);

                element.AppendChild(addCharacter);

            }
        }


        public void VideoXml(XmlDocument doc, XmlElement element, StoryEditorNode s_node)
        {
            if (s_node.baseNode is StoryVideoNode)
            {
                StoryVideoNode storyVideoNode = s_node.baseNode as StoryVideoNode;

                XmlElement videoxml = doc.CreateElement("Video");
                videoxml.SetAttribute("NodeId", s_node.index.ToString());
                videoxml.SetAttribute("Position", storyVideoNode.position.ToString());

                if (ContainsChinese(storyVideoNode.video))
                {
                    ErrorMessage(storyVideoNode, $"video node :{storyVideoNode.GetInstanceID()} video is chinese");
                }
                videoxml.SetAttribute("Path", storyVideoNode.video);

                findNextNodeXml(videoxml, s_node.baseNode);

                element.AppendChild(videoxml);
            }
        }

        public void NextChapterXml(XmlDocument doc, XmlElement element, StoryEditorNode s_node)
        {
            if (s_node.baseNode is StoryNextChapterNode)
            {
                StoryNextChapterNode storyNextChapter = s_node.baseNode as StoryNextChapterNode;

                XmlElement nextxml = doc.CreateElement("NextChapter");
                nextxml.SetAttribute("NodeId", s_node.index.ToString());
                nextxml.SetAttribute("Position", storyNextChapter.position.ToString());

                //if (ContainsChinese(storyNextChapter.storyGraphicName))
                //{
                //    ErrorMessage(storyNextChapter, $"nextchapter node :{storyNextChapter.GetInstanceID()} storyGraphicName is chinese");
                //}

                findNextNodeXml(nextxml, s_node.baseNode);

                element.AppendChild(nextxml);
            }
        }

        public void BackGroundXml(XmlDocument doc, XmlElement element, StoryEditorNode s_node)
        {
            if (s_node.baseNode is StoryBackgroundNode)
            {
                StoryBackgroundNode storyBackgroundNode = s_node.baseNode as StoryBackgroundNode;

                XmlElement backxml = doc.CreateElement("ChangeBackImg");

                backxml.SetAttribute("NodeId", s_node.index.ToString());
                backxml.SetAttribute("Position", storyBackgroundNode.position.ToString());

                if (ContainsChinese(storyBackgroundNode.background))
                {
                    ErrorMessage(storyBackgroundNode, $"background node :{storyBackgroundNode.GetInstanceID()} png is chinese");
                }

                backxml.SetAttribute("Path", storyBackgroundNode.background);

                findNextNodeXml(backxml, s_node.baseNode);

                element.AppendChild(backxml);
            }
        }

        public void DeleteCharacterXml(XmlDocument doc, XmlElement element, StoryEditorNode s_node)
        {
            if (s_node.baseNode is StoryDeleteCharacterNode)
            {
                StoryDeleteCharacterNode storyDelCharacterNode = s_node.baseNode as StoryDeleteCharacterNode;

                XmlElement deleteCharacter = doc.CreateElement("DeleteCharacter");

                deleteCharacter.SetAttribute("NodeId", s_node.index.ToString());
                deleteCharacter.SetAttribute("CharacterID", storyDelCharacterNode.ID);
                deleteCharacter.SetAttribute("Position", storyDelCharacterNode.position.ToString());

                findNextNodeXml(deleteCharacter, s_node.baseNode);

                element.AppendChild(deleteCharacter);
            }
        }

        public void SpeakAsideXml(XmlDocument doc, XmlElement element, StoryEditorNode s_node)
        {
            if (s_node.baseNode is StorySpeakAsideNode)
            {
                StorySpeakAsideNode storySpeakAsideNode = s_node.baseNode as StorySpeakAsideNode;

                XmlElement speadxml = doc.CreateElement("SpeakAside");
                speadxml.SetAttribute("NodeId", s_node.index.ToString());
                speadxml.SetAttribute("Content", storySpeakAsideNode.content);
                speadxml.SetAttribute("Position", storySpeakAsideNode.position.ToString());


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

        public void MessageLoopXml(XmlDocument doc, XmlElement element, StoryEditorNode s_node)
        {
            if (s_node.baseNode is StoryMessageLoopNode)
            {
                StoryMessageLoopNode storyMessageNode = s_node.baseNode as StoryMessageLoopNode;

                bool isConnectSelf = findCharacterNode(storyMessageNode, true);
                bool isConnectNpc = findCharacterNode(storyMessageNode, false);

                if (!isConnectSelf || !isConnectNpc)
                {
                    ErrorMessage(storyMessageNode, $"node : {storyMessageNode.GetInstanceID()} should has both Self and Npc");
                }

                XmlElement speadxml = doc.CreateElement("MessageLoop");
                speadxml.SetAttribute("NodeId", s_node.index.ToString());
                speadxml.SetAttribute("Position", storyMessageNode.position.ToString());

                if (!string.IsNullOrEmpty(storyMessageNode.image))
                {
                    speadxml.SetAttribute("CharacterImage", storyMessageNode.image);
                }


                speadxml.SetAttribute("Loop", storyMessageNode.loop);
                //speadxml.SetAttribute("Success", storyMessageNode.success);
                //speadxml.SetAttribute("Fail", storyMessageNode.fail);
                //speadxml.SetAttribute("Value", storyMessageNode.value);
                
                findNextNodeXml(speadxml, s_node.baseNode);

                element.AppendChild(speadxml);
            }
        }

        public void MessageXml(XmlDocument doc, XmlElement element, StoryEditorNode s_node)
        {
            if (s_node.baseNode is StoryMessageNode)
            {
                StoryMessageNode storyMessageNode = s_node.baseNode as StoryMessageNode;

                bool isConnectSelf = findCharacterNode(storyMessageNode, true);
                bool isConnectNpc = findCharacterNode(storyMessageNode, false);

                if(!isConnectSelf || !isConnectNpc)
                {
                    ErrorMessage(storyMessageNode, $"node : {storyMessageNode.GetInstanceID()} should has both Self and Npc");
                }

                XmlElement speadxml = doc.CreateElement("Message");
                speadxml.SetAttribute("NodeId", s_node.index.ToString());
                speadxml.SetAttribute("Position", storyMessageNode.position.ToString());

                if (!string.IsNullOrEmpty(storyMessageNode.image))
                {
                    speadxml.SetAttribute("CharacterImage", storyMessageNode.image);
                }

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

        public bool CheckHasCharacterID(string characterID)
        {
            foreach (var item in s_storyEditorNodeDic.Values)
            {
                if (item.baseNode is StoryAddCharacterNode)
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

        public void SpeakXml(XmlDocument doc, XmlElement element, StoryEditorNode s_node)
        {

            if (s_node.baseNode is StorySpeakNode)
            {
                StorySpeakNode storySpeakNode = s_node.baseNode as StorySpeakNode;

                bool isConnectNode = findCharacterNodeByID(s_node.baseNode, storySpeakNode.ID);

                XmlElement speak = doc.CreateElement("Speak");

                speak.SetAttribute("NodeId", s_node.index.ToString());
                speak.SetAttribute("Position", storySpeakNode.position.ToString());

                if (!CheckHasCharacterID(storySpeakNode.ID))
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

                    var addNode = GetAddCharacterNode(storySpeakNode.ID);

                    if (!isConnectNode || !addNode.isSelf)
                    {
                        ErrorMessage(storySpeakNode, $"node : {storySpeakNode.GetInstanceID()} require addCharacterNode isSelf==True");
                    }

                }
                else
                {
                    findNextNodeXml(speak, s_node.baseNode);

                    if(!isConnectNode)
                    {
                        ErrorMessage(storySpeakNode, $"node : {storySpeakNode.GetInstanceID()} require addCharacterNode");
                    }
                }

                element.AppendChild(speak);
            }

        }

        public void ExitGameXml(XmlDocument doc, XmlElement element, StoryEditorNode s_node)
        {
            if (s_node.baseNode is StoryExitGameNode)
            {
                StoryExitGameNode storyExitGameNode = s_node.baseNode as StoryExitGameNode;

                XmlElement videoxml = doc.CreateElement("ExitGame");
                videoxml.SetAttribute("Position", storyExitGameNode.position.ToString());
                videoxml.SetAttribute("NodeId", s_node.index.ToString());
                element.AppendChild(videoxml);
            }
        }

        public void BgmXml(XmlDocument doc, XmlElement element, StoryEditorNode s_node)
        {
            if (s_node.baseNode is StoryBgmNode)
            {
                StoryBgmNode storyBgmNode = s_node.baseNode as StoryBgmNode;

                XmlElement bgmxml = doc.CreateElement("Bgm");
                bgmxml.SetAttribute("NodeId", s_node.index.ToString());
                bgmxml.SetAttribute("Position", storyBgmNode.position.ToString());

                if (ContainsChinese(storyBgmNode.bgm))
                {
                    ErrorMessage(storyBgmNode, $"bgm node :{storyBgmNode.GetInstanceID()} bgm is chinese");
                }
                bgmxml.SetAttribute("Path", storyBgmNode.bgm);

                findNextNodeXml(bgmxml, s_node.baseNode);

                element.AppendChild(bgmxml);
            }
        }


        void SaveXmlFile(XmlDocument doc, string filePath)
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Encoding = new UTF8Encoding(false);
            settings.Indent = true;

            using (XmlWriter writer = XmlWriter.Create(filePath, settings))
            {
                doc.Save(writer);
            }
        }

        public class Utf8StringWriter : StringWriter
        {
            public override Encoding Encoding => new UTF8Encoding(false); //
        }

        public string SaveXmlToString(XmlDocument doc)
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Encoding = new UTF8Encoding(false); //
            settings.Indent = true;

            using (Utf8StringWriter stringWriter = new Utf8StringWriter())
            using (XmlWriter xmlWriter = XmlWriter.Create(stringWriter, settings))
            {
                doc.Save(xmlWriter);
                return stringWriter.ToString();
            }
        }

        void findNextNodeXml(XmlElement element, Node node, string nodeName = "Out")
        {
            int resIndex = findNextNode(node, nodeName);

            if (resIndex != -1)
                element.SetAttribute("JumpId", resIndex.ToString());
            else
            {
                ErrorMessage(node as StoryBaseNode, $"node : {node.GetInstanceID()} can not find nextnode");
            }

        }

        StoryAddCharacterNode GetAddCharacterNode(string ID)
        {
            foreach (var node in s_storyEditorNodeDic.Values)
            {
                if(node.baseNode is StoryAddCharacterNode)
                {
                    StoryAddCharacterNode addNode = node.baseNode as StoryAddCharacterNode;
                    if (addNode.ID == ID)
                    {
                        return addNode;
                    }
                }
            }

            return null;
        }

        bool findCharacterNodeByID(Node currentNode, string ID)
        {
            List<NodePort> preNodePorts = currentNode.GetPort("In").GetConnections();

            if (preNodePorts.Count <= 0)
            {
                return false;
            }

            foreach(var node in preNodePorts)
            {
                if (node.node is StoryAddCharacterNode)
                {
                    StoryAddCharacterNode storyAddCharacter = node.node as StoryAddCharacterNode;
                    if (storyAddCharacter.ID == ID)
                    {
                        return true;
                    }
                    else
                    {
                        return findCharacterNodeByID(node.node, ID);
                    }
                }
                else
                {
                    return findCharacterNodeByID(node.node, ID);
                }
            }

            return false;
        }

        bool findCharacterNode(Node currentNode, bool isSelf)
        {
            List<NodePort> preNodePorts = currentNode.GetPort("In").GetConnections();

            if (preNodePorts.Count <= 0)
            {
                return false;
            }

            foreach (var node in preNodePorts)
            {
                if (node.node is StoryAddCharacterNode)
                {
                    StoryAddCharacterNode storyAddCharacter = node.node as StoryAddCharacterNode;
                    if (storyAddCharacter.isSelf == isSelf)
                    {
                        return true;
                    }
                    else
                    {
                        return findCharacterNode(node.node, isSelf);
                    }
                }
                else
                {
                    return findCharacterNode(node.node, isSelf);
                }
            }

            return false;
        }

        int findNextNode(Node node, string nodeName = "Out")
        {
            Node nextNode = null;

            int resIndex = -1;

            node.Ports.ToList().ForEach(port =>
            {
                //Debug.Log($"port.fieldName:{port.fieldName}");
                if (port.fieldName == nodeName)
                {
                    if (port.ConnectionCount > 1)
                    {
                        resIndex = -2;
                    }
                    else if (port.Connection != null)
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

        void buildNode(StoryGraph storyGraph)
        {
            foreach (var node in storyGraph.nodes)
            {
                if (s_startNode.GetInstanceID() != node.GetInstanceID())
                {
                    addNode(node);
                }
            }
        }

        void addNode(Node node)
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

        XmlDocument buildNodeXml()
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
                RebornXml(doc, element, item);
                AddCharacterXml(doc, element, item);
                SpeakAsideXml(doc, element, item);
                SpeakXml(doc, element, item);
                MessageXml(doc, element, item);
                MessageLoopXml(doc, element, item);
                DeleteCharacterXml(doc, element, item);
                VideoXml(doc, element, item);
                BackGroundXml(doc, element, item);
                NextChapterXml(doc, element, item);
                ExitGameXml(doc, element, item);
                BgmXml(doc, element, item);
            }

            return doc;
        }

        public void ExportStoryNode(StoryGraph storyGraph)
        {
            Node startNode = FindStartNode(storyGraph);
            if (startNode == null)
            {
                ErrorMessage(null, "can not find startNode");
                Debug.LogError("generate fail!!!");
                return;
            }

            s_startNode = startNode;
            addNode(s_startNode);

            buildNode(storyGraph);
        }

        public void ExportStoryTool(StoryGraph storyGraph)
        {
            Debug.Log("Custom Tool Clicked!");

            ExportStoryNode(storyGraph);

            XmlDocument doc = buildNodeXml();

            exportBtn.saveData = SaveXmlToString(doc);
            //SaveXmlFile(doc, $"Assets/StreamingAssets/A_AssetBundles/HGF/buqun1.xml");

            Debug.Log("XML generate finish !!!!");
        }

        private void ExportStory(StoryGraph storyGraph)
        {
            ClearAllStoryGraphic();

            ExportStoryTool(storyGraph);

            if (s_errorMessage.Count > 0)
                messageBox.SetContent(s_errorMessage[s_errorMessage.Count - 1]);
        }
    }
}
