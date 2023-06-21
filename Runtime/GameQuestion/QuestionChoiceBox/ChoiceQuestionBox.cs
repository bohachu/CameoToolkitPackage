using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cameo.UI;
using UnityEngine.UI;
using Cameo;

using UnityEngine.Events;

//多選題基礎類別    

namespace Cameo.QuestionGame
{
    [System.Serializable]
    public partial class QuestionEntity
    {
        public string QuestionID;
        public string QuestionBoxType;
        public string Question;
        public string QuestionAudioUrl;
        public Sprite QuestionImage;
        public List<string> Choices; //文字選項
        public List<Sprite> ChoiceImages; //圖片選項

        public int ChoiceCount
        {
            get
            {

                if (Choices != null)
                {
                    if(Choices.Count>0)
                        return Choices.Count;
                }
                if(ChoiceImages != null)
                {
                    if(ChoiceImages.Count>0)
                        return ChoiceImages.Count;
                } 
                
                return 0;
            }
        }
        public List<int> Answer;
        public string Reason; //原因
        public string Remark;//備注，例如出處
        public UnityAction<string> OnSuccess;
        public UnityAction OnFail;
        public UnityAction OnCancel;
        public UnityAction OnClose;
        public UnityAction<int> OnAnserDone;
        public UnityAction OnTimeUp;
    }
    [System.Serializable]
    public partial class ChoiceBTNSet
    {
        [HideInInspector]
        public int index;
        public Image image; //選項圖片
        public Text text; //選項文字
        public Button ChoiceBTN;//選項按鈕
        [SerializeField]
        Color correctColor = new Color(47 / 255f, 84 / 255f, 85 / 255f);
        [SerializeField]
        Color wrongColor = new Color(189 / 255f, 72 / 255f, 61 / 255f);
        [SerializeField]
        Color selectColor = Color.white;
        public void SetActive(bool active)
        {
            if (text != null)
                text.gameObject.SetActive(active);
            if (image != null)
                image.gameObject.SetActive(active);
            if (ChoiceBTN != null)
                ChoiceBTN.gameObject.SetActive(active);
        }
        List<int> answers;
        UnityAction OnCorrect;
        UnityAction OnWrong;
        public void Init(int index, QuestionEntity questionEntity , UnityAction OnCorrect, UnityAction OnWrong)
        {
            this.index = index;
            if(this.text != null)
            {
                if(questionEntity.Choices.Count<=index)
                {
                    Debug.LogError("文字選項數量不足，請檢查上搞選項數量");
                    return;
                }
                 this.text.text = questionEntity.Choices[index];
            }
               
            if(this.image != null)
            {
                if (questionEntity.ChoiceImages.Count <= index)
                {
                    Debug.LogError("圖片選項數量不足，請檢查上搞圖片數量與圖片是否可以下載");
                    return;
                }
                this.image.sprite = questionEntity.ChoiceImages[index];
            }
                
            this.answers = questionEntity.Answer;
            this.OnCorrect = OnCorrect;
            this.OnWrong = OnWrong;
            ChoiceBTN.onClick.AddListener(
              ()=>{
                OnClick(answers, OnCorrect, OnWrong);
              } 
            );
        }
        public void OnClick(List<int> Answers, UnityAction OnCorrect, UnityAction OnWrong)
        {
            //選取的按鈕改變outline顏色
            if(isCorrect(Answers))
            {
                ChoiceBTN.image.color = correctColor;
                OnCorrect.Invoke();
            }
            else
            {
                ChoiceBTN.image.color = wrongColor;
               if(text!=null)
                    text.color = selectColor;
                OnWrong.Invoke();
            }
        }
        bool isCorrect(List<int> Answers)
        {
            return Answers.Contains(index+1);
        }
    }
    public class ChoiceQuestionBox : BaseMessageBox
    {
        public static BaseMessageBox ShowBox(ChoiceQuestionBox prefab, QuestionEntity question)
        {
            Dictionary<string, object> param = new Dictionary<string, object>();
            param.Add("QuestionEntity", question);
            return MessageBoxManager.Instance.ShowMessageBox(prefab, param);
        }
        [HideInInspector]
        public QuestionEntity questionEntity;
        [SerializeField]
        protected Text questionText;
        [SerializeField]
        protected Image questionImage;
        [SerializeField]
        protected Text ReasonText;
        [SerializeField]
        protected Button closeButton;
        [SerializeField]
        protected TimerCountdown timer;

        [SerializeField]
        public List<ChoiceBTNSet> optionsUIs;

        [SerializeField]
        int TimmerDuration = 60;
        [SerializeField]
        protected bool isAutoCloseBox = true; //是否在答題完成兩秒後自動關閉視窗
        float startTime;
        bool isAnserCorrect = false;
        bool isAnsered=false;

        public int workingTime
        {
            get;
            set;
        }
        public UnityEvent OnCorrect;
        public UnityEvent OnClosed;
        public UnityEvent OnAnswered;
        public UnityEvent OnStartSetting;
    
