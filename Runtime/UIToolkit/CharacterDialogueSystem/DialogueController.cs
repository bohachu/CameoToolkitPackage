using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using Cameo;
[System.Serializable]
public class DialogueActionUnit
{
    public DialogData originData; // for debug or plugin use
    public string Dialogue;
    public DialogueController.CharacterExpression characterExpression = DialogueController.CharacterExpression.Default;

    public bool IsBGChange = false;
    [ShowIf("IsBGChange")]
    public Sprite BGImage;

    public bool IsCenterImgChange = false;
    [ShowIf("IsCenterImgChange")]
    public Sprite CenterImg;
    public bool IsAudioEnable = false;
    [ShowIf("IsAudioEnable")]
    public AudioClipType Audio;

    public bool IsActionEnable = false;
    [ShowIf("IsActionEnable")]
    public UnityEvent Action;
  
    public DialogueActionUnit()
    {
        Dialogue = "";
        Action = new UnityEvent();
    }
    
    public DialogueActionUnit(DialogData data)
    {
        Dialogue = data.Dialogue;
        Action = new UnityEvent();
        originData = data;
    }
}
[System.Serializable]
public class DialogueSet
{
    public List<DialogueActionUnit> AdvanceDialogues;
   
    public DialogueSet()
    {
        AdvanceDialogues = new List<DialogueActionUnit>();
    }
    public DialogueController.CharacterPosition characterPosition = DialogueController.CharacterPosition.Right;
   
    public int CurDialogueIndex
    {
        get
        {
            return DialogueController.Instance.CurrentDialogueIndex;
        }
    }
    void CheckError()
    {
        if(AdvanceDialogues == null) AdvanceDialogues = new List<DialogueActionUnit>();
        if(AdvanceDialogues.Count ==0)
        {
            AdvanceDialogues.Add(new DialogueActionUnit());
        }
    }
    public void ChangeDialogueOrder(int order)
    {
        DialogueController.Instance.ChangeOrder(order);
    }
    public void ResetDialogueOrder()
    {
        DialogueController.Instance.ResetOrder();
    }
    public void StartDialogues()
    {
        CheckError();
        var DialogueCenter = DialogueController.Instance;
        if(DialogueCenter!=null)
        {
            DialogueCenter.SetCharactorState(characterPosition);
            
            DialogueCenter.ShowDialogues(AdvanceDialogues);
           
        }
        else
        {
            Debug.Log("DialogueCenter is null");
        }
       
    }
    //任何DialogueSet都可以執行此指令，會直接顯現按鈕，但是使用者流程上的bug尚未解決，如果對話本身是多句就會出問題，需要謹慎使用
    public void ShowBTN(Action action, DialogueController.BTNType finalBTN = DialogueController.BTNType.Next)
    {
        var DialogueCenter = DialogueController.Instance;
        DialogueCenter.ShowBTN(action, finalBTN);
    }
    public void HideBTN()
    {
        var DialogueCenter = DialogueController.Instance;
        DialogueCenter.HideBTN();
    }
    public void StartDialogues(Action action, DialogueController.BTNType finalBTN= DialogueController.BTNType.Next)
    {
        //Debug.Log("啟動有action的對話");
        CheckError();
        var DialogueCenter = DialogueController.Instance;
        if (DialogueCenter != null)
        {
            
           DialogueCenter.ShowDialogues(AdvanceDialogues, action, finalBTN);
           
        }
       
    }
    public void Hide()
    {
        //Debug.Log("隱藏對話");
        var DialogueCenter = DialogueController.Instance;
        DialogueCenter.ShowHideDialogue(false);
    }
}

[System.Serializable]
public class OnDialogueShowEvent : UnityEvent<DialogueActionUnit> { }


public class DialogueController : MonoBehaviour
{
    private static DialogueController instance;

