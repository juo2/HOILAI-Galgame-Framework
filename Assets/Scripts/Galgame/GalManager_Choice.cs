using System.Collections.Generic;
using UnityEngine;
using XGUI;
using static ScenesScripts.GalPlot.GalManager.Struct_PlotData;
using static XGUI.XListView;

namespace ScenesScripts.GalPlot
{
    public class GalManager_Choice : MonoBehaviour
    {
        XListView xListView;

        List<Struct_Choice> struct_Choices;

        List<GalComponent_Choice> galComponent_Choices = new List<GalComponent_Choice>();

        private void Awake ()
        {
            xListView = GetComponent<XListView>();
            xListView.onCreateRenderer.AddListener(onListCreateRenderer);
            xListView.onUpdateRenderer.AddListener(onListUpdateRenderer);
            //GameObject_Choice = Resources.Load<GameObject>("HGF/Button-Choice");
        }

        void onListCreateRenderer(ListItemRenderer listItem)
        {
            Debug.Log("GalManager_Choice onListCreateRenderer");

            GalComponent_Choice gl_choice = listItem.gameObject.GetComponent<GalComponent_Choice>();
            galComponent_Choices.Add(gl_choice);
        }

        void onListUpdateRenderer(ListItemRenderer listItem)
        {
            Debug.Log("GalManager_Choice onListUpdateRenderer");

            GalComponent_Choice gl_choice = galComponent_Choices[listItem.index];
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

        public void Button_Click_Choice ()
        {
            xListView.dataCount = 0;
            xListView.ForceRefresh();
        }
    }
}