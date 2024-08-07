using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cameo;
using Cameo.UI;
using Newtonsoft.Json;
using System.Dynamic;
/// <summary>
/// 這個是高中英文的上稿內容，增加了，地圖圖片，以及書籍連結。
/// </summary>
public class Page_BTNMenuLevel : Page_BTNMenuPage
{
    [System.Serializable]
    public class LancherSelector
    {
        public string LancherName;
        public UI_BTNLancherBase Lancher;
    }
    public List<LancherSelector> LancherSelectors;
    protected UI_BTNLancherBase GetLancher(string LancherName)
    {
        //Debug.Log("判斷 GetLancher:"+LancherName);
        foreach (var item in LancherSelectors)
        {
            if(item.LancherName==LancherName)
            {
                return item.Lancher;
            }
        }
#if UNITY_EDITOR
        Debug.LogError("找不到LancherName:"+LancherName);
#endif
        return null;
    }

    public Image MapUI;
    public List<Button> ExtraUrlBTN;

    [System.Serializable]
    public class ScoreIconCondition
    {
        //達到分數才會會使用此圖案
        public int Score;
        public Sprite Icon;
    }
    // 依據分數高低，替換按鈕的圖案
    [SerializeField]
    List<ScoreIconCondition> ScoreIconConditions;
    ScoreIconCondition FindHestestButUnderScoreIconCondition(int score)
    {
        ScoreIconCondition result = null;
        int max = int.MinValue;
        foreach (var item in ScoreIconConditions)
        {
            if (item.Score > max && item.Score <= score)
            {
                max = item.Score;
                result = item;
            }
        }
        return result;
    }
  
    //依據分數，替換按鈕的圖案
    private void SetupBTNImageWithScore()
    {
        for(int i=0;i< buttons.Count; i++)
        {
            var obj = buttons[i];
            var curBtnState = playerMissionState.GetStateByID(obj.bntID);
          
            if(curBtnState.isLock) {
                continue; //如果是鎖定的，就不用設定圖案了
            }
            
            int score = curBtnState.Score;
            //判斷目前score最接近的scoreIconCOndition是哪一個
          //  Debug.Log("設定ＵＩ by score:" + score+" , "+obj.bntID);
            var condition = FindHestestButUnderScoreIconCondition(score);
            if (condition != null)
            {
                obj.image.sprite = condition.Icon;
            }
            else
            {
                Debug.LogError("沒有設定分數圖案");
            }
        }

        
    }
    
    public class ExtraParamStruct
    {
        public string BookUrl;
        public string MapImageID;
        public string GameLauncher;
    }

