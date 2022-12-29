using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Cameo;
using Cameo.UI;

public class UI_BTNDataManager : Singleton<UI_BTNDataManager>
{
    public List<UI_BTNPageDataLoader> preloaders;
    public IEnumerator WaitForloading(string BTNMenuUniqueID)
    {
        var loader = GetPreloader(BTNMenuUniqueID);
        if(loader)
        yield return null;
    }
    public string GetStateFileName(string BTNMenuUniqueID)
    {
        return GetPreloader(BTNMenuUniqueID).StateFileName;
    }
    public string GetSheetID(string BTNMenuUniqueID)
    {
        return GetPreloader(BTNMenuUniqueID).BTNDtataSheetID;
    }
    public IEnumerator Preload(string BTNMenuUniqueID)
    {
         yield return GetPreloader(BTNMenuUniqueID).InitializeCoroutine();
    }
    public List<BTNData> GetBTNData(string BTNMenuUniqueID)
    {
        return GetPreloader(BTNMenuUniqueID).BtnDatas;
    }
    public MissionBTNState GetMissionData(string BTNMenuUniqueID, string BTNID)
    {
        var pageData = GetPreloader(BTNMenuUniqueID);
        if(pageData==null)
        {
            Debug.LogError("找不到MissionData，可能沒有下載或是沒有建立：" + BTNMenuUniqueID);
            return null;
        }
        var datalist = pageData.MissionData;
        foreach(var obj in datalist)
        {
            if (obj.BTNID == BTNID)
                return obj;
        }
        return null;
    }
    public List<MissionBTNState> GetMissionData(string BTNMenuUniqueID)
    {
        return GetPreloader(BTNMenuUniqueID).MissionData;
    }
    public List<MissionBTNState> SetMissionData(string BTNMenuUniqueID, List<MissionBTNState>  data)
    {
        return GetPreloader(BTNMenuUniqueID).MissionData= data;
    }
    public UI_BTNPageDataLoader GetPreloader(string BTNMenuUniqueID)
    {
       
        foreach(var obj in preloaders)
        {
           // Debug.Log(obj.BTNMenuUniqueID + " compare to "+ BTNMenuUniqueID);
            
            if (obj.BTNMenuUniqueID == BTNMenuUniqueID)
                return obj;
            
        }
        Debug.LogError("找不到對應的 BTN Menu Preloader, 請設定場景中"+name);
        Debug.Log("對應ID:"+BTNMenuUniqueID);
        return null;
    }
    public IEnumerator LoadAll()
    {
        foreach (var obj in preloaders)
            yield return StartCoroutine(obj.InitializeCoroutine());
    }
   
}
