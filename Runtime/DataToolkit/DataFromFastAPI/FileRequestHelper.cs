using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using LitJson;
using System;
using UnityEngine.Networking;

namespace Cameo
{
    public class FileRequestHelper :Singleton<FileRequestHelper>
    {
       
        public async Task<T> LoadJson<T>(string url) where T : class
        {
            //Debug.Log("LoadJson: " + url);

            UnityWebRequest www = new UnityWebRequest(url);

            www.downloadHandler = new DownloadHandlerBuffer();

            await www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogErrorFormat(www.error);
                return null;
            }
            else
            {
                try
                {
                    return JsonMapper.ToObject<T>(www.downloadHandler.text);
                }
                catch(Exception e)
                {
                    Debug.LogFormat("Load {0} error: {1},{2}", url, www.downloadHandler.text, e);

                    return null;
                } 
            }
        }

        public async Task<T> LoadJson<T>(string url, Func<JsonData, T> parser) where T : class
        {
            //Debug.Log("LoadJson with parser: " + url);

            UnityWebRequest www = new UnityWebRequest(url);

            www.downloadHandler = new DownloadHandlerBuffer();

            await www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogErrorFormat(www.error);
                return null;
            }
            else
            {
                //ebug.Log("size: " + (www.downloadHandler.data.Length / 1000).ToString() + "kb");
                //Debug.Log(www.downloadHandler.text);
                return parser(JsonMapper.ToObject(www.downloadHandler.text));
            }
        }

        public async Task<string> LoadJsonString(string url)
        {
            //Debug.Log(url);

            UnityWebRequest www = new UnityWebRequest(url);

            www.downloadHandler = new DownloadHandlerBuffer();

            await www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
                return null;
            }
            else
            {
                //Debug.Log("size: " + (www.downloadHandler.data.Length / 1000).ToString() + "kb");
                return www.downloadHandler.text;
            }
        }

        public async Task InvokeAPI(string url)
        {
            //Debug.Log(url);

            UnityWebRequest www = new UnityWebRequest(url);

            await www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
            }
        }

        public async Task<RequestResult> InvokeAPI<T>(string url, Func<JsonData, T> parser) where T:class
        {
            //Debug.Log(url);

            UnityWebRequest www = new UnityWebRequest(url);

            www.downloadHandler = new DownloadHandlerBuffer();

            await www.SendWebRequest();

            RequestResult result = new RequestResult();

            if (www.result != UnityWebRequest.Result.Success)
            {
                result.Content = null;

                result.ErrorMsg = www.error;

                Debug.Log(www.error);
            }
            else
            {
                result.Content = parser(JsonMapper.ToObject(www.downloadHandler.text));

                result.ErrorMsg = "";
            }

            return result;
        }

        //以JsonObject Array方式載入(含Key)
        public async Task<T[]> LoadArray<T>(string spreadSheet, string workSheet, int index, Func<JsonData, T> parser, string userAccount,string token) where T : class
        {
            T[] returnArray = null;

            string url = string.Format("{0}/?{1}={2}&{3}={4}&{5}={6}.sheet&{7}={8}", Settings.BaseDataUrl,
                Settings.AccountKey, userAccount,
                Settings.TokenKey, token,
                Settings.SpreadSheetKey, spreadSheet,
                Settings.WorkSheetKey, workSheet);

            UnityWebRequest www = new UnityWebRequest(url);

            www.downloadHandler = new DownloadHandlerBuffer();

            await www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogErrorFormat(www.error);

                Debug.Log("target url : " + url);
                Debug.Log("可能是user id與user token驗證失敗");
            }
            else
            {
                //Debug.Log("size: " + (www.downloadHandler.data.Length / 1000).ToString() + "kb");

                JsonData jsonData = JsonMapper.ToObject(www.downloadHandler.text);

                returnArray = new T[jsonData.Count - index];

                for (int i = index; i < jsonData.Count; i++)
                {
                    T obj = parser(jsonData[i]);
                    returnArray[i - index] = obj;
                }
            }

            return returnArray;
        }

        //以每row一個array的方式載入(不含Key，較省資源，但是必須知道一個物件的variable來自array第幾個元素)
        public async Task<T[]> LoadArrayFromStringArray<T>(string spreadSheet, string workSheet, int index, Func<string[], T> parser, string UserAccount, string Token) where T : class
        {
            T[] returnArray = null;
            List<T> dataArray = new List<T>();
            string url = string.Format("{0}/?{1}={2}&{3}={4}&{5}={6}.sheet&{7}={8}", Settings.BaseListUrl,
                Settings.AccountKey, UserAccount,
                Settings.TokenKey, Token,
                Settings.SpreadSheetKey, spreadSheet,
                Settings.WorkSheetKey, workSheet);

          //  Debug.Log(url);

            UnityWebRequest www = new UnityWebRequest(url);

            www.downloadHandler = new DownloadHandlerBuffer();

            await www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogErrorFormat(www.error);
            }
            else
            {
                //Debug.Log("size: " + (www.downloadHandler.data.Length / 1000).ToString() + "kb");

                string[][] data = JsonMapper.ToObject<string[][]>(www.downloadHandler.text);
                
                returnArray = new T[data.Length - index];

                for (int i = index; i < data.Length; i++)
                {
                    try
                    {
                        T obj = parser(data[i]);
                        returnArray[i - index] = obj;
                        dataArray.Add(obj);
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError("表單格式錯誤"+ e.ToString());
                        for (int ii = 0; i < data.Length; i++)
                        {
                            Debug.Log(data[ii]);
                        }
                    }
                }
                data = null;
                GC.Collect();
            }
            returnArray = dataArray.ToArray();
            return returnArray;
        }

        public async Task<string> UploadGameData(string fileName, object data, string UserAccount, string Token)
        {
            UnityWebRequest www = new UnityWebRequest(Settings.SetRequestUrl, "POST");

            Dictionary<string, string> requestBody = new Dictionary<string, string>();
            requestBody[Settings.AccountKey] = UserAccount; 
            requestBody[Settings.TokenKey] = Token;
            requestBody[Settings.FileKey] = fileName;
            requestBody[Settings.ContentKey] = JsonMapper.ToJson(data);

            string jsonStr = JsonMapper.ToJson(requestBody);

            byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(jsonStr);

            www.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
            www.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            await www.SendWebRequest();
            jsonToSend = null;
            GC.Collect();
            if (www.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.Log("Error While Sending: " + www.error);
                return www.error;
            }
            else
            {
                //Debug.Log("Received: " + www.downloadHandler.text);
                return null;
            }
            
        }

        public async Task<string> UploadMessageData(List<string> readedMessageIDs, List<string> unreadedMessageIDs,string UserAccount, string Token)
        {
            UnityWebRequest www = new UnityWebRequest(Settings.SetMessageReadUrl, "POST");

            Dictionary<string, object> requestBody = new Dictionary<string, object>();
            requestBody[Settings.AccountKey] = UserAccount;
            requestBody[Settings.TokenKey] = Token;
            requestBody[Settings.ReadMessageListKey] = readedMessageIDs;
            requestBody[Settings.UnreadMessageListKey] = unreadedMessageIDs;

            string jsonStr = JsonMapper.ToJson(requestBody);

            byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(jsonStr);

            www.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
            www.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            await www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.Log("Error While Sending: " + www.error);
                return www.error;
            }
            else
            {
                //Debug.Log("Received: " + www.downloadHandler.text);
                return null;
            }
        }

        public async Task<string> UploadLog(string logTable, string logs, string UserAccount, string Token)
        {
            UnityWebRequest www = new UnityWebRequest(Settings.LogUploadUrl, "POST");

            Dictionary<string, object> requestBody = new Dictionary<string, object>();
            requestBody[Settings.AccountKey] = UserAccount;
            requestBody[Settings.TokenKey] = Token;
            requestBody[Settings.TableKey] = logTable;
            requestBody[Settings.LogKey] = logs;

            string jsonStr = JsonMapper.ToJson(requestBody);

            byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(jsonStr);

            www.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
            www.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            await www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.Log("Error While Sending: " + www.error);
                return www.error;
            }
            else
            {
                //Debug.Log("Received: " + www.downloadHandler.text);
                return null;
            }
        }

        public async Task<string> UploadImage(byte[] file, string folder, string fileName)
        {
            string url = string.Format("{0}{1}/", Settings.UploadFileUrl, folder);

            WWWForm form = new WWWForm();

            form.AddBinaryData(Settings.UploadFileKey, file, fileName, "image/jpeg");

            UnityWebRequest www = UnityWebRequest.Post(url, form);

            await www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.Log("Error While Sending: " + www.error);
                return www.error;
            }
            else
            {
                //Debug.Log("Received: " + www.downloadHandler.text);
                return null;
            }
        }

        public async Task<Texture2D> DownloadImage(string url)
        {
            UnityWebRequest www = new UnityWebRequest(url, "GET");

            www.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();

            www.SetRequestHeader("Content-Type", "image/jpeg");

            await www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.Log("Error While Sending: " + www.error);

                return null;
            }
            else if(www.downloadHandler.text == "Not Found")
            {
                //Debug.Log("Download image " + www.downloadHandler.text);

                return null;
            }
            else
            {
                //Debug.Log("Received: " + www.downloadHandler.text);

                Texture2D tex = new Texture2D(2, 2);

                tex.LoadImage(www.downloadHandler.data, false);

                return tex;
            }
        }

        #region FAPI

        /// <summary>
        /// 取得連續Json檔案
        /// </summary>
        public async Task<T[]> LoadJsonList<T>(string[] paths, Func<JsonData, T> parser) where T:class
        {
            T[] returnArray = null;

            UnityWebRequest www = new UnityWebRequest(Settings.BaseFapi, "POST");

            Dictionary<string, object> requestBody = new Dictionary<string, object>();
            requestBody["command"] = "file";
            requestBody["action"] = "read-files-not-null";
            requestBody["paths"] = paths;

            string jsonStr = JsonMapper.ToJson(requestBody, false);

            Debug.Log(www.url);
            Debug.Log(jsonStr);

            byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(jsonStr);

            www.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
            www.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            await www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
            }
            else if (www.downloadHandler.text == "null")
            {
                Debug.Log("Get null from server");
            }
            else
            {
                //Debug.Log("size: " + (www.downloadHandler.data.Length / 1000).ToString() + "kb");
                
                JsonData jsonData = JsonMapper.ToObject(www.downloadHandler.text);

                returnArray = new T[paths.Length];

                for (int i = 0; i < paths.Length; ++i)
                {
                    returnArray[i] = parser(JsonMapper.ToObject(jsonData[i].ToString()));
                }
            }

            return returnArray;
        }

        /// <summary>
        /// 取得不重複key資料的最後一筆
        /// </summary>
        public async Task<T[]> LoadArrayGroupLast<T>(string path, string[] columns, Func<string[], T> parser) where T:class
        {
            T[] returnArray = null;

            UnityWebRequest www = new UnityWebRequest(Settings.BaseFapi, "POST");

            Dictionary<string, object> requestBody = new Dictionary<string, object>();
            requestBody["command"] = "csv";
            requestBody["action"] = "group-last";
            requestBody["path"] = path;
            requestBody["columns"] = columns;

            string jsonStr = JsonMapper.ToJson(requestBody, false);

            Debug.Log(www.url);
            Debug.Log(jsonStr);

            byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(jsonStr);

            www.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
            www.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            await www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
            }
            else
            {
                //Debug.Log("size: " + (www.downloadHandler.data.Length / 1000).ToString() + "kb");

                JsonData jsonData = JsonMapper.ToObject(www.downloadHandler.text);

                //Debug.Log(www.downloadHandler.text);

                returnArray = new T[jsonData.Count];

                for (int i = 0; i < returnArray.Length; ++i)
                {
                    Debug.Log(jsonData[i].ToString());
                    returnArray[i] = parser(JsonMapper.ToObject<string[]>(jsonData[i].ToJson()));
                }
            }

            return returnArray;
        }

        /// <summary>
        /// 針對csv寫入一筆資料
        /// </summary>
        /// <param name="path">csv path</param>
        /// <param name="columns">columns名稱</param>
        /// <param name="values">對應column的值</param>
        /// <returns></returns>
        public async Task<string> AppendAndCreate(string path, string[] columns, string[] values)
        {
            UnityWebRequest www = new UnityWebRequest(Settings.BaseFapi, "POST");

            Dictionary<string, object> requestBody = new Dictionary<string, object>();
            requestBody["command"] = "csv";
            requestBody["action"] = "create-append";
            requestBody["path"] = path;
            requestBody["columns"] = columns;
            requestBody["values"] = values;

            string jsonStr = JsonMapper.ToJson(requestBody, false);

            Debug.Log(www.url);
            Debug.Log(jsonStr);

            byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(jsonStr);

            www.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);

            www.SetRequestHeader("Content-Type", "application/json");

            await www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);

                return www.error;
            }
            else
            {
                return null;
            }
        }
        #endregion
    }

    public class RequestResult
    {
        public object Content;

        public string ErrorMsg;

        public bool IsSuccess
        {
            get
            {
                return string.IsNullOrEmpty(ErrorMsg);
            }
        }
    }
}