    protected void SetupLancher()
    {
        var BTNData = UI_BTNDataManager.Instance.GetBTNData(BTNMenuUniqueID);
         for(int i=0;i< BTNData.Count; i++)
        {
             var eachExtraParam = JsonConvert.DeserializeObject<ExtraParamStruct>(BTNData[i].ExtraParam);
             Debug.Log(BTNData[i].BTNID+","+BTNData[i].Desc+"SetupLancher:"+eachExtraParam.GameLauncher+" , raw data : "+BTNData[i].ExtraParam);
            var lancher = GetLancher(eachExtraParam.GameLauncher);
            if(lancher!=null)
            {
                 buttons[i].btnLuncher = lancher;
            }
        }
    }
    [SerializeField]
    UI_BTNLancherBase _subLevelLauncher;
    public override void OnOpen()
    {
        base.OnOpen();
        for (int i = 0; i < buttons.Count; i++)
        {
            if (UI_BTNDataManager.Instance.GetPreloader("Sublevel_" + buttons[i].bntData.BTNID) != null)
            {
                buttons[i].IsSublevel = true;
                buttons[i].PageID = "SublevelPage";
                buttons[i].button.gameObject.GetComponentInChildren<UI_BTNSublevelProgress>(true).gameObject.SetActive(true);
                buttons[i].btnLuncher = _subLevelLauncher;
            }
        }
    }
    public override void SetupBTNUI()
    {
        foreach (var obj in buttons)
        {
            if (obj.UnLockBtn != null)
            {
                obj.UnLockBtn.onClick.RemoveAllListeners();
                obj.UnLockBtn.onClick.AddListener(() =>
                {
                    if (paymentBox == null)
                    {
                        Debug.LogError("找不到payment box");
                    }
                    else
                    {
                        paymentBox.ShowPaymentBox(() =>
                        {
                            UnlockBTN(obj.bntID);
                            paymentBox.LogPayment(obj.bntID);
                            Debug.Log("付費解鎖成功:" + obj.bntID + "," + obj.bntData.Name);
                        });
                    }
                });
            }

            if (obj.btnLuncher == null) obj.btnLuncher = DefaultLancher;
            obj.button.onClick.RemoveAllListeners();
            obj.button.onClick.AddListener(() =>
            {
                try
                {
                    curScelectedBTN = obj; 
                    OnLancherClick();
            }
                catch (System.Exception ex)
            {
                Debug.LogError("遊戲過程有問題：" + ex.Message + "\n" + ex.StackTrace);
                Debug.Log("應變措施，關閉所有messagebox");
                MessageBoxManager.Instance.CloseAllOpenedBoxWithoutInvokeClosedFunc();
            }

        });
        }
        if (Button_Return != null)
        {
            //            Debug.Log("Button_Return 設定回上一頁");
            Button_Return.onClick.RemoveAllListeners();
            Button_Return.onClick.AddListener(() =>
            {
                //回到上一頁
                pageManager.ToPrev();
            });
            Button_Return.enabled = true;
        }

        SetupBTNImageWithScore();

        //設定書籍連結

        var BTNData = UI_BTNDataManager.Instance.GetBTNData(BTNMenuUniqueID);
        for(int i=0;i< BTNData.Count; i++)
        {
            var curBtnState = playerMissionState.GetStateByID(BTNData[i].BTNID);
          
            if(curBtnState.isLock) {
                ExtraUrlBTN[i].gameObject.SetActive(false);
                continue; //如果是鎖定的，就不用設定圖案了
            }
            if(string.IsNullOrWhiteSpace(BTNData[i].ExtraParam)) {
               ExtraUrlBTN[i].gameObject.SetActive(false);
               continue;
            }
            var eachExtraParam = JsonConvert.DeserializeObject<ExtraParamStruct>(BTNData[i].ExtraParam);
            if(eachExtraParam==null)
            {
                ExtraUrlBTN[i].gameObject.SetActive(false);
                Debug.LogError("ExtraParam is null");
                continue;
            }
            string outUrl =eachExtraParam.BookUrl;// BTNData[i].ExtraParam;
            if (string.IsNullOrWhiteSpace(outUrl))
            {
                ExtraUrlBTN[i].gameObject.SetActive(false);
            }
            else
            {
                ExtraUrlBTN[i].gameObject.SetActive(true);
                ExtraUrlBTN[i].enabled = true;
                ExtraUrlBTN[i].onClick.RemoveAllListeners();
                ExtraUrlBTN[i].onClick.AddListener(() => {
                    Application.OpenURL(outUrl);
                });
            }
        }
        SetupLancher();

    }
    //專為高中英文設計的上稿內容，增加了，地圖圖片，以及書籍連結。
    protected override void SetupWithParam()
    {
        base.SetupWithParam();

        var BTNData = UI_BTNDataManager.Instance.GetBTNData(BTNMenuUniqueID);
        ExtraParamStruct test = new ExtraParamStruct();
       
        var extraParam = JsonConvert.DeserializeObject<ExtraParamStruct>(BTNData[0].ExtraParam);
        //設定地圖圖片
        string MapName = extraParam.MapImageID;
        List<string> ImageAddres = new List<string>();
        ImageAddres.Add(MapName);
        Debug.Log("start load MapName:" + MapName);
        StartCoroutine(ResourceManager.Instance.preloadImagesAssetCoroutine(ImageAddres, () =>
        {
            Debug.Log("complete MapImageID:" + MapName);
            Sprite map = ResourceManager.Instance.LoadAsset<Sprite>(MapName);
            if (map == null)
            {
                Debug.LogError("尚未下載地圖，確認上稿是否有問題："+ MapName);
            }
            else
            {
                MapUI.sprite = map;
            }
           
        }));
       
    }

}
