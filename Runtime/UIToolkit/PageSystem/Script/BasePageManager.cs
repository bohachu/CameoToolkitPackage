using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.Events;

namespace Cameo.UI
{
    public class BasePageManager : MonoBehaviour
    {
        [InlineEditor, SerializeField]
        protected PageSystemConfig Config;

        protected bool IsConfigNull
        {
            get
            {
                return Config == null;
            }
        }
        #if UNITY_EDITOR
        [ShowIf("IsConfigNull"), Button("Create Config")]
        protected void createConfig()
        {
            Config = ScriptableObjectUtility.CreateAsset<PageSystemConfig>();
        }
        #endif
        public string ID;

        public UnityEvent OnSwitchStart;

        public UnityEvent OnSwitchEnd;

        [HideIf("IsConfigNull"), ValueDropdown("PageIDs"), SerializeField]
        protected string startPageID;

        private ValueDropdownList<string> PageIDs
        {
            get
            {
                ValueDropdownList<string> pageIDs = new ValueDropdownList<string>();

                if (Config != null)
                {
                    for (int i = 0; i < Config.Pages.Count; ++i)
                        pageIDs.Add(Config.Pages[i].ID);
                }

                return pageIDs;
            }
        }

        private bool IsStartPageValid
        {
            get
            {
                return !string.IsNullOrEmpty(startPageID);
            }
        }

        [ShowIf("IsStartPageValid"), Button("Edit Start Page")]
        private void instanciateStartPage()
        {
            GameObject page = Instantiate(Config.PageMapping[startPageID].Prefab.gameObject, pageRootTrans);
        }

        [SerializeField]
        protected Transform pageRootTrans;

        [SerializeField]
        protected float animationWaitTime = 0.2f;

        protected string currentPageID;

        private Dictionary<string, object> currentPageParamMapping;

        private BasePage currentPage = null;

        private string nextPageID;

        private Dictionary<string, object> nextPageParamMapping;

        private BasePage nextPage = null;
        
        private bool isSkipAnimation;

        private bool isEnableSwitchPage;

        private bool isToPrev;

        public bool IsEnableSwitchPage
        {
            get
            {
                return isEnableSwitchPage;
            }
        }


        public virtual void Initialize(Dictionary<string, object> paramMapping = null, string startPageID = null)
        {
            isEnableSwitchPage = false;

            if (currentPage != null)
            {
                Destroy(currentPage.gameObject);
                currentPageID = null;
                currentPage = null;
            }

            string curStartPage = (string.IsNullOrEmpty(startPageID)) ? this.startPageID : startPageID;

            if(Config.PageMapping.ContainsKey(curStartPage))
            {
                createNextPage(curStartPage, true, paramMapping);
            }

            isSkipAnimation = true;
            isToPrev = false;
//            Debug.Log("switchPageCoroutine");
            StartCoroutine("switchPageCoroutine");
        }
        public virtual void SwitchTo(string nextPageID, bool isSkipAnimation = false, Dictionary<string, object> paramMapping = null)
        {
            //Debug.LogFormat("Switch to {0}", nextPageID);

            if (isEnableSwitchPage)
            {
               
                if (!Config.PageMapping.ContainsKey(nextPageID))
                {
                    Debug.LogFormat("page: [{0}] is not exist.", nextPageID);
                    return;
                }

                isEnableSwitchPage = false;
                Debug.Log("SwitchTo "+ nextPageID);
                createNextPage(nextPageID, false, paramMapping);

                this.isSkipAnimation = isSkipAnimation;
                isToPrev = false;

                StartCoroutine("switchPageCoroutine");
            }
            else
            {
                Debug.Log("page is now switching.");
            }
        }

        public virtual void ToPrev()
        {
            if(isEnableSwitchPage)
            {
                while (PageHistoryManager.Count(ID) > 0)
                {
                    PageSwitchRecord switchRecord = PageHistoryManager.Pop(ID);

                    //由於定義了頁面深度小於0則不會計入頁面置換紀錄，這種頁面無論如何都可以到上一頁
                    if (Config.PageMapping[switchRecord.PageID].Depth < Config.PageMapping[currentPageID].Depth || Config.PageMapping[currentPageID].Depth < 0)
                    {
                        isEnableSwitchPage = false;

                        createNextPage(switchRecord.PageID, false, switchRecord.ParamMapping);
                        isSkipAnimation = false;
                        isToPrev = true;

                        StartCoroutine("switchPageCoroutine");

                        break;
                    }
                }
            }
        }

        protected virtual void sendLogBeforeSwitchPage(string fromPageID, string toPageID)
        {

        }

        public BasePage createNextPage(string nextPageID, bool setAppearCompleted, Dictionary<string, object> paramMapping)
        {
            sendLogBeforeSwitchPage(this.nextPageID, nextPageID);

            this.nextPageID = nextPageID;
            this.nextPageParamMapping = paramMapping;

            GameObject nextPageObj = Instantiate(Config.PageMapping[nextPageID].Prefab.gameObject, pageRootTrans);
            nextPage = nextPageObj.GetComponent<BasePage>();

            if (Config.PageMapping[nextPageID].UseDevelopParam)
            {
                BasePageParam pageParam = nextPageObj.GetComponent<BasePageParam>();
                paramMapping = (pageParam == null) ? null : pageParam.GetParam();
            }
            nextPage.Initialize(this, setAppearCompleted, paramMapping);

            if (currentPage != null)
            {
                if (Config.PageMapping[nextPageID].Depth >= Config.PageMapping[currentPageID].Depth)
                {
                    nextPageObj.transform.SetAsLastSibling();
                }
                else
                {
                    nextPageObj.transform.SetAsFirstSibling();
                }
            }
            return nextPage;
        }

        protected virtual IEnumerator switchPageCoroutine()
        {

            OnSwitchStart.Invoke();
            
            float openTime = (isSkipAnimation || (currentPage != null && (Config.PageMapping[nextPageID].Depth < Config.PageMapping[currentPageID].Depth))) ? 0 : animationWaitTime;
            float closeTime = (isSkipAnimation || (currentPage != null && (Config.PageMapping[nextPageID].Depth >= Config.PageMapping[currentPageID].Depth))) ? 0 : animationWaitTime;

            if (currentPage != null)
            {
                if (!isToPrev && !string.IsNullOrEmpty(currentPageID) && Config.PageMapping[currentPageID].Depth >= 0)
                {
                    while (PageHistoryManager.Count(ID) > 0 &&
                           Config.PageMapping[PageHistoryManager.Peek(ID).PageID].Depth >= Config.PageMapping[currentPageID].Depth)
                    {
                        PageSwitchRecord switchRecord = PageHistoryManager.Pop(ID);
                        //Debug.Log("Pop: " + switchRecord.PageID);
                    }
                    PageHistoryManager.Push(ID, new PageSwitchRecord(currentPageID, currentPageParamMapping));
                    //Debug.Log("Push: " + currentPageID);
                }

                currentPage.OnClose();
                currentPage.Close((isSkipAnimation) ? 0 : animationWaitTime);
                yield return new WaitForSeconds(closeTime);
                currentPage.OnClosed();
                Destroy(currentPage.gameObject);

            }
            
            currentPage = nextPage;
            currentPageID = nextPageID;
            currentPageParamMapping = nextPageParamMapping;

            currentPage.OnOpen();
            currentPage.Open((isSkipAnimation) ? 0 : animationWaitTime);
            yield return new WaitForSeconds(openTime);
            currentPage.OnOpened();

            isEnableSwitchPage = true;

            OnSwitchEnd.Invoke();
        }
    }
}

