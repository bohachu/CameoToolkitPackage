using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

using System;
namespace Cameo.UI
{
    public class MessageBoxManager : Singleton<MessageBoxManager>
    {
        public Image Background;
        public MessageBoxInfo[] MessageBoxInfoList;
        public float FadeTime = 0.2f;
        public Color BackgroundColor = Color.black;

        public Action OnAllMessageBoxClosed=new Action(() => { });
        private RectTransform rectTran;
        private List<BaseMessageBox> curOpendMessageBoxs = new List<BaseMessageBox>();
        private BaseMessageBox curMsgBox = null;
        private Dictionary<string, object> paramMapping;
        private Dictionary<string, BaseMessageBox> msgBoxInfoMap;
        public Action OnAnyMessageBoxOpened=new Action(() => { });
        public Action OnAnyMessageBoxClosed=new Action(() => { });
        public BaseMessageBox ShowComfirmBox(string msg, UnityEngine.Events.UnityAction onClick, UnityEngine.Events.UnityAction OnCancel = null,bool isUseBackground=true)
        {
            Dictionary<string, object> paramMap = new Dictionary<string, object>();
            paramMap[UI_ComfirmCloseBox.Parm_Msg] = msg;
            paramMap[UI_ComfirmCloseBox.Parm_OK] = onClick;
            paramMap[UI_ComfirmCloseBox.Parm_Cancel] = OnCancel;
            return MessageBoxManager.Instance.ShowMessageBox(UI_ComfirmCloseBox.BOX_ID, paramMap,isUseBackground);
        }
        public void ShowSimpleMessageBox(string msg, UnityEngine.Events.UnityAction onClick = null)
        {
            Dictionary<string, object> paramMap = new Dictionary<string, object>();
            paramMap[SimpleBox.MSG_ID] = msg;
            paramMap[SimpleBox.OnOK_ID] = onClick;
            MessageBoxManager.Instance.ShowMessageBox(SimpleBox.BOX_ID, paramMap);
        }
        void Awake()
        {
            
            OnAllMessageBoxClosed=new Action(() => { });
            rectTran = GetComponent<RectTransform>();
            BackgroundOnOff(false);
            msgBoxInfoMap = new Dictionary<string, BaseMessageBox>();
            for (int i = 0; i < MessageBoxInfoList.Length; ++i)
            {
                msgBoxInfoMap.Add(MessageBoxInfoList[i].TypeName, MessageBoxInfoList[i].MessageBox);
            }
        }

        private void OnDestroy()
        {
            CancelInvoke("AfterCloseBoxShowNextBox");
        }
        
        public BaseMessageBox ShowMessageBox(string TypeName, Dictionary<string, object> dicParams = null, bool isUseBackground = true)
        {
            curMsgBox = msgBoxInfoMap[TypeName];
            return ShowMessageBox(curMsgBox, dicParams, isUseBackground);
        }
        public void BackgroundOnOff(bool IsOn)
        {
            Background.raycastTarget = IsOn;
            Background.enabled = IsOn;
        }
        public BaseMessageBox ShowMessageBox(BaseMessageBox boxPrefab,Dictionary<string, object> dicParams = null, bool isUseBackground = true)
        {
           // Debug.Log("1 ShowMessageBox clearNullBox");
            ClearNullBox();
            OnAnyMessageBoxOpened();
            Background.raycastTarget = true;
            paramMapping = dicParams;

            GameObject msgBox = Instantiate(boxPrefab.gameObject);
            msgBox.GetComponent<RectTransform>().SetParent(transform, false);
            msgBox.SetActive(false);

            BaseMessageBox messageBox = msgBox.GetComponent<BaseMessageBox>();
            messageBox.SetParam(dicParams);
            messageBox.isUsingBackground = isUseBackground;
            if (curOpendMessageBoxs.Count == 0)
            {
                CancelInvoke();

                curOpendMessageBoxs.Add(messageBox);
            }
            else
            {
                curOpendMessageBoxs.Add(messageBox);

            }
          
            Background.rectTransform.SetAsLastSibling();
               //Debug.Log("2 BG SetAsLastSibling");
            if (isUseBackground)
            {
                BackgroundOnOff(true);
                LeanTween.value(gameObject, new Color(0, 0, 0, 0), BackgroundColor, FadeTime).setOnUpdateColor(updateColor);
                rectTran.SetAsLastSibling();
                Invoke("onFadeInFinished", FadeTime);
            }
            else
            {
                BackgroundOnOff(false);
                onFadeInFinished();
            }
            messageBox.transform.SetAsLastSibling();
 //Debug.Log("3 show end");
            return messageBox;
        }
        public void SetBoxOrder(BaseMessageBox messageBox)
        {
            Background.rectTransform.SetAsLastSibling();
            messageBox.transform.SetAsLastSibling();
        }

