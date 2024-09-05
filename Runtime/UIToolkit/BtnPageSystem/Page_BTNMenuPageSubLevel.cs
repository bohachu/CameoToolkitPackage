using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine.UI;
using Cameo;
using Cameo.UI;
using System.Linq;

/// <summary>
/// 可重用的按鈕選單
/// 下載選單資料
/// 下載關卡特殊資料
/// 鎖定與隱藏特定按鈕
/// 存取使用者的資料，紀錄有哪些按鈕選項已經解鎖
/// </summary>
///
// 定義選單頁面類別
public class Page_BTNMenuPageSubLevel : BasePage
{
    [Tooltip("用於存取專屬此頁面的Menu與state data，必須與FileIndex中的資料相同")]
    [SerializeField]
    protected string BTNMenuUniqueID = "StoryMode";
    [SerializeField]
    protected Button Button_Return;//just hide UI, not close
    [Tooltip("有指定UI物件的話，會依據上稿的按鈕name來定義下一頁面的title")]
    [SerializeField]
    protected Text TitleText;
    [SerializeField]
    protected List<BTNUISet> buttons;
    [SerializeField]
    protected UI_BTNLancherBase DefaultLancher;
    [SerializeField]
    PaymentToUnlock paymentBox;
    public const string Param_PageID = "Param_PageID";
    public const string Param_Title = "Param_Title";
    private void onSwitch(string pageID)
    {
        pageManager.SwitchTo(pageID);
    }
    public void OnBackClicked()
    {
        pageManager.ToPrev();
    }

    protected PlayerMissionState playerMissionState;
    UI_BTNLancherBase gameLancher = null;//目前開啟的遊戲啟動器，需要關卡按鈕被點選的時候實體化，遊戲結束時刪除
    void ClearLancher()
    {
        if (gameLancher != null)
        {
            Debug.Log("刪除遊戲啟動器 Lancher:"+gameLancher.gameObject.name);
            DestroyImmediate(gameLancher.gameObject);
            gameLancher = null;
        }
    }
    BTNUISet curScelectedBTN = null;
    void OnLancherClick()
    {
        if(curScelectedBTN==null)
        {
            Debug.LogError("沒有選擇任何按鈕，無法啟動遊戲");
            return;
        }
        var obj=curScelectedBTN;
        bool isFirstPlay = false;
        var missionData = UI_BTNDataManager.Instance.GetMissionData(BTNMenuUniqueID, obj.bntID);
        if(missionData==null)
        {
            Debug.LogError("找不到mission data, 預設為非第一次遊玩");
        }
        else
            isFirstPlay = !missionData.isDone;

        if (obj.btnLuncher != null)
        {
            Debug.Log("啟動："+ obj.bntID+","+obj.PageID);
            //如果有LancherProcess, 則會執行，沒有則略過
            if(gameLancher!=null)
            {
                Destroy(gameLancher.gameObject);
                gameLancher = null;
            }
            gameLancher = Instantiate<UI_BTNLancherBase>(obj.btnLuncher);
            gameLancher.GetComponent<RectTransform>().SetParent(transform, false);

            StartCoroutine(gameLancher.LanchProcess(obj.bntData,(ScoreResult value)=> {
                Debug.Log("遊戲結束，執行OnMissionDone：");
                //如果有填寫PageID, 則會啟動換頁，不會進入遊戲，沒有則是完成體驗，解鎖下一個按鈕
                if (!string.IsNullOrEmpty(obj.PageID))
                {
                        Debug.Log("啟動換頁："+ obj.PageID);
                    //如果有填寫PageID, 則會啟動換頁，不會進入遊戲
                    pageManager.SwitchTo(obj.PageID, false, CreateParam(obj.bntData));
                }
                else
                {
                    //沒有pageID 所以是啟動遊戲模組已經成功完成體驗了
                    //遊戲成功後，顯示本次選單頁面，並解鎖下一個按鈕
                        ActiveDisactiveAllBTNs(true);
                        Debug.Log("完成遊戲，解鎖下一關");
                        MissionDoneUnlockNextBTN(value);
                        Debug.Log("遊戲結束，顯示上層選單頁面");
                        OnBackClicked();
                }
                
                    
                Invoke("ClearLancher", 0.2f);
            } ,
                UI_BTNDataManager.Instance.GetSheetID(BTNMenuUniqueID), isFirstPlay,
                () => {
                    ActiveDisactiveAllBTNs(true);
                    Debug.Log("遊戲取消後，顯示上層選單頁面");
                    OnBackClicked();
                }));
            ActiveDisactiveAllBTNs(false);
        }
        else if(!string.IsNullOrEmpty(obj.PageID))
        {//如果有填寫PageID, 則會啟動換頁
            pageManager.SwitchTo(obj.PageID,false,CreateParam(obj.bntData));
        }
        else{
            Debug.Log("Launcher is null && obj.PageID is null");
        }

    }