    public static DialogueController Instance
    {
        get
        {
            if(instance==null)
            {
                var prefab = Resources.Load("DialogueCanvas", typeof(GameObject)) as GameObject;
                var obj = Instantiate(prefab);
                instance = FindObjectOfType<DialogueController>();
                instance.initCanvasOrder();
            }
            return instance;
        }
    }
    public int CurrentDialogueIndex
    {
        get
        {
            return multiDialogueIndex;
        }
    }
     DialogueActionUnit currentDialogue;
    public DialogueActionUnit CurrentDialogue
    {
        get
        {
            return currentDialogue;
        }
    }
    public OnDialogueShowEvent _OnDialogueShowEvent;
    public Text DialogueText;
    public Button dialogueBTN;
    public Image BGImage;
    public Image CenterImage;
    public DialogueMultiMediaPlayer BGMediaPlayer;
    public DialogueMultiMediaPlayer CenterMediaPlayer;

    private int InitDialogueOrder=2;
    private Canvas UICanvas;
    public void ChangeOrder(int newOrder)
    {
        UICanvas.sortingOrder= newOrder;
    }
    public void ResetOrder()
    {
        UICanvas.sortingOrder = InitDialogueOrder;
    }
    void initCanvasOrder()
    {
        if (UICanvas == null)
        {
            UICanvas = GetComponent<Canvas>();
            InitDialogueOrder = UICanvas.sortingOrder;
        }
    }
   
    [SerializeField]
    Button dialogueBTN2;
    public GameObject DialogRoot;
    [SerializeField]
    Text CharacterName;
    [SerializeField]
    Image CharacterImg;
    [SerializeField]
    RectTransform CharacterLeftPos;
    [SerializeField]
    RectTransform CharacterRightPos;
    [SerializeField]
    Image DialogueBG;
    [SerializeField]
    Sprite LeftDialogue;
    [SerializeField]
    Sprite RIghtDialogue;

    [SerializeField]
    List<CharacterSet> CharacterSets;
    public CharacterSet PlayerSet;

    [SerializeField]
    string PlayerReplaceString = "__(玩家名字)__";
    [System.Serializable]
    public class CharacterSet
    {
        public string CharacterID;
        public string CharacterName;
        public Sprite CharacterImg;
    }
    public enum CharacterExpression
    {
        Default = 0,
        Role1 = 1,
        Role2 = 2,
        Role3 = 3,
        Role4 = 4,
        Role5 = 5,
        Role6 = 6,
        Role7 = 7,
        Role8 = 8,
        Role9 = 9,
        Smile=10,
        Sad=11,
        Player=100
    }
    public enum CharacterPosition
    {
        Right = 0,
        Left = 1,
        Hide = 2
    }
    public enum BTNType
    {
        Continue=0,
        Next=1,
        Comfirm=2,
        TakePhoto=3,
        ChoseColor=4,
        ToPlant=5,
        ToFlower=6,
        MissionEnd=7,
        ToLeaf = 8,
        Skip=9,
        Start=10
    }
    public enum BTNIndex
    {
        First=0,
        Second=1
    }
    Button ChoseBTN(BTNIndex bTNIndex)
    {
        switch(bTNIndex)
        {
            case BTNIndex.First:
                return dialogueBTN;
            case BTNIndex.Second:
                return dialogueBTN2;
        }
        return dialogueBTN;
    }
    private void setBTNText(Button btn, BTNType bTNType)
    {
        var text = btn.GetComponentInChildren<Text>();
        switch(bTNType)
        {
            case BTNType.Comfirm:
                text.text = "確認";
                break;
            case BTNType.Continue:
                text.text = "繼續>";
                break;
            case BTNType.Next:
                text.text = "下一步>";
                break;
            case BTNType.TakePhoto:
                text.text = "開始拍照";
                break;
            case BTNType.ToLeaf:
                text.text = "加入葉";
                break;
            case BTNType.ToPlant:
                text.text = "種植";
                break;
            case BTNType.ToFlower:
                text.text = "加入花";
                break;
            case BTNType.MissionEnd:
                text.text = "回到地圖";
                break;
            case BTNType.Skip:
                text.text = "略過";
                break;
            case BTNType.Start:
                text.text = "開始";
                break;
        }
    }
    public void ShowBTN(Action action,BTNType bTNType, BTNIndex bTNIndex= BTNIndex.First)
    {
        Button curBTN = ChoseBTN(bTNIndex);
        setBTNText(curBTN,bTNType);
        curBTN.gameObject.SetActive(true);
        curBTN.onClick.RemoveAllListeners();
        curBTN.interactable = true;
        curBTN.onClick.AddListener(delegate {
            curBTN.interactable = false;
            //action.Invoke();
            
            StartCoroutine(DelayAction(0.1f, action));

            StartCoroutine(EnableButtonAfterDelay(curBTN, 0.2f));
        });
        AddBTNClickSound(curBTN);
    }
    IEnumerator DelayAction(float seconds,Action action)
    {
        yield return new WaitForSeconds(seconds);
        action.Invoke();
    }

