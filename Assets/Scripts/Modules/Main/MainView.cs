using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XGUI;

namespace XModules.Main
{
    public class MainView : XBaseView
    {
        [SerializeField]
        XButton homeBtn;

        [SerializeField]
        XButton dialogueBtn;

        [SerializeField]
        XButton profileBtn;

        [SerializeField]
        XLoader homeLoader;

        [SerializeField]
        XLoader dialogueLoader;

        [SerializeField]
        XLoader profileLoader;

        XLoader.LoaderItemRenderer homeRenderer = null;
        XLoader.LoaderItemRenderer dialogueRenderer = null;
        XLoader.LoaderItemRenderer profileRenderer = null;


        void setDisableAll()
        {
            if(homeRenderer != null)
            {
                homeRenderer.gameObject.SetActive(false);
            }
            if (dialogueRenderer != null)
            {
                dialogueRenderer.gameObject.SetActive(false);
            }
            if (profileRenderer != null)
            {
                profileRenderer.gameObject.SetActive(false);
            }
        }


        // Start is called before the first frame update
        void Start()
        {
            homeLoader.onCreateRenderer.AddListener((XLoader.LoaderItemRenderer loaderItem) =>
            {
                homeRenderer = loaderItem;
            });

            dialogueLoader.onCreateRenderer.AddListener((XLoader.LoaderItemRenderer loaderItem) =>
            {
                dialogueRenderer = loaderItem;
            });

            profileLoader.onCreateRenderer.AddListener((XLoader.LoaderItemRenderer loaderItem) =>
            {
                profileRenderer = loaderItem;
            });

            homeBtn.onClick.AddListener(() => {
                setDisableAll();
                if (homeRenderer == null)
                {
                    homeLoader.StartLoad();
                }
                else
                {
                    homeRenderer.gameObject.SetActive(true);
                }
            });

            dialogueBtn.onClick.AddListener(() => {

                setDisableAll();
                if (dialogueRenderer == null)
                {
                    dialogueLoader.StartLoad();
                }
                else
                {
                    dialogueRenderer.gameObject.SetActive(true);
                }
            });

            profileBtn.onClick.AddListener(() => {

                setDisableAll();
                if (profileRenderer == null)
                {
                    profileLoader.StartLoad();
                }
                else
                {
                    profileRenderer.gameObject.SetActive(true);
                }
            });

            homeLoader.StartLoad();
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}



