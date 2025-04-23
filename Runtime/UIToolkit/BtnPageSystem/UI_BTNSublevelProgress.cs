using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Cameo.UI;

// 需要把BTNMenuPage的private List<BTNUISet> buttons 改成 public

public class UI_BTNSublevelProgress : MonoBehaviour
{
    //001 public Image ActiveProgress; // 進度條的 Image 組件 
    [SerializeField]
    private Image ActiveProgress; // 進度條的 Image 組件

    void Start()
    {
        //Debug.Log("Starting SublevelProgress");
        // 獲取進度條的 Image 組件
        StartCoroutine(InitializeProgress());
    }

    private IEnumerator InitializeProgress()
    {
        //Debug.Log("進度條進度更新 - 開始協程");
        // 等待一幀確保所有 Start 方法被調用
        yield return null;
        //Debug.Log("進度條進度更新 - 繼續執行");
        // 獲取進度條的父 Button 組件
        Button parentButton = GetComponentInParent<Button>();

        Page_BTNMenuPage pageScript = transform.parent.parent.parent.parent.GetComponent<Page_BTNMenuPage>(); // 调整路径以指向正确的父对象
        if (pageScript != null && parentButton != null)
        {
            foreach (BTNUISet btnSet in pageScript.buttons)
            {
                // 檢查 BTNUISet 中的 button 是否為進度條的父 Button
                if (btnSet.button == parentButton)
                {
                    // 找到對應的 BTNUISet
                    StartCoroutine(UpdateProgress(btnSet));
                    break;
                }
            }
        }
        else
        {
            if (pageScript == null)
                Debug.Log("PageScript is null");
            if (parentButton == null)
                Debug.Log("ParentButton is null");
        }
    }
    //002 public IEnumerator UpdateProgress(BTNUISet button)
    public virtual IEnumerator UpdateProgress(BTNUISet button)
    {
        int progress = 0;
        float waitTime = 0f;
        float maxWaitTime = 1f; // 最大等待一秒
        string sublevelID = button.bntData.NextPageIndexID;

        // 等待直到 sublevelID 非空或達到最大等待時間
        while (string.IsNullOrEmpty(sublevelID) && waitTime < maxWaitTime)
        {
            yield return null; // 等待一幀
            waitTime += Time.deltaTime; // 更新等待的時間
            sublevelID = button.bntData.NextPageIndexID; // 重新檢查 sublevelID
        }

        //Debug.Log("NextPageIndexID: " + sublevelID);
        List<BTNData> sublevelData = UI_BTNDataManager.Instance.GetBTNData(sublevelID);
        for (int j = 0; j< sublevelData.Count; j++)
        {
            var sublevelProcess = UI_BTNDataManager.Instance.GetMissionData(sublevelID, sublevelData[j].BTNID);
            if (sublevelProcess == null || !sublevelProcess.isDone)
            {
                break;
            }
            progress += 1;
        }
        //Debug.Log(button.bntData.NextPageIndexID + " level progress: "+ progress);

        // 根據進度數據更新進度條
        ActivateProgress(progress);
    }
    //003 extract as method
    protected virtual void ActivateProgress(int progress)
    {
        // 獲取總進度數量
        int totalProgress = 3; // 預設值為3，保持與現有程式碼相容
        
        // 獲取父級按鈕
        Button parentButton = GetComponentInParent<Button>();
        if (parentButton != null)
        {
            Page_BTNMenuPage pageScript = transform.parent.parent.parent.parent.GetComponent<Page_BTNMenuPage>();
            if (pageScript != null)
            {
                foreach (BTNUISet btnSet in pageScript.buttons)
                {
                    if (btnSet.button == parentButton && btnSet.bntData != null)
                    {
                        string sublevelID = btnSet.bntData.NextPageIndexID;
                        if (!string.IsNullOrEmpty(sublevelID))
                        {
                            List<BTNData> sublevelData = UI_BTNDataManager.Instance.GetBTNData(sublevelID);
                            if (sublevelData != null)
                            {
                                totalProgress = sublevelData.Count;
                            }
                        }
                        break;
                    }
                }
            }
        }

        // 計算進度比例
        float progressRatio = totalProgress > 0 ? (float)progress / totalProgress : 0f;
        ActiveProgress.fillAmount = progressRatio;
    }
}