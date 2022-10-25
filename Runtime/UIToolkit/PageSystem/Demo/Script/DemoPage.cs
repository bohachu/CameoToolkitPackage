using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cameo.UI;
using UnityEngine.UI;

public class DemoPage : BasePage
{
    public RawImage ImageComp;
    public Text TextComp;

    private void Start()
    {
        DemoPageSwitcher[] switchers = GetComponentsInChildren<DemoPageSwitcher>();
        for(int i=0; i<switchers.Length; ++i)
        {
            switchers[i].OnClickedCallback += onSwitch;
        }
    }

    private void onSwitch(string pageID)
    {
        pageManager.SwitchTo(pageID);
    }

    public void OnBackClicked()
    {
        pageManager.ToPrev();
    }

    public override void OnOpen()
    {
        if(paramMapping != null)
        {
            Texture2D image = (paramMapping["image"] != null) ? (Texture2D)paramMapping["image"] : null;
            if (ImageComp != null && image != null)
            {
                ImageComp.texture = image;
                ImageComp.SetNativeSize();
            }

            string text = (paramMapping["text"] != null) ? paramMapping["text"].ToString() : null;
            if (TextComp != null && text != null)
            {
                TextComp.text = text;
            }
        }
    }
}
