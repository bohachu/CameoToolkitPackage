using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Cameo.UI;
public class UITool_GameLoadingProcess : BaseMessageBox
{
    [SerializeField]
    private Text progressText; //目前loading log 顯示
    [SerializeField]
    private Text titleText;
    [SerializeField]
    private ProgressBarController progressBarController;
    [SerializeField]
    private string loadingText = "載入遊戲資料中...";

    public static BaseMessageBox ShowBox(BaseMessageBox prefab)
    {
        return MessageBoxManager.Instance.ShowMessageBox(prefab, null, false);
    }
    private void Awake()
    {
        titleText.gameObject.SetActive(false);
    }
    // Start is called before the first frame update
    private void Start()
    {
        titleText.text = loadingText;

        titleText.gameObject.SetActive(true);
    }
    public void OnLoadEnd()
    {
        titleText.gameObject.SetActive(true);
    }
    public void SetInfoProgress(string info, float progress)
    {
        SetInfo(info);
        progressBarController.SetProgress(progress);
    }
    public void SetInfo(string info)
    {
        progressText.text = info;
    }
   
}