    IEnumerator EnableButtonAfterDelay(Button button, float seconds)
    {
        yield return new WaitForSeconds(seconds);
        button.interactable = true;
    }
    public void HideBTN(BTNIndex bTNIndex = BTNIndex.First)
    {
        Button curBTN = ChoseBTN(bTNIndex);
        curBTN.gameObject.SetActive(false);

    }
    private CharacterExpression preExpression;
    public int GetExpressionIndexByName(string roleID)
    {
        int ind = 0;
        foreach(var obj in CharacterSets)
        {
            if (obj.CharacterID == roleID)
                return ind;
            ind++;
        }
        if (roleID == PlayerSet.CharacterID) return (int)CharacterExpression.Player;
        Debug.LogError("對話expression中找不到對應的角色名稱，確認dialog controller："+roleID);
        return 0;
    }
    public void SetCharacterExpression(CharacterExpression characterExpression)
    {
        
        //if (preExpression == characterExpression) return;
        preExpression = characterExpression;
        int characterIndex = (int)characterExpression;
        CharacterImg.enabled = true;
        if (characterExpression == CharacterExpression.Player)
        {
            if(PlayerSet.CharacterImg!=null)
                CharacterImg.sprite = PlayerSet.CharacterImg;
            else CharacterImg.enabled = false;
            CharacterName.text = PlayerSet.CharacterName;
        }
        else
        {
            
            CharacterImg.sprite = CharacterSets[characterIndex].CharacterImg;
            CharacterName.text = CharacterSets[characterIndex].CharacterName;
        }
    }
    public void SetCharactorState(CharacterPosition characterPosition)
    {
        switch(characterPosition)
        {
            case CharacterPosition.Left:
                CharacterImg.rectTransform.position = CharacterLeftPos.position;
                DialogueBG.sprite = LeftDialogue;
                CharacterImg.gameObject.SetActive(true);
                break;
            case CharacterPosition.Right:
                CharacterImg.rectTransform.position = CharacterRightPos.position;
                DialogueBG.sprite = RIghtDialogue;
                CharacterImg.gameObject.SetActive(true);
                break;
            case CharacterPosition.Hide:
                CharacterImg.gameObject.SetActive(false);
                break;

        }
    }
    public void Reset()
    {
        Debug.Log("Reset");
        BGImage.sprite = null;
        BGImage.gameObject.SetActive(false);
        CenterImage.sprite = null;
        CenterImage.gameObject.SetActive(false);
        if(BGMediaPlayer!=null)
        {
            BGMediaPlayer.Reset();
        }
        if(CenterMediaPlayer!=null)
        {
            CenterMediaPlayer.Reset();
        }
    }

    public void ShowHideDialogue(bool isShow)
    {
        DialogRoot.SetActive(isShow);
        if(isShow)
        {
            if(BGImage.sprite!=null)
            {
                BGImage.gameObject.SetActive(isShow);
            }
            if(CenterImage.sprite!=null)
            {
                CenterImage.gameObject.SetActive(isShow);
            }
        }
        else
        {
             BGImage.gameObject.SetActive(false);
              CenterImage.gameObject.SetActive(false);
        }
       
       if(BGMediaPlayer!=null)
        {
            BGMediaPlayer.IsShow(isShow);
        }
        if(CenterMediaPlayer!=null)
        {
            CenterMediaPlayer.IsShow(isShow);
        }
        //dialogueBTN.gameObject.SetActive(isShow);

    }
    
