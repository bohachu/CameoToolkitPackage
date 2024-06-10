using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Cameo.UI;


// 需要把BTNMenuPage的private List<BTNUISet> buttons 改成 public

public class UI_BTNSublevelProgress : MonoBehaviour
{
    //001 public Image ActiveProgress; // 进度条的 Image 组件 
    [SerializeField]
    private Image ActiveProgress; // 进度条的 Image 组件

    void Start()
    {
        //Debug.Log("Starting SublevelProgress");
        // 获取进度条的 Image 组件
        StartCoroutine(InitializeProgress());
    }

    private IEnumerator InitializeProgress()
    {
        //Debug.Log("進度條進度更新 - 開始協程");
        // 等待一帧确保所有 Start 方法被调用
        yield return null;
        //Debug.Log("進度條進度更新 - 繼續執行");
        // 获取进度条的父 Button 组件
        Button parentButton = GetComponentInParent<Button>();

        Page_BTNMenuPage pageScript = transform.parent.parent.parent.parent.GetComponent<Page_BTNMenuPage>(); // 调整路径以指向正确的父对象
        if (pageScript != null && parentButton != null)
        {
            foreach (BTNUISet btnSet in pageScript.buttons)
            {
                // 检查 BTNUISet 中的 button 是否为进度条的父 Button
                if (btnSet.button == parentButton)
                {
                    // 找到对应的 BTNUISet
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

        // 根据进度数据更新进度条
        ActivateProgress(progress);
    }
    //003 extract as method
    protected virtual void ActivateProgress(int progress)
    {
        switch (progress)
        {
            case 1:
                ActiveProgress.fillAmount = 0.33f; // 进度为 1 时显示 1/3
                break;
            case 2:
                ActiveProgress.fillAmount = 0.67f; // 进度为 2 时显示 2/3
                break;
            case 3:
                ActiveProgress.fillAmount = 1.0f; // 进度为 3 时显示完整进度
                break;
            default:
                ActiveProgress.fillAmount = 0f; // 默认情况，无进度
                break;
        }
    }
}