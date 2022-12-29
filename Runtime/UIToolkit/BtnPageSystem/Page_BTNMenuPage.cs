using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine.UI;
using Cameo;
using Cameo.UI;
/// <summary>
/// 可重用的按鈕選單
/// 下載選單資料
/// 下載關卡特殊資料
/// 鎖定與隱藏特定按鈕
/// 存取使用者的資料，紀錄有哪些按鈕選項已經解鎖
/// </summary>
///
namespace Cameo.UI
{
    #region ClassDefine
    [System.Serializable]
    public class BTNUISet
    {
        public Button button;
        public Button UnLockBtn;
        public Text BtnTitle;

        public Text BtnDescription;
        public Image image;
        public Image LockImgObj;
        public UI_BTNLancherBase btnLuncher;
        public BTNData bntData;
        public Sprite LockImg;
        public Sprite NormImg;
        public string PageID;
        public string bntID
        {
            get
            {
                return bntData.BTNID;
            }
        }
        public void InitData(BTNData data)
        {
            bntData = data;
            if (!data.IsShow)
            {
                button.gameObject.SetActive(false);
            }

            if (!string.IsNullOrWhiteSpace(data.Name))
                if (BtnTitle != null)
                    BtnTitle.text = data.Name;
            if (!string.IsNullOrWhiteSpace(data.Desc))
                if(BtnDescription!=null)
                    BtnDescription.text = data.Desc;
        }
        public void SetupImage(Sprite normimg,Sprite lockimg)
        {
            LockImg = lockimg;
            NormImg = normimg;
//            Debug.Log("SetupImage:" + bntData.BTNID+ " IsLockAtFirst:" + bntData.IsLockAtFirst);
            if (bntData.IsLockAtFirst)
            {
                if (LockImg != null) image.sprite = LockImg;
            }
            else
            {
                if (NormImg != null) image.sprite = NormImg;
            }

        }
    }
    [System.Serializable]
    public class BTNData
    {
        public string BTNID;
        public string Name;
        public string Desc;
        public bool IsShow;
        public bool IsLockAtFirst;
        public string ImageName;
        public string LockImageName;
        public string QuestionWorkSheetName;    // 如果按鈕下一頁不再是選單，而是進入遊戲，則需要填寫遊戲資料的表格所在
        public string DialogueWorkSheetName;    // 如果按鈕下一頁不再是選單，而是進入遊戲，則需要填寫遊戲的對話的表格所在
        public string Background;
        public string AwardFirstID;
        public string NextPageIndexID; //高中英文新增欄位： 下一頁的google sheet fileindex key, 用來下載下一頁的資料
        public string ExtraParam;//用於額外通用型的資料，高中英文新增欄位：增加額外點選時候的資料URl
        public override string ToString()
        {
            return BTNID + "," + Name + "," + IsShow + "," + IsLockAtFirst + "," + ImageName + "," + QuestionWorkSheetName + "," + Background + "," + AwardFirstID;

        }
    }
    [System.Serializable]
    public class MissionBTNState
    {
        public string BTNID;    //任務的按鈕ＩＤ
        public bool isDone = false; //任務是否完成
        public bool isLock = true;  //任務是否鎖定中
        public int Score=0; //任務的分數
        public MissionBTNState()
        {
            BTNID = "";
            isDone = false;
            isLock = true;
        }
    }
    public class PlayerMissionState
    {
        public List<MissionBTNState> missionBTNStates;

        public void InitFirstTIimePlayData(List<BTNUISet> buttons)
        {
            Debug.Log("第一次遊玩，建立初始狀態資料");
            // init defat missionData
            missionBTNStates = new List<MissionBTNState>();
            for (int i = 0; i < buttons.Count; i++)
            {
                MissionBTNState newOne = new MissionBTNState();
                newOne.BTNID = buttons[i].bntData.BTNID;
                newOne.isDone = false;
                newOne.isLock = buttons[i].bntData.IsLockAtFirst;
                missionBTNStates.Add(newOne);
            }
        }
        public bool IsDataNullOrError()
        {
            if (missionBTNStates == null) return true;
            if (missionBTNStates.Count == 0) return true;

            foreach (var obj in missionBTNStates)
                if (string.IsNullOrEmpty(obj.BTNID)) return true;
            return false;
        }

        public string getNextBTNID(string currentBTNID, List<BTNUISet> buttons)
        {
            for (int i = 0; i < buttons.Count; i++)
            {
                if (buttons[i].bntID == currentBTNID)
                {
                    if (i == buttons.Count - 1)
                    {
                        // 所有任務都完成了
                        return "AllMissionDone";
                    }
                    else
                    {
                        return buttons[i + 1].bntID;
                    }
                }
            }
            return "";
        }

