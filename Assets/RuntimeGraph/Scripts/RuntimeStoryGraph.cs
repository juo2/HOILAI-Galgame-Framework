using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.UI;
using XGUI;
using XModules.Data;
using XModules.Proxy;

namespace XNode.Story
{
	public partial class RuntimeStoryGraph : MonoBehaviour, IPointerClickHandler {
		
		[Header("Graph")]
        public StoryGraph graph;
        [Header("Prefabs")]
		
		public UGUIAddCharacter runtimeAddCharacterPrefab;
		public UGUIBackground runtimeBackgroundPrefab;
		public UGUIBgm runtimeBgmPrefab;
		public UGUIDeleteCharacter runtimeDeleteCharacterPrefab;
		public UGUIExitGame runtimeExitGamePrefab;
		public UGUIMessage runtimeMessagePrefab;
		public UGUIMessageLoop runtimeMessageLoopPrefab;
		public UGUINextChapter runtimeNextChapterPrefab;
		public UGUIReborn runtimeRebornPrefab;
		public UGUISpeak runtimeSpeakPrefab;
		public UGUISpeakAside runtimeSpeakAsidePrefab;
		public UGUIVideo runtimeVideoPrefab;

		public Connection runtimeConnectionPrefab;

		[Header("References")]
		public UGUIContextMenu graphContextMenu;
		public UGUINodeContextMenu nodeContextMenu;
		public UGUITooltip tooltip;

		public ScrollRect scrollRect { get; private set; }
        private List<UGUIBaseNode> nodes = new List<UGUIBaseNode>();
		private List<UGUIBaseNode> nodesPool = new List<UGUIBaseNode>();

		private Dictionary<string, Node> xmlNodeDic = new Dictionary<string, Node>();

		public SaveFileXml exportBtn;
		public OpenFileXml importBtn;
		public InputField nameField;
		public MessageBox messageBox;
		public ConfigChoice configChoice;

		public XListView xListView;
		Dictionary<int, PanelBtnItem> btnItemList = new Dictionary<int, PanelBtnItem>();
		List<string> storyList = new List<string>();

		private void Start() {


			configChoice.SetActive(false);

			xListView.onCreateRenderer.AddListener(onListCreateRenderer);
			xListView.onUpdateRenderer.AddListener(onListUpdateRenderer);

			storyList.Clear();
			ProxyManager.GetStoryList(1, () =>
			{
				ProxyManager.GetStoryList(0, () => {

					foreach(var item in DataManager.getStoryList())
                    {
						storyList.Add(item.title);
					}
					foreach (var item in DataManager.getStoryNoPlayList())
					{
						storyList.Add(item.title);
					}

					xListView.dataCount = storyList.Count;
					xListView.ForceRefresh();
				});
			});
			
			// Create a clone so we don't modify the original asset
			graph = new StoryGraph();
			//graph = graph.Copy() as StoryGraph;
			scrollRect = GetComponentInChildren<ScrollRect>();
			
			graphContextMenu.onClickSpawn -= SpawnNode;
			graphContextMenu.onClickSpawn += SpawnNode;

			exportBtn.preCallBack = () => 
			{
				ExportStory(graph);

				if (string.IsNullOrEmpty(nameField.text)) 
				{
					messageBox.SetContent("剧本名为空");
					return;
				}

				if (s_errorMessage.Count > 0)
                {
					return;
				}

				exportBtn.fileName = nameField.text;

				//如果不是editor需要发送xml给后端
#if !UNITY_EDITOR
				string xmlString = exportBtn.saveData; // 这是你的XML字符串
				StartCoroutine(SyncNpcInfoCoroutine( xmlString));
#endif
			};

			importBtn.finishCallBack = (string xml) => 
			{
				LoadPlot(xml);
			};

		}

		void onListCreateRenderer(XListView.ListItemRenderer listItem)
		{
			PanelBtnItem btnItem = listItem.gameObject.GetComponent<PanelBtnItem>();
			btnItemList[listItem.instanceID] = btnItem;
		}

		void onListUpdateRenderer(XListView.ListItemRenderer listItem)
		{
			PanelBtnItem btnItem = btnItemList[listItem.instanceID];
			btnItem.Refresh(storyList[listItem.index]);
		}

		public void ShowImage(UnityAction<string> action)
        {
#if XConfigMode
			var configImageList = AssetManagement.AssetManager.Instance.GetConfigImages();
			configImageList.Insert(0, string.Empty);
			configChoice.OnShowImage(configImageList, action);
#endif
		}

