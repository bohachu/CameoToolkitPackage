using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Events;
using Cameo.UI;
using Cameo;
using UnityEngine.UI;

public class SimpleBox : BaseMessageBox
{
    public const string BOX_ID = "SimpleDialog";
    public const string MSG_ID = "message";

    public const string OnOK_ID = "ActionOnClick";
    public Text textComp;
    private UnityAction actionClick;
    protected override void onOpen()
    {
        if(paramMapping != null)
        {
            textComp.text = paramMapping[MSG_ID].ToString();
            actionClick = (UnityAction)paramMapping[OnOK_ID];
        }
    }

    protected override void onClose()
    {
        if(actionClick!=null)
        {
            Debug.Log("actionClick is click");
            actionClick.Invoke();
        }
        else
        {

            Debug.Log("actionClick is null");
        }
        SystemAudioCenter.Instance.PlayOneShot(AudioClipType.CommonUIButton);
    }
}