        private void onFadeInFinished()
        {
            for(int i=0; i<curOpendMessageBoxs.Count; ++i)
            {
                if (!curOpendMessageBoxs[i].isOpen)
                {
                    curOpendMessageBoxs[i].gameObject.SetActive(true);
                    curOpendMessageBoxs[i].Open(this);
                }
            }
        }

        public void CloseAllOpenedBoxWithoutInvokeClosedFunc()
        {
           
           ClearNullBox();
            for(int i=curOpendMessageBoxs.Count - 1; i >= 0; --i)
            {
                curOpendMessageBoxs[i].Close(false);
            }
             BackgroundOnOff(false);

        }
        bool isBackgroundFadable()
        {
            foreach(var obj in  curOpendMessageBoxs)
            {
                if(obj.isUsingBackground)
                {
                    return false;
                }
            }
            return Background.enabled;
        }
        public void OnMessageBoxClosed(BaseMessageBox msgBox)
		{
            ClearNullBox();
          //  Debug.Log("1 開始關閉MessageBox");
            OnAnyMessageBoxClosed();
			curOpendMessageBoxs.Remove (msgBox);
			Destroy (msgBox.gameObject);
             Debug.Log("2消滅MessageBox");
			if (isBackgroundFadable()) 
			{

           //     Debug.Log("3 Message灰色背景開始消失");
                LeanTween.value(gameObject, BackgroundColor, new Color(0, 0, 0, 0), FadeTime).setOnUpdateColor(updateColor);
                Invoke ("AfterCloseBoxShowNextBox", FadeTime);
			}
            else
            {
                AfterCloseBoxShowNextBox();
            }
         //   Debug.Log("6 關閉視窗完成");
        }
        void ClearNullBox()
        {
            for (int i = curOpendMessageBoxs.Count - 1; i >= 0; --i)
            {
                if (curOpendMessageBoxs[i] == null)
                {
                    curOpendMessageBoxs.RemoveAt(i);
                }
            }
        }
        void AfterCloseBoxShowNextBox()
        {
          
            // Debug.Log("5重新排列MessageBox");
            if (curOpendMessageBoxs.Count > 0)
            {
                if(Background==null)
                {
                    Debug.LogError("Background==null");
                }

                Background.rectTransform.SetAsLastSibling();

                if(curOpendMessageBoxs[curOpendMessageBoxs.Count - 1]==null)
                {
                    Debug.LogError("curOpendMessageBoxs[curOpendMessageBoxs.Count - 1]==null");
                }
                
                curOpendMessageBoxs[curOpendMessageBoxs.Count - 1].transform.SetAsLastSibling();
            }
            if (curOpendMessageBoxs.Count == 0)
            {
                BackgroundOnOff(false);
               //  Debug.Log("6所有的MessageBox都關閉了");
                OnAllMessageBoxClosed();
            }
           //  Debug.Log("7完成關閉MessageBox");
        }

        private void updateColor(Color color)
        {
            Background.color = color;
        }
    }
}