		public void ShowCharacter(UnityAction<string> action)
        {
			var characterImageList = FindAllCharacter();
			configChoice.OnShowCharacter(characterImageList, action);
		}

		public void ShowAudio(UnityAction<string> action)
		{
#if XConfigMode
			var configImageList = AssetManagement.AssetManager.Instance.GetConfigAudio();
			configImageList.Insert(0, string.Empty);

			configChoice.OnShowAudio(configImageList, action);
#endif

		}

		public void ShowBgm(UnityAction<string> action)
		{
#if XConfigMode

			var configImageList = AssetManagement.AssetManager.Instance.GetConfigBgm();
			configImageList.Insert(0, string.Empty);

			configChoice.OnShowBgm(configImageList, action);
#endif

		}

		public void ShowVideo(UnityAction<string> action)
		{
#if XConfigMode

			var configImageList = AssetManagement.AssetManager.Instance.GetConfigVideo();
			configImageList.Insert(0, string.Empty);

			configChoice.OnShowVideo(configImageList, action);
#endif
		}
		
		IEnumerator SyncNpcInfoCoroutine(string xmlString)
		{
			string url = "http://ai.sorachat.site/chat/npc/syncNpcInfo";

			// 创建一个表单
			WWWForm form = new WWWForm();

			// 将XML字符串转换为字节流
			byte[] xmlBytes = Encoding.UTF8.GetBytes(xmlString);

			// 将字节流添加到表单中，"application/xml" 是MIME类型
			form.AddBinaryData("file", xmlBytes, $"{nameField.text}.xml", "application/xml");

			using (UnityWebRequest request = UnityWebRequest.Post(url, form))
			{
				// 添加 Header 参数

				// 发送请求并等待服务器响应
				yield return request.SendWebRequest();

				if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
				{
					// 打印错误信息
					Debug.LogError(request.error);
				}
				else
				{
					// 处理服务器响应
					Debug.Log("NPC Info synced successfully.");

					messageBox.SetContent("导出剧本成功!!!");

					Debug.Log(request.downloadHandler.text); // 打印服务器返回的信息
				}
			}
		}

		public List<ConfigChoice.CharacterImage> FindAllCharacter()
        {
			List<ConfigChoice.CharacterImage> tupleList = new List<ConfigChoice.CharacterImage>();
			foreach (var node in nodes)
            {
				if (node.node is StoryAddCharacterNode)
                {
					StoryAddCharacterNode addNode = node.node as StoryAddCharacterNode;
					if (!string.IsNullOrEmpty(addNode.ID))
                    {
						ConfigChoice.CharacterImage characterImage = new ConfigChoice.CharacterImage();
						characterImage.ID = addNode.ID;
						characterImage.imageName = addNode.image;

						tupleList.Add(characterImage);
					}
				}
			}
			return tupleList;
		}

		private UGUIBaseNode getNodePrefabInternal<T>()
        {
			foreach(var node in nodesPool)
            {
				if (node is T)
                {
					nodesPool.Remove(node);
					return node;
                }
            }

			return null;
        }

		private UGUIBaseNode getNodePrefab(Node node)
        {
			UGUIBaseNode runtimeNode = null;
			if (node is StoryRebornNode)
            {
				runtimeNode = getNodePrefabInternal<UGUIReborn>();
				if (runtimeNode == null)
					runtimeNode = Instantiate(runtimeRebornPrefab);
			}
			else if (node is StoryAddCharacterNode)
			{
				runtimeNode = getNodePrefabInternal<UGUIAddCharacter>();
				if (runtimeNode == null)
					runtimeNode = Instantiate(runtimeAddCharacterPrefab);
			}
			else if (node is StoryBackgroundNode)
			{
				runtimeNode = getNodePrefabInternal<UGUIBackground>();
				if (runtimeNode == null)
					runtimeNode = Instantiate(runtimeBackgroundPrefab);
			}
			else if (node is StoryBgmNode)
			{
				runtimeNode = getNodePrefabInternal<UGUIBgm>();
				if (runtimeNode == null)
					runtimeNode = Instantiate(runtimeBgmPrefab);
			}
			else if (node is StoryDeleteCharacterNode)
			{
				runtimeNode = getNodePrefabInternal<UGUIDeleteCharacter>();
				if (runtimeNode == null)
					runtimeNode = Instantiate(runtimeDeleteCharacterPrefab);
			}
			else if (node is StoryExitGameNode)
			{
				runtimeNode = getNodePrefabInternal<UGUIExitGame>();
				if (runtimeNode == null)
					runtimeNode = Instantiate(runtimeExitGamePrefab);
			}
			else if (node is StoryMessageNode)
			{
				runtimeNode = getNodePrefabInternal<UGUIMessage>();
				if (runtimeNode == null)
					runtimeNode = Instantiate(runtimeMessagePrefab);
			}
			else if (node is StoryMessageLoopNode)
            {
				runtimeNode = getNodePrefabInternal<UGUIMessageLoop>();
				if (runtimeNode == null)
					runtimeNode = Instantiate(runtimeMessageLoopPrefab);
			}
			else if (node is StoryNextChapterNode)
			{
				runtimeNode = getNodePrefabInternal<UGUINextChapter>();
				if (runtimeNode == null)
					runtimeNode = Instantiate(runtimeNextChapterPrefab);
			}
			else if (node is StorySpeakAsideNode)
			{
				runtimeNode = getNodePrefabInternal<UGUISpeakAside>();
				if (runtimeNode == null)
					runtimeNode = Instantiate(runtimeSpeakAsidePrefab);
			}
			else if (node is StorySpeakNode)
			{
				runtimeNode = getNodePrefabInternal<UGUISpeak>();
				if (runtimeNode == null)
					runtimeNode = Instantiate(runtimeSpeakPrefab);
			}
			else if (node is StoryVideoNode)
			{
				runtimeNode = getNodePrefabInternal<UGUIVideo>();
				if (runtimeNode == null)
					runtimeNode = Instantiate(runtimeVideoPrefab);
			}

			return runtimeNode;
		}

