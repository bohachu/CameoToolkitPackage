using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cameo.UI;

public class DemoPageSwitcher : MonoBehaviour
{
    public Action<string> OnClickedCallback;

    [SerializeField]
    public string pageID;

    public void OnClick()
    {
        OnClickedCallback.Invoke(pageID);
    }
}


