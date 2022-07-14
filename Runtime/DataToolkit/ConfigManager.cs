using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Threading.Tasks;
using LitJson;
using System;
namespace Cameo
{
    /// <summary>
    /// 讀取位於Streamming assets目錄底下的設定檔案
    /// 檔案名稱建議符合class name
    /// </summary>
    public class CongigTool
    {
        public static async Task<T> LoadConfig<T>(string configName, Func<JsonData, T> parser) where T : class
        {
            T returnConfig = null;
            string  configUrl = Path.Combine(Application.streamingAssetsPath, configName + ".json");
#if UNITY_EDITOR
            string configJsonStr;
            using (var reader = File.OpenText(configUrl))
            {
                configJsonStr = await reader.ReadToEndAsync();
            }
#elif UNITY_WEBGL
            configJsonStr = await FileRequestHelper.Instance.LoadJsonString(configUrl);
#endif
            if (!string.IsNullOrEmpty(configJsonStr))
            {
                JsonData jsonData = JsonMapper.ToObject(configJsonStr);
                T obj = parser(jsonData);
                return returnConfig;
            }
            else
            {
                Debug.LogError("Load Config file failed! missing " + typeof(T).Name.ToString() + ".json in StreamingAssets.");

                return null;
            }
        }
    }
    public abstract class IConfigLoader : MonoBehaviour
    {
        public virtual async Task Load()
        {
            await Task.Yield();
        }
    }
    public abstract class IConfigLoaderWithParams : MonoBehaviour
    {
        public virtual async Task LoadWithParams(params object[] loadParams)
        {
            await Task.Yield();
        }
    }
    
    public class ConfigManager : Singleton<ConfigManager>
    {
        [Tooltip("不需要輸入參數的config loader")]
        public List<IConfigLoader> ConfigLoaders;

        public IEnumerator InitializeCoroutine()
        {
            yield return runAsyncTask().AsIEnumerator();
        }
        private async Task runAsyncTask()
        {
            foreach(var obj in ConfigLoaders)
            {
                await obj.Load();
            }
        }
    }

   
}