		private void recycleNodePrefab(UGUIBaseNode runtimeNode)
        {
			runtimeNode.OnRecycle();
			runtimeNode.SetActive(false);
			nodes.Remove(runtimeNode);
			nodesPool.Add(runtimeNode);
        }

		public void Refresh() {
			Clear();
			SpawnGraph();
		}

		public void Clear() {
			for (int i = nodes.Count - 1; i >= 0; i--) {
				//Destroy(nodes[i].gameObject);
				recycleNodePrefab(nodes[i]);
			}
			nodes.Clear();
		}

		public void SpawnGraph() {
			if (nodes != null) nodes.Clear();
			else nodes = new List<UGUIBaseNode>();

			for (int i = 0; i < graph.nodes.Count; i++) {
				Node node = graph.nodes[i];
				
				UGUIBaseNode runtimeNode = getNodePrefab(node);
				runtimeNode.SetActive(true);
				runtimeNode.transform.SetParent(scrollRect.content);
				runtimeNode.node = node as StoryBaseNode;
				runtimeNode.graph = this;
				runtimeNode.OnCreate();
				nodes.Add(runtimeNode);
			}
		}

		public UGUIBaseNode GetRuntimeNode(Node node) {
			for (int i = 0; i < nodes.Count; i++) {
				if (nodes[i].node == node) {
					return nodes[i];
				} else { }
			}
			return null;
		}

		public void SpawnNode(Type type, Vector2 position) {
            Node node = graph.AddNode(type);
            node.name = type.Name;
            node.position = position;
            Refresh();
		}

		public void OnPointerClick(PointerEventData eventData) {
			if (eventData.button != PointerEventData.InputButton.Right)
				return;

			graphContextMenu.OpenAt(eventData.position);
		}

		Vector2 stringToVector2(string input)
        {
			Vector2 vector = Vector2.zero;
			// 移除字符串中的括号
			string trimmedInput = input.Trim('(', ')');

			// 分割字符串以获取x和y的字符串表示
			string[] parts = trimmedInput.Split(',');

			if (parts.Length == 2)
			{
				// 尝试将字符串转换为浮点数
				bool parseSuccessX = float.TryParse(parts[0], out float x);
				bool parseSuccessY = float.TryParse(parts[1], out float y);

				if (parseSuccessX && parseSuccessY)
				{
					// 创建Vector2实例
					vector = new Vector2(x, y);

					//Debug.Log("转换成功: " + vector);
				}
				else
				{
					Debug.LogError("字符串转换浮点数失败");
				}
			}
			else
			{
				Debug.LogError("输入字符串格式不正确");
			}

			return vector;
		}