    public virtual void SetupBTNUI()
    {
        // prepare btn click to lancher
//        Debug.Log("Setup base BTNUI");
        foreach (var obj in buttons)
        {
            if(obj.UnLockBtn!=null)
            {
                obj.UnLockBtn.onClick.RemoveAllListeners();
                obj.UnLockBtn.onClick.AddListener(() =>
                {
                    if(paymentBox==null)
                    {
                        Debug.LogError("找不到payment box");
                    }
                    else
                    {
                        paymentBox.ShowPaymentBox(() =>
                        {
                            UnlockBTN(obj.bntID);
                            Debug.Log("付費解鎖成功:"+obj.bntID+","+obj.bntData.Name);
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
                    Debug.LogError("遊戲過程有問題："+ex.Message + "\n" + ex.StackTrace);
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
    }
    Dictionary<string,object> CreateParam(BTNData curBTNData)
    {
        Dictionary<string, object> parm = new Dictionary<string, object>();
        if(!string.IsNullOrWhiteSpace(curBTNData.NextPageIndexID))
        {
            parm.Add(Param_PageID, curBTNData.NextPageIndexID);
        }
        if (!string.IsNullOrWhiteSpace(curBTNData.Name))
        {
            parm.Add(Param_Title, curBTNData.Name);
        }
        return parm;
    }
    protected virtual void SetupWithParam()
    {
        //Debug.Log("NextPage Param is null:" + (pageManager.nextPageParamMapping == null).ToString());
        if (paramMapping == null) return;
        if (paramMapping.ContainsKey(Param_PageID))
        {
            BTNMenuUniqueID = (string)paramMapping[Param_PageID];
            Debug.Log("取得下一頁的資料：" + BTNMenuUniqueID);
        }
        if (paramMapping.ContainsKey(Param_Title))
        {
            var titleName = (string)paramMapping[Param_Title];
            if (TitleText != null)
                TitleText.text = titleName;
        }

    }

    void ActiveDisactiveAllBTNs(bool isActive)
    {
        Debug.Log("將所有按鈕都 開或關 ActiveDisactiveAllBTNs:" + isActive);
        if(isActive)
        {
            //如果是啟動按鈕，則要重新設定按鈕狀態
            SetupMissionData(UI_BTNDataManager.Instance.GetMissionData(BTNMenuUniqueID));
            SetupBTNUI();
        }
        else
        {
            //如果是關閉按鈕，則要先關閉所有按鈕
            foreach (var obj in buttons)
            {
                obj.button.enabled = isActive;
            }
        }
        if (Button_Return != null)
            Button_Return.enabled = isActive;

    }
    public override void OnOpen()
    {
        SetupWithParam();

        SetupBTNs(UI_BTNDataManager.Instance.GetBTNData(BTNMenuUniqueID));
        SetupMissionData(UI_BTNDataManager.Instance.GetMissionData(BTNMenuUniqueID));

        SetupBTNUI();       
    }
    public override void OnOpened()
    {
        BTNUISet highestUnlockedBtn = null;
        foreach (var btn in buttons)
        {
            var state = playerMissionState.GetStateByID(btn.bntID);
            if (state != null && !state.isLock)
            {
                highestUnlockedBtn = btn;
            }
        }
        Debug.Log("Going OnLancherClick() "+ (curScelectedBTN!=null).ToString());
        if (highestUnlockedBtn != null)
        {
            curScelectedBTN = highestUnlockedBtn;
            OnLancherClick();
        }
        else
        {
            if (buttons.Any())
            {
                curScelectedBTN = buttons[0];
                OnLancherClick();
            }
            else
            {
                Debug.LogError("沒有任何Launcher可以點擊");
            }
        }
    }

    public void UnlockBTN(string btnID)
    {
        var nextState = playerMissionState.GetStateByID(btnID);
        nextState.isLock = false;
        UploadMissionState();
        playerMissionState.SetBtnLockByState(ref buttons);
        SetupBTNUI();
    }

    public void MissionDoneUnlockNextBTN(ScoreResult result)
    {
        //依據任務分數，判斷是否要解鎖下一個按鈕
        string NextBTNID = playerMissionState.getNextBTNID(result.ID, buttons);
        if (string.IsNullOrEmpty(NextBTNID))
        {
            Debug.Log(result.ID+"找不到解鎖的BTN ID, 默認不解鎖,next ID："+ NextBTNID);
            return;
        }
        var curState = playerMissionState.GetStateByID(result.ID);
        if (curState.Score < Mathf.CeilToInt(result.Score))
            curState.Score = Mathf.CeilToInt(result.Score);
        curState.PlayNum = curState.PlayNum + 1;
        if (result.IsPass)
        {
            //通過，解鎖
            Debug.Log("通過，解鎖" + NextBTNID);
            curState.isDone = true;

            if (NextBTNID == "AllMissionDone")
            {
                //沒有下一個按鈕ＩＤ了，全部任務完成，要解鎖上一層menu的BTNID，顯示成功提示並且關閉本頁，回到上一頁
                var State = playerMissionState.GetStateByID(result.ID);
                State.isDone = true;
                // 把所有button的isLock設為false, 第一個button為true
                for (int i = 0; i < buttons.Count; i++)
                {
                    string buttonID = buttons[i].bntID;
                    var buttonState = playerMissionState.GetStateByID(buttonID);
                    Debug.Log("已鎖定"+buttonID+":"+(i!=0?"True":"False"));
                    buttonState.isLock = !(i == 0); // 第一個button設為true，其它設為false
                }

                Debug.Log("All mission done，完全過關");
            }
            else
            {
                //解鎖下一個按鈕
                var nextState = playerMissionState.GetStateByID(NextBTNID);
                nextState.isLock = false;
            }
                       Debug.Log("遊戲結束10");
        }
        playerMissionState.SetBtnLockByState(ref buttons);
        UploadMissionState();
        SetupBTNUI();
        Debug.Log("關卡解鎖完成");
    }

    //資料設定
    protected virtual void SetupBTNs(List<BTNData> btnDatas)
    {
        // setup buttons
        for (int i = 0; i < btnDatas.Count; i++)
        {
            if (i >= buttons.Count) break;

            buttons[i].InitData(btnDatas[i]);
            if (!string.IsNullOrEmpty(btnDatas[i].ImageName))
            {
                Sprite normimg = ResourceManager.Instance.LoadAsset<Sprite>(btnDatas[i].ImageName);
                buttons[i].NormImg = normimg;
            }
            if (!string.IsNullOrEmpty(btnDatas[i].LockImageName))
            {
                Sprite lockimg = ResourceManager.Instance.LoadAsset<Sprite>(btnDatas[i].LockImageName);
                buttons[i].LockImg = lockimg;
            }
        }
    }
    protected List<MissionBTNState> SetupMissionData(List<MissionBTNState> missionData)
    {
        if (playerMissionState == null) playerMissionState = new PlayerMissionState();
        playerMissionState.missionBTNStates = missionData;
        if (playerMissionState.IsDataNullOrError())
        {
            Debug.Log("闖關狀態資料異常，重新建立初次遊玩狀態");
            playerMissionState.InitFirstTIimePlayData(buttons);
            UploadMissionState();
        }

        playerMissionState.SetBtnLockByState(ref buttons);
        return playerMissionState.missionBTNStates;
    }
    #region 資料下載上傳


    public void UploadMissionState()
    {
        UI_BTNDataManager.Instance.SetMissionData(BTNMenuUniqueID, playerMissionState.missionBTNStates);
    }
    #endregion
}
