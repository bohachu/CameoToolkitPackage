using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using LitJson;

namespace Cameo
{
    public class DownloadInfoManager : Singleton<DownloadInfoManager>
    {
      
        private Dictionary<string, DownloadInfo> indexMap;

        private bool isDevelopVersion = false;
        private bool isLoaded = false;
       
        public IEnumerator InitializeCoroutine(string userAccount , string token,bool isDevelopVersion)
        {
            
            if (!isLoaded||this.isDevelopVersion != isDevelopVersion)
            {
                this.isDevelopVersion = isDevelopVersion;
                yield return runAsyncTask(userAccount, token).AsIEnumerator();
            }
                
            else yield return null;
        }

        private async Task runAsyncTask(string userAccount, string token)
        {
            DownloadInfo[] dataIndices = await FileRequestHelper.Instance.LoadArray<DownloadInfo>(FastAPISettings.DataIndexSpreadSheet,
                isDevelopVersion ? FastAPISettings.DataIndexWorkSheetDevelop : FastAPISettings.DataIndexWorkSheet,
                FastAPISettings.DataIndexStartRow, parser, userAccount, token);

            indexMap = new Dictionary<string, DownloadInfo>();
            foreach (DownloadInfo dataIndex in dataIndices)
            {
                if (dataIndex == null)
                {
                    Debug.LogWarning("FileIndex sheet might has blank cells or wrong format");
                    continue;
                }
                indexMap[dataIndex.ID] = dataIndex;
            }
            isLoaded = true;
        }

        private DownloadInfo parser(JsonData jsonData)
        {
            DownloadInfo newinfo = new DownloadInfo() ;

            newinfo.ID = (string)jsonData["ID"];
            newinfo.SpreadSheet = (string)jsonData["SpreadSheet"];
            newinfo.WorkSheet = (string)jsonData["WorkSheet"];
            newinfo.StartRow = int.Parse((string)jsonData["StartRow"]);
            return newinfo;
        }

        public DownloadInfo DownloadInfo(string downloadInfoID)
        {
            return (indexMap != null && indexMap.ContainsKey(downloadInfoID)) ? indexMap[downloadInfoID] : null;
        }
    }

    [SerializeField]
    public class DownloadInfo
    {
        public string ID;
        public string SpreadSheet;
        public string WorkSheet;
        public int StartRow;
        public DownloadInfo()
        {
            
        }
    }
}