		void XmlToNode(XElement element)
        {
			string NodeId = element.Attribute("NodeId").Value;
			if (element.Name == "Reborn")
			{
				string position = element.Attribute("Position").Value;

				StoryRebornNode node = graph.AddNode<StoryRebornNode>();
				node.name = element.Name.ToString();
				node.position = stringToVector2(position);
				xmlNodeDic[NodeId] = node;
			}
			else if (element.Name == "AddCharacter")
            {
				string ID = element.Attribute("CharacterID").Value;
				string name = element.Attribute("CharacterName").Value;
				string image = element.Attribute("CharacterImage").Value;
				string isSelf = element.Attribute("IsSelf").Value;
				string position = element.Attribute("Position").Value;

				StoryAddCharacterNode node = graph.AddNode<StoryAddCharacterNode>();
				node.name = element.Name.ToString() ;
				node.position = stringToVector2(position);
				node.ID = ID;
				node.p_name = name;
				node.image = image;
				node.isSelf = isSelf == "True";
				xmlNodeDic[NodeId] = node;
			}
			else if (element.Name == "ChangeBackImg")
			{
				string Path = element.Attribute("Path").Value;
				string position = element.Attribute("Position").Value;

				StoryBackgroundNode node = graph.AddNode<StoryBackgroundNode>();
				node.name = element.Name.ToString();
				node.position = stringToVector2(position);
				node.background = Path;
				xmlNodeDic[NodeId] = node;
			}
			else if (element.Name == "DeleteCharacter")
			{
				string ID = element.Attribute("CharacterID").Value;
				string position = element.Attribute("Position").Value;

				StoryDeleteCharacterNode node = graph.AddNode<StoryDeleteCharacterNode>();
				node.name = element.Name.ToString();
				node.position = stringToVector2(position);
				node.ID = ID;
				xmlNodeDic[NodeId] = node;
			}
			else if (element.Name == "DeleteCharacter")
			{
				string ID = element.Attribute("CharacterID").Value;
				string position = element.Attribute("Position").Value;

				StoryDeleteCharacterNode node = graph.AddNode<StoryDeleteCharacterNode>();
				node.name = element.Name.ToString();
				node.position = stringToVector2(position);
				node.ID = ID;
				xmlNodeDic[NodeId] = node;
			}
			else if(element.Name == "ExitGame")
            {
				string position = element.Attribute("Position").Value;

				StoryExitGameNode node = graph.AddNode<StoryExitGameNode>();
				node.name = element.Name.ToString();
				node.position = stringToVector2(position);
				xmlNodeDic[NodeId] = node;
			}
			else if (element.Name == "MessageLoop")
            {
				string position = element.Attribute("Position").Value;
				string image = element.Attribute("CharacterImage")?.Value;
				StoryMessageLoopNode node = graph.AddNode<StoryMessageLoopNode>();
				node.loop = element.Attribute("Loop").Value;
				//node.success = element.Attribute("Success").Value;
				//node.fail = element.Attribute("Fail").Value;
				//node.value = element.Attribute("Value").Value;

				node.name = element.Name.ToString();
				node.position = stringToVector2(position);
				node.image = image;
				xmlNodeDic[NodeId] = node;

			}
			else if (element.Name == "Message")
			{
				string position = element.Attribute("Position").Value;
				string image= element.Attribute("CharacterImage")?.Value;

				StoryMessageNode node = graph.AddNode<StoryMessageNode>();
				node.name = element.Name.ToString();
				node.position = stringToVector2(position);
				node.image = image;

				int times = 1;
				foreach (var ClildItem in element.Elements())
				{
					if (ClildItem.Name.ToString() == "Choice")
                    {
						if(times == 1)
                        {
							node.opt1 = ClildItem.Value;
						}
						else if (times == 2)
						{
							node.opt2 = ClildItem.Value;
						}
						else if (times == 3)
						{
							node.opt3 = ClildItem.Value;
						}
						else if (times == 4)
						{
							node.opt4 = ClildItem.Value;
						}
						times++;
					}
				}
				xmlNodeDic[NodeId] = node;
			}
			else if (element.Name == "NextChapter")
			{
				string position = element.Attribute("Position").Value;

				StoryNextChapterNode node = graph.AddNode<StoryNextChapterNode>();
				node.name = element.Name.ToString();
				node.position = stringToVector2(position);
				xmlNodeDic[NodeId] = node;
			}
			else if (element.Name == "SpeakAside")
            {
				string content = element.Attribute("Content").Value;
				string AudioPath = element.Attribute("AudioPath")?.Value;
				string position = element.Attribute("Position").Value;

				StorySpeakAsideNode node = graph.AddNode<StorySpeakAsideNode>();
				node.name = element.Name.ToString();
				node.position = stringToVector2(position);
				node.content = content;
				node.audio = AudioPath;
				xmlNodeDic[NodeId] = node;
			}
			else if (element.Name == "Video")
			{
				string Path = element.Attribute("Path").Value;
				string position = element.Attribute("Position").Value;

				StoryVideoNode node = graph.AddNode<StoryVideoNode>();
				node.name = element.Name.ToString();
				node.position = stringToVector2(position);
				node.video = Path;
				xmlNodeDic[NodeId] = node;
			}
			else if (element.Name == "Bgm")
			{
				string Path = element.Attribute("Path").Value;
				string position = element.Attribute("Position").Value;

				StoryBgmNode node = graph.AddNode<StoryBgmNode>();
				node.name = element.Name.ToString();
				node.position = stringToVector2(position);
				node.bgm = Path;
				xmlNodeDic[NodeId] = node;
			}
			else if (element.Name == "Speak")
			{
				string ID = element.Attribute("CharacterID").Value;
				string image = element.Attribute("CharacterImage")?.Value;
				string content = element.Attribute("Content").Value;
				string AudioPath = element.Attribute("AudioPath")?.Value;
				string position = element.Attribute("Position").Value;


				StorySpeakNode node = graph.AddNode<StorySpeakNode>();
				node.name = element.Name.ToString();
				node.position = stringToVector2(position);
				node.ID = ID;
				node.image = image;
				node.content = content;
				node.audio = AudioPath;
				int times = 1;

				node.isJump = element.Elements().Count() != 0;

				if (element.Elements().Count() != 0)
                {
					foreach (var ClildItem in element.Elements())
					{
						if (ClildItem.Name.ToString() == "Choice")
						{
							if (times == 1)
							{
								node.opt1 = ClildItem.Value;
							}
							else if (times == 2)
							{
								node.opt2 = ClildItem.Value;
							}
							else if (times == 3)
							{
								node.opt3 = ClildItem.Value;
							}
							else if (times == 4)
							{
								node.opt4 = ClildItem.Value;
							}
							times++;
						}
					}
				}
				xmlNodeDic[NodeId] = node;
			}

		}

