using System.Collections;
using System.Collections.Generic;
using Cameo.UI;
using UnityEngine;

public class Page_SimplePage : BasePage
{
    private void onSwitch(string pageID)
    {
        pageManager.SwitchTo(pageID);
    }
    public void OnBackClicked()
    {
        pageManager.ToPrev();
    }
}
