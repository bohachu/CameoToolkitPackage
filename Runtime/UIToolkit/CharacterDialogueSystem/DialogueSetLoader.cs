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
        public int Index;
        public string Dialogue;
        //public int RoleIndex;
        public string RoleName;
    }
    [System.Serializable]
    public class DialogueDataSet
    {
        public List<DialogData> dialogDatas;
        public List<DialogueSet> ConvertToDialogueSet()
        {
            Dictionary<string, DialogueSet> dialogueFilter = GetDialogueSetByGroupID();
            List <DialogueSet> result = new List<DialogueSet>();
            foreach (var obj in dialogueFilter)
            {
                result.Add(obj.Value);
            }
            return result;
        }
        public Dictionary<string, DialogueSet> GetDialogueSetByGroupID()
        {
            Dictionary<string, DialogueSet> dialogueFilter = new Dictionary<string, DialogueSet>();
            foreach (var obj in dialogDatas)
            {
                var oneDialogue = new DialogueActionUnit(obj.Dialogue);
                oneDialogue.characterExpression = (DialogueController.CharacterExpression)DialogueController.Instance.GetExpressionIndexByName(obj.RoleName);

                if (dialogueFilter.ContainsKey(obj.GroupID))
                {
                    dialogueFilter[obj.GroupID].AdvanceDialogues.Add(oneDialogue);
                }
                else
                {
                    DialogueSet oneSet = new DialogueSet();
                    oneSet.AdvanceDialogues.Add(oneDialogue);
                    dialogueFilter.Add(obj.GroupID, oneSet);
                }
            }
            return dialogueFilter;
        }
        public string SpreedSheetName;
        public string WorkSheetName;
        public async Task loadDataSets(string SpreadSheet, string WorkSheet,string UserAccount, string Token)
        {
            try
            {
                string url = string.Format("{0}/?{1}={2}&{3}={4}&{5}={6}.sheet&{7}={8}", FastAPISettings.BaseDataUrl,
                        FastAPISettings.AccountKey, UserAccount,
                        FastAPISettings.TokenKey, Token,
                        FastAPISettings.SpreadSheetKey, SpreadSheet,
                        FastAPISettings.WorkSheetKey, WorkSheet);
                string jsonStr = await FileRequestHelper.Instance.LoadJsonString(url);

                dialogDatas = JsonConvert.DeserializeObject<List<DialogData>>(jsonStr);

            }
            catch (System.Exception e)
            {
                Debug.LogError(e);
            }
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
            yield return dialogueDownloadSets.loadDataSets(SpreadSheetName, WorksheetName, UserAccount, Token);
            DialogueSets = dialogueDownloadSets.GetDialogueSetByGroupID();
        }
  
        public void ShowDialogue(string GroupID, UnityAction OnDialogueEnd)
        {
            //Debug.Log("啟動對話："+GroupID);
            DialogueSets[GroupID].StartDialogues(() => {
                DialogueSets[GroupID].Hide();
                OnDialogueEnd.Invoke();
            });
        }

    }
}

