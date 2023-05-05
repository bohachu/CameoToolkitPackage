using System.Collections;
using System.Collections.Generic;
using Cameo;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Events;
using System.Threading.Tasks;
namespace Cameo
{
    [System.Serializable]
    public class DialogData
    {
        public string GroupID;
        //public int Index;
        public string Dialogue;
        //public int RoleIndex;
        public string RoleName; // 其實是角色ID不是name, 但是歷史因素，這邊沒有改

        public string BGImage;
        public string CenterImage;

        public string Audio; // audio file name or url

        public string ExtraParam; // josn format string for non standard data
    }
    [System.Serializable]
    public class DialogueDataSet
    {
        public enum PresetImageCommand
        {
            None,
            Show,
            Hide
        }
        public List<DialogData> dialogDatas;
        public List<DialogueSet> ConvertToDialogueSet(Dictionary<string, Sprite> Images)
        {
            Dictionary<string, DialogueSet> dialogueFilter = GetDialogueSetByGroupID(Images);
            List<DialogueSet> result = new List<DialogueSet>();
            foreach (var obj in dialogueFilter)
            {
                result.Add(obj.Value);
            }
            return result;
        }
        public Dictionary<string, DialogueSet> GetDialogueSetByGroupID(Dictionary<string, Sprite> Images)
        {
            Dictionary<string, DialogueSet> dialogueFilter = new Dictionary<string, DialogueSet>();
            DialogData preData = null;
            foreach (var obj in dialogDatas)
            {
                var oneDialogue = new DialogueActionUnit(obj);
                oneDialogue.characterExpression = (DialogueController.CharacterExpression)DialogueController.Instance.GetExpressionIndexByName(obj.RoleName);
                //遇到hide指令，就把圖片清空
                if (obj.BGImage.EndsWith(PresetImageCommand.Hide.ToString()))
                {
                    oneDialogue.BGImage = null;
                    oneDialogue.IsBGChange = true;
                }
                if (obj.CenterImage.EndsWith(PresetImageCommand.Hide.ToString()))
                {
                    oneDialogue.CenterImg = null;
                    oneDialogue.IsCenterImgChange = true;
                }

                if (!string.IsNullOrWhiteSpace(obj.BGImage))
                {
                    if(preData!=null && preData.BGImage!=obj.BGImage)
                        oneDialogue.IsBGChange = true;
                    if (Images.ContainsKey(obj.BGImage))
                    {
//                        Debug.Log("找到圖片，放入對白:" + obj.BGImage);
                        oneDialogue.BGImage = Images[obj.BGImage];
                    }
                }

                if (!string.IsNullOrWhiteSpace(obj.CenterImage))
                {
                    if (preData != null && preData.CenterImage != obj.CenterImage)
                    {
                        oneDialogue.IsCenterImgChange = true;
                        Debug.Log(obj.Dialogue+":圖片不同，替換前景圖片:" + obj.CenterImage);
                    }
                    if (Images.ContainsKey(obj.CenterImage))
                    {
                        oneDialogue.CenterImg = Images[obj.CenterImage];
                    }
                    else
                    {
                        //找不到圖片，可能是其他多媒體例如影片或是iframe
                        Debug.Log("找不到圖片，可能是其他多媒體例如影片或是iframe:" + obj.CenterImage);
                        oneDialogue.CenterImg = null;
                    }
                }


                if (dialogueFilter.ContainsKey(obj.GroupID))
                {
                    dialogueFilter[obj.GroupID].AdvanceDialogues.Add(oneDialogue);
                }
                else
                {
                    //Debug.Log("新增對話組:"+obj.GroupID); //每一組新的對話，都會重新設定背景圖片
                    DialogueSet oneSet = new DialogueSet();
                    oneSet.AdvanceDialogues.Add(oneDialogue);
                    dialogueFilter.Add(obj.GroupID, oneSet);
                }
                preData = obj;
            }
            return dialogueFilter;
        }
        public string SpreedSheetName;
        public string WorkSheetName;
        public async Task loadDataSets(string SpreadSheet, string WorkSheet, string UserAccount, string Token)
        {
            string url = string.Format("{0}/?{1}={2}&{3}={4}&{5}={6}.sheet&{7}={8}", FastAPISettings.BaseDataUrl,
                      FastAPISettings.AccountKey, UserAccount,
                      FastAPISettings.TokenKey, Token,
                      FastAPISettings.SpreadSheetKey, SpreadSheet,
                      FastAPISettings.WorkSheetKey, WorkSheet);
            try
            {

                string jsonStr = await FileRequestHelper.Instance.LoadJsonString(url);
                //Debug.Log("下載對白資料成功");
                //Debug.Log(jsonStr);
                dialogDatas = JsonConvert.DeserializeObject<List<DialogData>>(jsonStr);
                ConvertAllURL();
            }
            catch (System.Exception e)
            {
                Debug.LogError("下載對白資料失敗");
                Debug.LogError(url);
                Debug.LogError(e);
            }
        }
        void ConvertAllURL()
        {
            foreach (var obj in dialogDatas)
            {
                obj.Audio = ConvertRouterURL(obj.Audio);
                obj.BGImage = ConvertRouterURL(obj.BGImage);
                obj.CenterImage = ConvertRouterURL(obj.CenterImage);
            }
        }
        string ConvertRouterURL(string url)
        {
            return Cameo.Cameo_URLRouter.GetURL(url);
        }
    }
    [System.Serializable]
    public class DialogueSetLoader
    {
        [SerializeField]
        List<DialogueSet> StoryFlow;
        Dictionary<string, DialogueSet> DialogueSets;
        DialogueDataSet dialogueDownloadSets;
        void Setup(string SpreadSheetName, string WorksheetName)
        {
            dialogueDownloadSets = new DialogueDataSet();
            dialogueDownloadSets.SpreedSheetName = SpreadSheetName;
            dialogueDownloadSets.WorkSheetName = WorksheetName;
        }
        // Start is called before the first frame update
        public IEnumerator Initializ(string SpreadSheetName, string WorksheetName, string UserAccount, string Token)
        {
            Setup(SpreadSheetName, WorksheetName);
            yield return dialogueDownloadSets.loadDataSets(SpreadSheetName, WorksheetName, UserAccount, Token).AsIEnumerator();
            ImageDownloadHelper imageDownloadHelper = new ImageDownloadHelper();
            List<string> imageURL = new List<string>();
            foreach (var obj in dialogueDownloadSets.dialogDatas)
            {

                if (!string.IsNullOrEmpty(obj.BGImage) && (obj.BGImage != DialogueDataSet.PresetImageCommand.Hide.ToString()))
                {
                    //Debug.Log("下載對話背景圖片:" + obj.BGImage);
                    if (DialogueMultiMediaPlayer.isImage(obj.BGImage))
                        imageURL.Add(obj.BGImage);
                }
                if (!string.IsNullOrEmpty(obj.CenterImage) && (obj.CenterImage != DialogueDataSet.PresetImageCommand.Hide.ToString()))
                {
                    if (DialogueMultiMediaPlayer.isImage(obj.CenterImage))
                        imageURL.Add(obj.CenterImage);
                }
            }
            yield return imageDownloadHelper.DownloadImages(imageURL);
            DialogueSets = dialogueDownloadSets.GetDialogueSetByGroupID(imageDownloadHelper.LoadedImages);
        }

        public void ShowDialogue(string GroupID, UnityAction OnDialogueEnd = null)
        {
            if (string.IsNullOrWhiteSpace(GroupID))
            {
                Debug.Log("對話組ID為空，跳過對話直接執行下一步");
                if (OnDialogueEnd != null) OnDialogueEnd.Invoke();
                return;
            }
            if (!DialogueSets.ContainsKey(GroupID))
            {
                Debug.LogError("對話群組不存在，請檢查上搞系統：" + GroupID);
                if (OnDialogueEnd != null) OnDialogueEnd.Invoke();
                return;
            }
            //Debug.Log("啟動對話："+GroupID);
            if (OnDialogueEnd != null)
                DialogueSets[GroupID].StartDialogues(() =>
                {
                    DialogueSets[GroupID].Hide();
                    OnDialogueEnd.Invoke();
                });
            else
                DialogueSets[GroupID].StartDialogues();
        }

    }
}

