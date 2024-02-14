using System.Collections.Generic;
using UnityEngine;
using XGUI;
using static XModules.GalManager.Struct_PlotData;
using static XGUI.XListView;

namespace XModules.GalManager
{
    public class GalManager_Message : MonoBehaviour
    {
        [SerializeField]
        XListView xListView;

        List<Struct_Choice> struct_Choices;

        Dictionary<int, GalComponent_Choice> galComponent_ChoiceDic;

        private void Awake ()
        {
            galComponent_ChoiceDic = new Dictionary<int, GalComponent_Choice>();

            xListView.onCreateRenderer.AddListener(onListCreateRenderer);
            xListView.onUpdateRenderer.AddListener(onListUpdateRenderer);
            //GameObject_Choice = Resources.Load<GameObject>("HGF/Button-Choice");
        }

        void onListCreateRenderer(ListItemRenderer listItem)
        {
            //Debug.Log("GalManager_Choice onListCreateRenderer");

            GalComponent_Choice gl_choice = listItem.gameObject.GetComponent<GalComponent_Choice>();
            galComponent_ChoiceDic[listItem.instanceID] = gl_choice;
        }

        void onListUpdateRenderer(ListItemRenderer listItem)
        {
            //Debug.Log("GalManager_Choice onListUpdateRenderer");

            GalComponent_Choice gl_choice = galComponent_ChoiceDic[listItem.instanceID];
            Struct_Choice choices_data = struct_Choices[listItem.index];

            gl_choice.Init(choices_data.JumpID, choices_data.Title);
        }

        [SerializeField]
        public void CreatNewChoice (List<Struct_Choice> choiceList)
        {
            struct_Choices = choiceList;
            xListView.dataCount = choiceList.Count;
            xListView.ForceRefresh();
            //var _ = GameObject_Choice;
            //_.GetComponent<GalComponent_Choice>().Init(JumpID, Title);
            //Instantiate(_, this.transform);
            //return;
        }

    }
}