using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Cameo;
using System;
public class StroyLoaderManager : Singleton<StroyLoaderManager>
{
    //紀錄已經下載過的劇情對話，避免重複下載
    Dictionary<string, DialogueSetLoader> dialogueSetLoaderDic = new Dictionary<string, DialogueSetLoader>();

    class DialogueIDSet
    {
        public string FileIndexID;
        public string GroupID;
        public  UnityAction OnComplete;
        public DialogueIDSet(string FileIndexID, string GroupID, UnityAction OnComplete = null)
        {
            this.FileIndexID = FileIndexID;
            this.GroupID = GroupID;
            this.OnComplete = OnComplete;
        }
        public bool isEqual(DialogueIDSet other)
        {
            return FileIndexID == other.FileIndexID && GroupID == other.GroupID;
        }
    }
    List<DialogueIDSet> QueuedDialogues = new List<DialogueIDSet>();
    bool isShowingDialogue = false;
    DialogueSetLoader curDialogueSetLoader;
    public enum StoryType
    {
        Start,
        Tutorial,
        MainStory,
        SideStory,
        End,
        SetName,
        AvatarSelect,
    }

    //顯示劇情對話，如果對話還沒下載完成，會先下載再顯示，如果對話已經下載完成，直接顯示，如果對話正在顯示，會先加入佇列，等對話結束後再顯示
    public void ShowStoryByGroupID(string FileIndexID, string GroupID, UnityAction OnComplete)
    {

        if (dialogueSetLoaderDic.ContainsKey(FileIndexID))
        {
            curDialogueSetLoader = dialogueSetLoaderDic[FileIndexID];
            ShowOrQueueDialogue(FileIndexID, GroupID, OnComplete);
        }
        else
            LoadStoryByID(FileIndexID, () =>
            {
                curDialogueSetLoader = dialogueSetLoaderDic[FileIndexID];
                ShowOrQueueDialogue(FileIndexID, GroupID, OnComplete);
            });
    }
    void ShowOrQueueDialogue(string FileIndexID, string GroupID, UnityAction OnComplete)
    {
        if (isShowingDialogue)
        {
            Debug.Log("對話正在顯示，加入佇列");
            QueuedDialogues.Add(new DialogueIDSet(FileIndexID, GroupID, OnComplete));
        }
        else
        {
            isShowingDialogue = true;
            ShowStoryByGroupID(FileIndexID, GroupID, ()=>{

                isShowingDialogue = false;
                if (OnComplete != null) OnComplete.Invoke();
                if (QueuedDialogues.Count > 0)
                {
                    Debug.Log("對話結束，繼續顯示佇列中的對話");
                    var nextDialogue = QueuedDialogues[0];
                    QueuedDialogues.RemoveAt(0);
                    ShowStoryByGroupID(nextDialogue.FileIndexID, nextDialogue.GroupID, nextDialogue.OnComplete);
                }
            });
        }
    }
    public void LoadStoryByID(string FileIndexID, Action OnComplete)
    {
        if (string.IsNullOrEmpty(FileIndexID))
        {
            Debug.Log("劇情對話ID為空");
            return;
        }
        StartCoroutine(LoadingCoroutine(FileIndexID, OnComplete));
    }
    public IEnumerator LoadingCoroutine(string FileIndexID = "GameStory", Action OnComplete = null)
    {
        //Debug.Log("開始下載劇情對話"+FileIndexID);
        var IndexData = DownloadInfoManager.Instance.DownloadInfo(FileIndexID);
        if (IndexData == null)
        {
            Debug.Log("劇情對話上稿ID錯誤：請檢查IndexData是否有此ID:" + FileIndexID);
            yield break;
        }
        if (dialogueSetLoaderDic == null)
            dialogueSetLoaderDic = new Dictionary<string, DialogueSetLoader>();
        if (dialogueSetLoaderDic.ContainsKey(FileIndexID))
        {
            curDialogueSetLoader = dialogueSetLoaderDic[FileIndexID];
            if (OnComplete != null) OnComplete.Invoke();
            yield break;
        }
        var dialogueSetLoader = new DialogueSetLoader();
        yield return curDialogueSetLoader.Initializ(IndexData.SpreadSheet, IndexData.WorkSheet, GlobalDataMediator.PlayerAccount, GlobalDataMediator.PlayerToken);
        dialogueSetLoaderDic.Add(FileIndexID, dialogueSetLoader);
        curDialogueSetLoader = dialogueSetLoader;

        //Debug.Log("下載劇情對話"+FileIndexID+"index:"+IndexData.SpreadSheet+" "+IndexData.WorkSheet);

        if (OnComplete != null) OnComplete.Invoke();
    }
}
