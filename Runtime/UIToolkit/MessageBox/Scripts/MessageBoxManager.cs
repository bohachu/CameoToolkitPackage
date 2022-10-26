﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace Cameo.UI
{
    public class MessageBoxManager : Singleton<MessageBoxManager>
    {
        public Image Background;
        public MessageBoxInfo[] MessageBoxInfoList;
        public float FadeTime = 0.2f;
        public Color BackgroundColor = Color.black;

        private RectTransform rectTran;
        private List<BaseMessageBox> curOpendMessageBoxs = new List<BaseMessageBox>();
        private BaseMessageBox curMsgBox = null;
        private Dictionary<string, object> paramMapping;
        private Dictionary<string, BaseMessageBox> msgBoxInfoMap;
        public void ShowComfirmBox(string msg, UnityEngine.Events.UnityAction onClick, UnityEngine.Events.UnityAction OnCancel = null)
        {
            Dictionary<string, object> paramMap = new Dictionary<string, object>();
            paramMap[UI_ComfirmCloseBox.Parm_Msg] = msg;
            paramMap[UI_ComfirmCloseBox.Parm_OK] = onClick;
            paramMap[UI_ComfirmCloseBox.Parm_Cancel] = OnCancel;
            MessageBoxManager.Instance.ShowMessageBox(UI_ComfirmCloseBox.BOX_ID, paramMap);
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
            CancelInvoke("onFadeOutFinished");
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
            for(int i=curOpendMessageBoxs.Count - 1; i >= 0; --i)
            {
                curOpendMessageBoxs[i].Close(false);
            }
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
			curOpendMessageBoxs.Remove (msgBox);
			Destroy (msgBox.gameObject);

			if (isBackgroundFadable()) 
			{
                LeanTween.value(gameObject, BackgroundColor, new Color(0, 0, 0, 0), FadeTime).setOnUpdateColor(updateColor);
                Invoke ("onFadeOutFinished", FadeTime);
			}
            else
            {
                Background.rectTransform.SetAsLastSibling();
                if(curOpendMessageBoxs.Count>=1)
                    curOpendMessageBoxs[curOpendMessageBoxs.Count - 1].transform.SetAsLastSibling();
            }
        }

		private void onFadeOutFinished()
		{
            BackgroundOnOff(false);

		}

        private void updateColor(Color color)
        {
            Background.color = color;
        }
    }
}