    private void AddBTNClickSound(Button btn)
    {
        btn.onClick.AddListener(delegate {
            SystemAudioCenter.Instance.PlayOneShot(AudioClipType.DefaultClick);
        });
    }
    private void Awake()
    {
        if(instance==null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
        dialogueBTN.gameObject.SetActive(false);
        dialogueBTN2.gameObject.SetActive(false);
        BGImage.sprite = null;
        BGImage.gameObject.SetActive(false);
        CenterImage.sprite = null;
        CenterImage.gameObject.SetActive(false);
        DialogRoot.SetActive(false);
        Reset();
       
    }
   
    public void ShowDialogues(List<DialogueActionUnit> textWithAction, Action action=null, BTNType finalBTN = BTNType.Continue)
    {
        ShowHideDialogue(true);
        multiDialogueIndex = 0;
        //  Debug.Log("開始對話");
       // Debug.Log("ShowNextDialogue" + action);
        ShowNextDialogue(textWithAction, action, finalBTN);
    }
    int multiDialogueIndex = 0;
    string replacePlayerName(string dialogueSource)
    {
        if (dialogueSource.Contains(PlayerReplaceString))
        {
//            Debug.Log("字串包含：" + PlayerReplaceString+"替換為:"+ PlayerSet.CharacterName);
            return dialogueSource.Replace(PlayerReplaceString, PlayerSet.CharacterName);
        }
        else
        {
            return dialogueSource;
        }
    }
   
    void ShowTextAndAction(DialogueActionUnit dialogueActionUnit)
    {
        DialogueText.text = replacePlayerName(dialogueActionUnit.Dialogue);
        SetCharacterExpression(dialogueActionUnit.characterExpression);
        if(dialogueActionUnit.IsBGChange)
        {
            
            Debug.Log("BGChange:"+dialogueActionUnit.BGImage+","+dialogueActionUnit.originData.BGImage);
            BGImage.sprite = dialogueActionUnit.BGImage;
             Debug.Log("BGChange done :"+dialogueActionUnit.BGImage);
            if (dialogueActionUnit.BGImage != null)
            {
                BGImage.gameObject.SetActive(true);
            }
            else
            {
                BGImage.gameObject.SetActive(false);
            }
            if(BGMediaPlayer!=null)
            {
                BGMediaPlayer.PlayMedia(dialogueActionUnit.originData.BGImage);
            }
             Debug.Log("BGChange done :"+dialogueActionUnit.BGImage);
             
        }
        if (dialogueActionUnit.IsCenterImgChange)
        {
            //更換中間圖片，如果沒有圖片則隱藏
            CenterImage.sprite = dialogueActionUnit.CenterImg;
            CenterImage.color = Color.white;
            if (dialogueActionUnit.CenterImg != null)
            {
                CenterImage.gameObject.SetActive(true);
            }
            else
            {   
                CenterImage.gameObject.SetActive(false);
            }
            //如果是影片或是iframe則播放
//            Debug.Log("CenterImgChange 播放影片或是iframe:" + dialogueActionUnit.originData.CenterImage);
            if(CenterMediaPlayer!=null)
                CenterMediaPlayer.PlayMedia(dialogueActionUnit.originData.CenterImage);
            
        }
        if (dialogueActionUnit.IsAudioEnable)
        {
            SystemAudioCenter.Instance.PlayOneShot(dialogueActionUnit.Audio);
        }
        if (dialogueActionUnit.IsActionEnable)
        {
            dialogueActionUnit.Action.Invoke();
            
        }
           
        
    }
    bool ShowNextDialogue(List<DialogueActionUnit> text, Action action, BTNType finalBTN)
    {
        currentDialogue = text[multiDialogueIndex];
        _OnDialogueShowEvent.Invoke(currentDialogue);
        ShowTextAndAction(currentDialogue);
        if (multiDialogueIndex == (text.Count - 1))
        {   //Debug.Log("最後一句話");
            
            multiDialogueIndex = 0;
           // Debug.Log("ShowNextDialogue" + action);
            if (action != null)
            {
                //Debug.Log("結束了，往下一步，顯示BTN");
                ShowBTN(delegate {
                    action();
                }, finalBTN);
            }
            else
            {
                //Debug.Log("最後一句對話，隱藏BTN");
                HideBTN();
            }
            return true;
        }
        else
        {
            multiDialogueIndex++;
            //Debug.Log("還有下一句對話，顯示BTN");
            ShowBTN(delegate {
               // Debug.Log("按下了按鈕BTN");
                ShowNextDialogue(text, action, finalBTN);
            },  BTNType.Continue);
            return false;
        }
    }
   
}
