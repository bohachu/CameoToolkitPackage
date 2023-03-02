using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Threading.Tasks;
using Newtonsoft.Json;
using Cameo;
using Cameo.UI;
public class UI_BTNPageDataLoader : MonoBehaviour
{
    [SerializeField]
    private string _BTNMenuUniqueID = "StoryMode";
    public string BTNMenuUniqueID
    {
        get
        {
            return _BTNMenuUniqueID;
        }
        set
        {
            _BTNMenuUniqueID = value;
        }
    }
    public string BTNDtataSheetID
    {
        get
        {
            return BTNMenuUniqueID;
        }
    }
    public string StateFileName
    {
        get
        {
            return BTNMenuUniqueID + "_" + BTNMenuStateFileName;
        }
    }
    public const string BTNMenuStateFileName = "BTNState.json";
    [SerializeField]
    List<BTNData> btnDatas;
    public List<BTNData> BtnDatas
    {
        get
        {
            if(!isLoad)
            {
                Debug.LogError("Data is not loading yet");
            }
            return btnDatas;
        }
    }
    [SerializeField]
    List<MissionBTNState> missionData;
    public List<MissionBTNState> MissionData
    {
        get
        {
            if (!isLoad)
            {
                Debug.LogError("Data is not loading yet");
            }
            return missionData;
        }
        set
        {
            missionData = value;
            StartCoroutine(uploadBTNMissionDataTask().AsIEnumerator());
        }
    }
   
    async Task uploadBTNMissionDataTask()
    {
        Debug.Log("上傳闖關狀態");
        string errorMsg = await FileRequestHelper.Instance.UploadGameData(UI_BTNDataManager.Instance.GetStateFileName(BTNMenuUniqueID), MissionData, GlobalDataMediator.PlayerAccount, GlobalDataMediator.PlayerToken);

        if (!string.IsNullOrEmpty(errorMsg))
        {
            Debug.LogError(errorMsg);
        }
    }
    bool isLoad=false;
    public bool IsLoad
    {
        get
        {
            return isLoad;
        }
    }
    [Tooltip("if True, 不下載資料，直接使用設定數值")]
    [SerializeField]
    bool isOffLine = false;
    public IEnumerator InitializeCoroutine()
    {
        if(!isOffLine)
        {
            // load data from sheets and player json dada
            if (!isLoad)
                yield return LoadBTNData(BTNDtataSheetID).AsIEnumerator();
            
        }
        //預先下載 按鈕影像
        List<string> ImageAddres = GetImagesAddress(btnDatas);
        yield return ResourceManager.Instance.preloadImagesAssetCoroutine(ImageAddres);

        yield return LoadPlayerMissionState(StateFileName).AsIEnumerator();

        isLoad = true;
//        Debug.Log(name + " Loadding End");
        yield return null;
    }
    List<string> GetImagesAddress(List<BTNData> data)
    {
        List<string> ImageAddres = new List<string>();
        foreach(var obj in data)
        {
           if(!string.IsNullOrWhiteSpace(obj.ImageName))
            {
                ImageAddres.Add(obj.ImageName);
                if(!string.IsNullOrWhiteSpace(obj.LockImageName))
                {
                    if(obj.ImageName!=obj.LockImageName)
                        ImageAddres.Add(obj.LockImageName);
                }
            }
            
            if (!string.IsNullOrWhiteSpace(obj.Background))
            {
                ImageAddres.Add(obj.Background);
            }
        }
        return ImageAddres;
    }
    async Task LoadBTNData(string BTNDtataSheetID)
    {
        var downloadIndex = DownloadInfoManager.Instance.DownloadInfo(BTNDtataSheetID);
        if (downloadIndex == null)
        {
            Debug.Log("找不到sheet index:" + BTNDtataSheetID);
        }

        var SpreadSheet = downloadIndex.SpreadSheet;
        var WorkSheet = downloadIndex.WorkSheet;
        string url = string.Format("{0}/?{1}={2}&{3}={4}&{5}={6}.sheet&{7}={8}", FastAPISettings.BaseDataUrl,
                FastAPISettings.AccountKey,  GlobalDataMediator.PlayerAccount, 
                FastAPISettings.TokenKey, GlobalDataMediator.PlayerToken,
                FastAPISettings.SpreadSheetKey, SpreadSheet,
                FastAPISettings.WorkSheetKey, WorkSheet);

        try
        {
            string jsonStr = await FileRequestHelper.Instance.LoadJsonString(url);
//            Debug.Log(jsonStr);
            btnDatas = JsonConvert.DeserializeObject<List<BTNData>>(jsonStr);
            //Debug.Log(menuList[0].ToString());
            //return menuList;
        }
        catch (System.Exception e)
        {
            Debug.LogError("後台有問題：可能是index上稿錯誤");
            Debug.LogError(url);
            Debug.LogError(e);
        }
       // return new List<BTNData>();
    }
    async Task LoadPlayerMissionState(string StateFileName)
    {
        string url = string.Format("{0}/?{1}={2}&{3}={4}&{5}={6}",
                FastAPISettings.GetRequestUrl,
                FastAPISettings.AccountKey,  GlobalDataMediator.PlayerAccount, 
                FastAPISettings.TokenKey, GlobalDataMediator.PlayerToken,
                FastAPISettings.FileKey, StateFileName);

//        Debug.Log("Stste file : "+url);
        string jsonStr = await FileRequestHelper.Instance.LoadJsonString(url);
        if (jsonStr.Contains("No such file"))
        {
//            Debug.Log("找不到檔案，有可能是第一次使用者，進行建立檔案");
            missionData = new List<MissionBTNState>();
           // return missionState;
        }
        else
        {
//            Debug.Log(jsonStr);
            missionData = JsonConvert.DeserializeObject<List<MissionBTNState>>(jsonStr);
            //JsonTool.ListJsonConverter<MissionBTNState>(jsonStr);
           // return missionState;
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        if(isOffLine)
        {
            isLoad = true;
        }
    }
}
