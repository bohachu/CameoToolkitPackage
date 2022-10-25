using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Cameo.UI;
public class UITool_StartLoadingProcess : BaseMessageBox
{
    [SerializeField]
    private Text curInitText; //目前loading log 顯示
    [SerializeField]
    private Text openingText;

    [SerializeField]
    private Button startButton;

    [SerializeField]
    private GameObject developVersionText;

    [SerializeField]
    private GameObject versionText;

    [SerializeField]
    private string loadingText = "載入遊戲資料中...";

    [SerializeField]
    private string startGameText = "按下任意處開始遊戲";
    public static BaseMessageBox ShowBox(BaseMessageBox prefab)
    {
        return MessageBoxManager.Instance.ShowMessageBox(prefab, null, false);
    }
    private void Awake()
    {
        openingText.gameObject.SetActive(false);

        startButton.gameObject.SetActive(false);
    }
    // Start is called before the first frame update
    public void StartLoading(bool isDevelopVersion,UnityAction OnStartButtonClickAction)
    {
        SetVersion(isDevelopVersion);
        openingText.text = loadingText;

        openingText.gameObject.SetActive(true);
        startButton.gameObject.SetActive(false);
        startButton.onClick.RemoveAllListeners();
        startButton.onClick.AddListener(OnStartButtonClickAction);
        startButton.onClick.AddListener(OnGameStart);
    }
    public void OnLoadEnd()
    {
        openingText.text = startGameText;
        openingText.gameObject.SetActive(true);
        startButton.gameObject.SetActive(true);
    }
    void SetVersion(bool isDevelopVersion)
    {
        versionText.gameObject.SetActive(!isDevelopVersion);

        developVersionText.gameObject.SetActive(isDevelopVersion);
    }
    public void SetInfo(string info)
    {
//        Debug.Log(info);
        curInitText.text = info;
    }
    void OnGameStart()
    {
        openingText.gameObject.SetActive(false);
        startButton.gameObject.SetActive(false);
    }
}
