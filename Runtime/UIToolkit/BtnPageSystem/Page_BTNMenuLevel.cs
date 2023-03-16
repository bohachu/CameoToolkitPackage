using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cameo;
using Cameo.UI;
using Newtonsoft.Json;
/// <summary>
/// 這個是高中英文的上稿內容，增加了，地圖圖片，以及書籍連結。
/// </summary>
public class Page_BTNMenuLevel : Page_BTNMenuPage
{
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
            Debug.Log("設定ＵＩ by score:" + score+" , "+obj.bntID);
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
    }

    public override void SetupBTNUI()
    {
        Debug.Log("level SetupBTNUI");
        base.SetupBTNUI();

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
                ExtraUrlBTN[i].onClick.RemoveAllListeners();
                ExtraUrlBTN[i].onClick.AddListener(() => {
                    Application.OpenURL(outUrl);
                });
            }
           
        }

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
