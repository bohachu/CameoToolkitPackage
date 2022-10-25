using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Cameo;
namespace Cameo.UI
{
    public class UI_CloseTool : Singleton<UI_CloseTool>
    {
        [SerializeField]
        Button CloseBtn;
        [SerializeField]
        UI_ComfirmCloseBox UI_ComfirmClose;
        UnityAction OnComfirmClose;
       
        public void InitTool(UnityAction CloseComfirmAction)
        {
            OnComfirmClose = CloseComfirmAction;
            CloseBtn.onClick.RemoveAllListeners();
            CloseBtn.onClick.AddListener(CloseClicked);
            ShowHide(true);
        }
        public void ShowHide(bool isShow)
        {
            CloseBtn.gameObject.SetActive(isShow);
        }
        void CloseClicked()
        {
            UI_ComfirmCloseBox.ShowBox(UI_ComfirmClose, () => {
                OnComfirmClose.Invoke();
            }, () =>
            {
                //取消，不作反應
            });
        }
       
    }
}

