using System.Collections;
using System.Collections.Generic;
using Cameo;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Events;
using System.Threading.Tasks;
using System.Linq;

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
            int uniqueGroupCount = dialogDatas.Select(x => x.GroupID).Distinct().Count();
            Dictionary<string, DialogueSet> dialogueFilter = new Dictionary<string, DialogueSet>(uniqueGroupCount);
            DialogData preData = null;
            foreach (DialogData obj in dialogDatas)
            {
                DialogueActionUnit oneDialogue = new DialogueActionUnit(obj);
                oneDialogue.characterExpression = (DialogueController.CharacterExpression)DialogueController.Instance.GetExpressionIndexByName(obj.RoleName);
                //遇到hide指令，就把圖片清空
               
                ProcessBackgroundImage(obj, preData, oneDialogue, Images);
                ProcessCenterImage(obj, preData, oneDialogue, Images);

                if (!dialogueFilter.TryGetValue(obj.GroupID, out var dialogueSet))
                {
                    dialogueSet = new DialogueSet();
                    dialogueFilter.Add(obj.GroupID, dialogueSet);
                }
                dialogueSet.AdvanceDialogues.Add(oneDialogue);
                preData = obj;
            }
            return dialogueFilter;
        }

        private void ProcessBackgroundImage(DialogData obj, DialogData preData, DialogueActionUnit oneDialogue, Dictionary<string, Sprite> Images)
        {
            if(string.IsNullOrWhiteSpace(obj.BGImage))
                return;
            
            if (obj.BGImage.EndsWith(PresetImageCommand.Hide.ToString()))
            {
                oneDialogue.BGImage = null;
                oneDialogue.IsBGChange = true;
            }
            oneDialogue.IsBGChange = preData == null || preData.BGImage!=obj.BGImage;
                
            if (Images.TryGetValue(obj.BGImage, out var sprite))
            {
            //     Debug.Log(obj.Dialogue+"，對白找到背景圖片:" + obj.BGImage);
                oneDialogue.BGImage = sprite;
            }
        }

        private void ProcessCenterImage(DialogData obj, DialogData preData, DialogueActionUnit oneDialogue, Dictionary<string, Sprite> Images)
        {
            if(string.IsNullOrWhiteSpace(obj.CenterImage))
                return;
            
            if (obj.CenterImage.EndsWith(PresetImageCommand.Hide.ToString()))
            {
                oneDialogue.CenterImg = null;
                oneDialogue.IsCenterImgChange = true;
            }
            oneDialogue.IsCenterImgChange = preData == null || preData.CenterImage!=obj.CenterImage;
                
            if (Images.TryGetValue(obj.CenterImage, out var sprite))
            {
                oneDialogue.CenterImg = sprite;
            }
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
#if UNITY_EDITOR
            Debug.Log(url);
#endif
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
                Debug.LogError("下載對白資料失敗："+url);
                Debug.LogError(e);
            }
        }
        void ConvertAllURL()
        {
            foreach (var obj in dialogDatas)
            {
                if(obj.Audio!=null)
                    obj.Audio = ConvertRouterURL(obj.Audio);
                if(obj.BGImage!=null)
                    obj.BGImage = ConvertRouterURL(obj.BGImage);
                if(obj.CenterImage!=null)
                    obj.CenterImage = ConvertRouterURL(obj.CenterImage);
            }
        }
        string ConvertRouterURL(string url)
        {
            if(isIframe(url)) return url;
            return Cameo.Cameo_URLRouter.GetURL(url);
        }
        bool isIframe(string url)
        {
            //如果有包含iframe 就不轉換
//            Debug.Log("檢查是否為iframe:"+url+"如果有包含iframe字串不轉換domaine");
            return url.Contains("iframe");
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
                   
                    if (DialogueMultiMediaPlayer.isImage(obj.BGImage))
                    {
                        Debug.Log("下載對話背景圖片:" + obj.BGImage);
                         imageURL.Add(obj.BGImage);
                    }
                        
                }
                if (!string.IsNullOrEmpty(obj.CenterImage) && (obj.CenterImage != DialogueDataSet.PresetImageCommand.Hide.ToString()))
                {
                    if (DialogueMultiMediaPlayer.isImage(obj.CenterImage))
                    {
                        imageURL.Add(obj.CenterImage);
               //         Debug.Log("下載對話前景圖片:" + obj.CenterImage);
                    }
                        
                }
            }
            yield return imageDownloadHelper.DownloadImages(imageURL);
            Debug.Log("下載圖片完成");
            DialogueSets = dialogueDownloadSets.GetDialogueSetByGroupID(imageDownloadHelper.LoadedImages);
            Debug.Log("圖片 ID 設置完成");
        }

        public void ShowDialogue(string GroupID, UnityAction OnDialogueEnd = null)
        {
            DialogueController.Instance.Reset(); // 重置對話系統
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
                    DialogueController.Instance.Reset();// 不能在這裡reset, 會導致最後一句對話沒有顯示畫面
                    DialogueSets[GroupID].Hide();
                    OnDialogueEnd.Invoke();
                    //Debug.Log("對話結束:"+GroupID);
                    

                });
            else
                DialogueSets[GroupID].StartDialogues();
        }

    }
}