		public void LoadPlot(string _PlotText)
		{
			//			yield return null;

			//			string _PlotText = string.Empty;
			//			//string filePath = Path.Combine(AssetDefine.BuildinAssetPath, "HGF/Test.xml");
			//			string filePath = Path.Combine(Application.streamingAssetsPath, "A_AssetBundles/HGF/buqun1.xml");

			//#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
			//            filePath = "file://" + filePath;
			//#endif
			//			//            if (Application.platform == RuntimePlatform.Android)
			//			//            {
			//			//                filePath = "jar:file://" + Application.dataPath + "!/assets/HGF/Test.xml";
			//			//            }

			//			UnityWebRequest www = UnityWebRequest.Get(filePath);
			//			yield return www.SendWebRequest();

			//			if (www.result == UnityWebRequest.Result.Success)
			//			{
			//				_PlotText = www.downloadHandler.text;
			//			}
			//			else
			//			{
			//				Debug.Log("Error: " + www.error);
			//			}
			//try
			
			graph = new StoryGraph();

			{
				//GameAPI.Print($"游戏剧本：{_PlotText}");
				var PlotxDoc = XDocument.Parse(_PlotText);

				//-----开始读取数据

				foreach (var item in PlotxDoc.Root.Elements())
				{
					switch (item.Name.ToString())
					{
						case "Plot":
							{
								foreach (var element in item.Elements())
								{
									XmlToNode(element);
								}

								foreach (var element in item.Elements())
                                {
									if ((element.Name == "Speak" && element.Elements().Count() != 0) || element.Name == "Message")
									{
										string NodeId = element.Attribute("NodeId")?.Value;
										int times = 1;
										foreach (var ClildItem in element.Elements())
										{
											string JumpId = ClildItem.Attribute("JumpId")?.Value;
											Node fromNode = xmlNodeDic[NodeId];
											Node toNode = xmlNodeDic[JumpId];

											NodePort Out = fromNode.GetPort($"outOpt{times}");
											NodePort In = toNode.GetPort("In");

											Out.Connect(In);

											times++;
										}
									}
									else
									{
										string NodeId = element.Attribute("NodeId")?.Value;
										string JumpId = element.Attribute("JumpId")?.Value;

										if (!string.IsNullOrEmpty(JumpId))
										{
											Node fromNode = xmlNodeDic[NodeId];
											Node toNode = xmlNodeDic[JumpId];

											NodePort Out = fromNode.GetPort("Out");
											NodePort In = toNode.GetPort("In");
											Out.Connect(In);
										}
									}
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

			Clear();
			SpawnGraph();
		}
	}
}