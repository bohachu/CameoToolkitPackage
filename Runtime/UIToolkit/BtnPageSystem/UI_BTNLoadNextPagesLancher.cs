using System.Collections;
using System.Collections.Generic;
using Cameo.UI;
using UnityEngine;
using UnityEngine.Events;

public class UI_BTNLoadNextPagesLancher : UI_BTNLancherBase
{
    public UI_BTNPageDataLoader LoaderPrefab;
    public UITool_GameLoadingProcess LoadingInfoBox;
    public override IEnumerator LanchProcess(BTNData bTNData, UnityAction<Cameo.ScoreResult> _OnMissionDone, string DataSheetID, bool IsFirstPlay, UnityAction _OnMissionCancel = null)
    {
        var loading = UITool_GameLoadingProcess.ShowBox(LoadingInfoBox).GetComponent<UITool_GameLoadingProcess>();

        loading.SetInfoProgress("啟動頁面下載流程", 0);
        yield return base.LanchProcess(bTNData, _OnMissionDone, DataSheetID, IsFirstPlay, _OnMissionCancel);
        var Loader = Instantiate<UI_BTNPageDataLoader>(LoaderPrefab);
        Loader.BTNMenuUniqueID = bTNData.NextPageIndexID;
        Loader.transform.parent = UI_BTNDataManager.Instance.transform;
        UI_BTNDataManager.Instance.preloaders.Add(Loader);

        loading.SetInfoProgress("啟動頁面下載流程", 0.8f);
        yield return StartCoroutine(Loader.InitializeCoroutine());
        loading.Close();
        // 完成loadding 跳轉到下一頁，需要下一頁的page ID
        _OnMissionDone.Invoke(new Cameo.ScoreResult(bTNData.NextPageIndexID,0,0,true));
    }
}
