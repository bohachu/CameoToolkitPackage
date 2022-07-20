
using UnityEngine;
using Cameo;
using System.Threading.Tasks;
using LitJson;
using System.Collections;
namespace Cameo
{
    /// <summary>
    /// 依據目前運行的url來判斷要使用哪一個api domain,
    /// 藉此區分測試版本與正式版本兩版本不同的api
    /// </summary>
    public class FastAPIConfig : IConfigLoaderWithParams
    {
        [System.Serializable]
        public class UrlDef
        {
            public string APIDomain;
            public string GameDataUrl;
            public string LoginUrl;
        }
        private const string DefaultUrlKey = "Default";

        private const string UrlMapKey = "UrlMap";
        private UrlDef urlDefine = null;
        private string ClientUrl;
        private UrlDef parser(JsonData configJson)
        {
            UrlDef result = new UrlDef();
            if (configJson != null)
            {
                if (configJson.ContainsKey(UrlMapKey))
                {
                    JsonData urlDefJson = configJson[UrlMapKey][DefaultUrlKey];

                    result = JsonMapper.ToObject<UrlDef>(urlDefJson.ToJson());

                    foreach (string urlKey in configJson[UrlMapKey].Keys)
                    {
                        if (ClientUrl.StartsWith(urlKey))
                        {
                            urlDefJson = configJson[UrlMapKey][urlKey];

                            result = JsonMapper.ToObject<UrlDef>(urlDefJson.ToJson());

                            Debug.Log("url key: " + urlKey);

                            break;
                        }
                    }
                }
            }
            else
            {
                Debug.LogError("Load Config file failed!");
            }
            return result;
        }
        public IEnumerator InitializeCoroutine(string clientUrl)
        {
            yield return LoadWithParams(clientUrl).AsIEnumerator();
        }

        public async override Task LoadWithParams(object[] info)
        {
            ClientUrl = (string)info[0];
            urlDefine = await CongigTool.LoadConfig<UrlDef>(typeof(UrlDef).Name.ToString(), parser);
            FastAPISettings.BaseAPIUrl = urlDefine.APIDomain;
            FastAPISettings.LoginPageUrl = urlDefine.LoginUrl;
        }
    }

    public static class FastAPISettings
    {  //Api domain
        public static string BaseAPIUrl = "https://plant-hero.cameo.tw";
        //引導玩家登入的Url
        public static string LoginPageUrl = "https://adl.edu.tw/HomePage/login/?sys=planting";

        //fapi
        public static string BaseFapi { get { return BaseAPIUrl + "/fapi/"; } }

        //登入的Url(正式)
        public const string AccountKey = "str_user";
        public const string TokenKey = "str_token";
        public const string FileKey = "str_file";
        public const string ContentKey = "str_content";
        public const string TableKey = "str_table";
        public const string LogKey = "str_log";
        public const string ReadMessageListKey = "lst_message_id_set_true";
        public const string UnreadMessageListKey = "lst_message_id_set_false";
        //檔案索引檔的下載設定
        public const string DataIndexSpreadSheet = "FileIndex";
        public const string DataIndexWorkSheet = "Index";
        public const string DataIndexWorkSheetDevelop = "IndexDevelop";
        public const int DataIndexStartRow = 0;

        //下載遊戲資料回傳List of string格式
        public static string BaseListUrl { get { return BaseAPIUrl + "/sheet/get_all_values"; } }

        //下載遊戲資料的Url(from google sheet)
        public static string BaseDataUrl { get { return BaseAPIUrl + "/sheet/get_after_2_rows"; } }
        public const string SpreadSheetKey = "str_spreadsheet";
        public const string WorkSheetKey = "str_worksheet";

        //下載Message
        public static string SetMessageReadUrl { get { return BaseAPIUrl + "/message/set_messages_read/"; } }

        //Log url
        public static string LogUploadUrl { get { return BaseAPIUrl + "/log/web_log/"; } }
        public static string GetRequestUrl { get { return BaseAPIUrl + "/key_value/get"; } }

        //上傳玩家資料的Url
        public static string SetRequestUrl { get { return BaseAPIUrl + "/key_value/set/"; } }
        public static string UploadFileUrl { get { return BaseAPIUrl + "/upload/upload/?str_directory="; } }
        public const string UploadFileKey = "lst_files";

    }

}