        public void SetBtnLockByState(ref List<BTNUISet> buttons)
        {
            foreach (var btn in buttons)
            {
                var state = GetStateByID(btn.bntID);
                if (state == null)
                {
                    Debug.LogError(btn.bntID + " stat is null,建立新的狀態檔案");
                    InitFirstTIimePlayData(buttons);
                    state = GetStateByID(btn.bntID);
                    
                }
                btn.button.enabled = !state.isLock;

                if(btn.LockImgObj!=null)
                    btn.LockImgObj.gameObject.SetActive(state.isLock);

                if (state.isDone)
                {
                    if(btn.NormImg!=null)
                        btn.image.sprite = btn.NormImg;
                }
                else
                {
                    if (btn.LockImg != null)
                        btn.image.sprite = btn.LockImg;
                }
            }
        }
        public void SetupState(List<MissionBTNState> states)
        {
            missionBTNStates = states;
        }

        public MissionBTNState GetStateByID(string id)
        {
            foreach (var obj in missionBTNStates)
            {
                if (obj.BTNID == id) return obj;
            }
            return null;
        }

    }
    
    #endregion
}
public class Page_BTNMenuPage : BasePage
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


    public virtual void SetupBTNUI()
    {
        // prepare btn click to lancher
        foreach (var obj in buttons)
        {
            if(obj.UnLockBtn!=null)
            {
                obj.UnLockBtn.onClick.RemoveAllListeners();
                obj.UnLockBtn.onClick.AddListener(() =>
                {
                    PaymentToUnlock paymentBox = FindObjectOfType<PaymentToUnlock>();
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
                    StartCoroutine(obj.btnLuncher.LanchProcess(obj.bntData,(ScoreResult value)=> {
                        //如果有填寫PageID, 則會啟動換頁，不會進入遊戲，沒有則是完成體驗，解鎖下一個按鈕
                        if (!string.IsNullOrEmpty(obj.PageID))
                        {
                            //如果有填寫PageID, 則會啟動換頁，不會進入遊戲
                            pageManager.SwitchTo(obj.PageID, false, CreateParam(obj.bntData));
                        }
                        else
                        {
                            //沒有pageID 所以是啟動遊戲模組已經成功完成體驗了
                            //遊戲成功後，顯示本次選單頁面，並解鎖下一個按鈕
                            ActiveDisactiveAllBTNs(true);
                            MissionDoneUnlockNextBTN(value);
                        }
                    } ,
                        UI_BTNDataManager.Instance.GetSheetID(BTNMenuUniqueID), isFirstPlay,
                        () => {
                            //遊戲取消後，顯示本次選單頁面
                            ActiveDisactiveAllBTNs(true);
                        }));
                    ActiveDisactiveAllBTNs(false);
                }
                else if(!string.IsNullOrEmpty(obj.PageID))
                {//如果有填寫PageID, 則會啟動換頁
                    pageManager.SwitchTo(obj.PageID,false,CreateParam(obj.bntData));
                }
                
                SystemAudioCenter.Instance.PlayOneShot(AudioClipType.CommonUIButton);
                   
            });
        }
        if (Button_Return != null)
        {
            Button_Return.onClick.RemoveAllListeners();
            Button_Return.onClick.AddListener(() =>
            {
                //回到上一頁
                pageManager.ToPrev();
            });
        }
    }
    Dictionary<string,object> CreateParam(BTNData curBTNData)
    {
        ///為下一個page建立輸入資料
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
        Button_Return.enabled = isActive;
        foreach (var obj in buttons)
        {
            obj.button.enabled = isActive;
        }
    }
    public override void OnOpen()
    {
        SetupWithParam();

        SetupBTNs(UI_BTNDataManager.Instance.GetBTNData(BTNMenuUniqueID));
        SetupMissionData(UI_BTNDataManager.Instance.GetMissionData(BTNMenuUniqueID));

        SetupBTNUI();
    }

    public void UnlockBTN(string btnID)
    {
        //解鎖按鈕
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
        curState.Score = Mathf.CeilToInt(result.Score);
        
        if(result.IsPass)
        {
            //通過，解鎖
            Debug.Log("通過，解鎖" + NextBTNID);
            curState.isDone = true;
           
            if (NextBTNID == "AllMissionDone")
            {
                //沒有下一個按鈕ＩＤ了，全部任務完成，要解鎖上一層menu的BTNID，顯示成功提示並且關閉本頁，回到上一頁
                var State = playerMissionState.GetStateByID(result.ID);
                State.isDone = true;
                playerMissionState.SetBtnLockByState(ref buttons);
                UploadMissionState();
                Debug.Log("All mission done");
                return;
            }
            else
            {
                //解鎖下一個按鈕
                var nextState = playerMissionState.GetStateByID(NextBTNID);
                nextState.isLock = false;
            }
           
        }
        playerMissionState.SetBtnLockByState(ref buttons);
        UploadMissionState();
        SetupBTNUI();
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
                Sprite lockimg = ResourceManager.Instance.LoadAsset<Sprite>(btnDatas[i].LockImageName);
                buttons[i].SetupImage(normimg, lockimg);
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