        protected virtual void onStartSetting() { }

        protected override void onOpen()
        {
            if (paramMapping != null)
            {
                questionEntity = paramMapping["QuestionEntity"] as QuestionEntity;
                setQuestion();
            }

            closeButton.onClick.AddListener(() =>
            {
                //結束答題後，可以按下按鈕關閉視窗，不按，兩秒後也會自動關閉
                StopCoroutine("WaitTimeClose");
                this.Close();
            });

            closeButton.gameObject.SetActive(false);
          
            if (ReasonText != null)
                ReasonText.transform.parent.gameObject.SetActive(false);
                //ReasonText.gameObject.SetActive(false);
            if (timer != null)
                timer.StartTimmer(TimmerDuration, onTimeUp);
            onStartSetting();
            OnStartSetting.Invoke();
            isAnsered=false;
            startTime = Time.time;
        }

        protected virtual void setQuestion()
        {
            if(questionText!=null)
                questionText.text = questionEntity.Question;
            if(questionImage!=null && questionEntity.QuestionImage!=null)
                questionImage.sprite = questionEntity.QuestionImage;
            int OptionCount = questionEntity.ChoiceCount;
            for (int i = 0; i < OptionCount; i++)
            {
                if(optionsUIs.Count<=i)
                {
                    Debug.LogError("選項數量不足");
                    break;
                }
                optionsUIs[i].Init(i, questionEntity, onAnserCorrect, onAnserWrong);
            }
            //隱藏多餘的選項
            if(OptionCount<optionsUIs.Count)
            {
                for (int i = OptionCount; i < optionsUIs.Count; i++)
                {
                    optionsUIs[i].SetActive(false);
                }
            }
            if(ReasonText!=null)
            {
                ReasonText.text = questionEntity.Reason;
            }
        }
        void onAnserCorrect()
        {  
            isAnserCorrect = true;
            onAnswered();
            SystemAudioCenter.Instance.PlayOneShot(AudioClipType.ActionSuccess);
             OnCorrect.Invoke();
        }
        void onAnserWrong()
        {  
            isAnserCorrect = false;
            onAnswered();
            SystemAudioCenter.Instance.PlayOneShot(AudioClipType.ActionFail);
           
        }
        void onAnswered()
        {
            isAnsered=true;
            CountTotalTime();

            //顯示答案
             if (ReasonText != null)
                ReasonText.transform.parent.gameObject.SetActive(true);
                

            //先取消自動關閉，等待玩家按下關閉按鈕
            closeButton.gameObject.SetActive(true);
            StartCoroutine(WaitTimeClose(2));
            
            OnAnswered.Invoke();
        }
        // 當倒數計時器時間到了以後的行為
        protected virtual void onTimeUp()
        {
            isTimeUp = true;
            CountTotalTime();
        }

        void CountTotalTime()
        {
            workingTime = Mathf.CeilToInt(Time.time - startTime);
            if (timer != null)
                    timer.StopCount();
        }
        // 橡皮擦作用
        public void RandomRemoveOption()
        {
            // random remove one option in optionUIs
            List<int> optionNumList = new List<int>();
            for (int i = 0; i < questionEntity.Choices.Count; i++)
            {
                optionNumList.Add(i);
            }
            for(int i = 0; i < questionEntity.Answer.Count; i++)
            {
                optionNumList.Remove(questionEntity.Answer[i]);
            }
            
            int randomNum = Random.Range(0, optionNumList.Count);
            optionsUIs[randomNum].SetActive(false);
        }

        // 蝸牛作用
        public void ResetTimer()
        {
            if (timer != null)
                timer.ResetTime(TimmerDuration);
        }

        bool isCancelGame = false;
        bool isTimeUp = false;
        public void CloseWithoutAnyAction()
        {
            questionEntity.OnClose=null;
            isAnsered=false;
            isTimeUp=false;
            isCancelGame=false;
            this.Close();
        }
        protected override void onClosed()
        {
            base.onClosed();
            OnClosed.Invoke();
            
            if (isTimeUp)
            {
                if(questionEntity.OnTimeUp!=null)
                    questionEntity.OnTimeUp.Invoke(); 
                return;
            }
            if (isCancelGame)
            {
                if(questionEntity.OnCancel!=null)
                    questionEntity.OnCancel.Invoke();
                return;
            }
            if(isAnsered)
                if (isAnserCorrect)
                    questionEntity.OnSuccess.Invoke("QuestionID:" + questionEntity.QuestionID);
                else
                if(questionEntity.OnFail!=null) 
                    questionEntity.OnFail!.Invoke();
            if (questionEntity.OnClose != null)
                    questionEntity.OnClose.Invoke();
        }
        
        IEnumerator WaitTimeClose(int timeSeconds)
        {
            if (isAutoCloseBox)
            {
                yield return new WaitForSeconds(timeSeconds);
                this.Close();
            }
            else
            {
                yield return null;
            }
        }
    }
}
