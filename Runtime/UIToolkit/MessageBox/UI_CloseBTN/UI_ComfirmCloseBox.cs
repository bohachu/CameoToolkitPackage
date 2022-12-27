using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Cameo.UI;
namespace Cameo.UI
{
    public class UI_ComfirmCloseBox : BaseMessageBox
    {
        [SerializeField]
        Text Message;
        [SerializeField]
        Button comfirmBTN;
        [SerializeField]
        Button cancelBTN;
        UnityAction OnOK;
        UnityAction OnCancel;
        public const string Parm_OK = "Parm_OK";

        public const string Parm_Cancel = "Parm_Cancel";

        public const string Parm_Msg = "Parm_Msg";
        public const string BOX_ID = "UI_ComfirmCloseBox";
        public static UI_ComfirmCloseBox ShowBox(UI_ComfirmCloseBox prefab, UnityAction OnOk, UnityAction OnCancel = null, string text = "")
        {
            Dictionary<string, object> param = new Dictionary<string, object>();
            param.Add(Parm_OK, OnOk);
            param.Add(Parm_Cancel, OnCancel);
            param.Add(Parm_Msg, text);
            return (UI_ComfirmCloseBox) MessageBoxManager.Instance.ShowMessageBox(prefab, param);

        }
        public void SetOKBTNEnable(bool enable)
        {
            comfirmBTN.interactable = enable;
        }
        public void SetMsg(string msg)
        {
            Message.text = msg;
        }
        protected override void onOpen()
        {
            string msg = (string)paramMapping[Parm_Msg];
            if (!string.IsNullOrEmpty(msg))
                Message.text = msg;
            OnOK = (UnityAction)paramMapping[Parm_OK];
            OnCancel = (UnityAction)paramMapping[Parm_Cancel];
            comfirmBTN.onClick.RemoveAllListeners();
            comfirmBTN.onClick.AddListener(
                () => {
                    OnOK.Invoke();
                    Close();
                });
            cancelBTN.onClick.RemoveAllListeners();
            cancelBTN.onClick.AddListener(
                () => {
                    if(OnCancel!=null)
                        OnCancel.Invoke();
                    Close();
                });
        }

    }

}