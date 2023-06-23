using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;


namespace Cameo
{
    public class ResourceManager : Singleton<ResourceManager>
    {
        private Dictionary<string, object> assetPool = new Dictionary<string, object>();

        private AddressableToolSetting setting;

        private Dictionary<string, List<AddressDef>> groupSettingMap = new Dictionary<string, List<AddressDef>>();

        public IEnumerator InitilizeCoroutine()
        {
            ResourceRequest request = Resources.LoadAsync<AddressableToolSetting>(AddressableToolSetting.RelativePath);
            yield return request;

            setting = request.asset as AddressableToolSetting;

            foreach (GroupSetting groupSetting in setting.Groups)
            {
                groupSettingMap[groupSetting.GroupName] = groupSetting.AddressDefs;
            }
        }

        public T LoadAsset<T>(string assetName) where T : class
        {
            if(string.IsNullOrWhiteSpace(assetName))
            {
                Debug.LogError("In Resource manager, load assry but AssetName is null or empty");
                return null;
            }
            if(assetPool.ContainsKey(assetName))
            {
                return (T)assetPool[assetName];
            }
            else
            {
                Debug.LogErrorFormat("Asset: {0} is not loaded", assetName);
                return null;
            }
        }

        public void LoadAssetAsync<T>(string assetName, Action<LoadResult<T>> onLoadCompleted)
        {
            if (assetPool.ContainsKey(assetName))
            {
                LoadResult<T> loadResult = new LoadResult<T>();
                loadResult.AssetName = assetName;
                loadResult.Asset = (T)assetPool[assetName];
                onLoadCompleted.Invoke(loadResult);
            }
            else
            {
                StartCoroutine(loadAssetCoroutine<T>(assetName, onLoadCompleted));
            }
        }

        private IEnumerator loadAssetCoroutine<T>(string assetName, Action<LoadResult<T>> onLoadCompleted)
        {
            var loadAsync = Addressables.LoadAssetAsync<T>(assetName);

            yield return loadAsync;

            LoadResult<T> loadResult = new LoadResult<T>();

            loadResult.AssetName = assetName;

            if (loadAsync.Status == AsyncOperationStatus.Succeeded)
            {
                assetPool[assetName] = loadAsync.Result;


                loadResult.Asset = (T)assetPool[assetName];
                onLoadCompleted.Invoke(loadResult);
            }
            else
            {
                loadResult.ErrorMsg = string.Format("load {0} error: {1}", assetName, loadAsync.Status.ToString());
                onLoadCompleted.Invoke(loadResult);
            }
        }

        /// <summary>
        /// 預載入資源的進度
        /// </summary>
        public float LoadingProgress { get; private set; }

        /// <summary>
        /// 預先把資源載入記憶體，把資源從addressable載入記憶體，增加LoadAsync/Load速度
        /// </summary>
        /// <param name="groups">隸屬於哪個group</param>
        /// <param name="addresses">單個的資源</param>
        /// <param name="onCompleted"></param>
        public void PreloadAssets(List<string> groups = null, Action onCompleted = null)
        {
            StartCoroutine(preloadAssetCoroutine(groups, onCompleted));
        }
        /// <summary>
        /// 預先載入sprite圖片，後續可以直接在pool中取用
        /// </summary>
        /// <param name="address"></param>
        /// <param name="onCompleted"></param>
        /// <returns></returns>
        public IEnumerator preloadImagesAssetCoroutine(List<string> address, Action onCompleted = null)
        {
            for(int i =0;i< address.Count; i++)
            {
                AddressDef curAdd = new AddressDef();
                curAdd.Address = address[i];
                curAdd.Type = AssetTypeEnum.Texture2D;
                if (!assetPool.ContainsKey(curAdd.Address))
                    yield return StartCoroutine(loadAssetCoroutine<Sprite>(curAdd));
                LoadingProgress = i / address.Count;
            }
            if (onCompleted != null)
                onCompleted.Invoke();
        }
        private IEnumerator preloadAssetCoroutine(List<string> groups = null, Action onCompleted = null)
        {
            LoadingProgress = 0;

            float totalAssetCount = 0;
            float curAssetLoadedCount = 0;

            foreach (string group in groups)
            {
                if (groupSettingMap.ContainsKey(group))
                {
                    totalAssetCount += groupSettingMap[group].Count;
                }
            }

            foreach (string group in groups)
            {
                if (groupSettingMap.ContainsKey(group))
                {
                  
                    List<AddressDef> addressDefs = groupSettingMap[group];
                    for (int i = 0; i < addressDefs.Count; ++i)
                    {
                        if (!assetPool.ContainsKey(addressDefs[i].Address))
                        {
                            if(addressDefs[i].Type == AssetTypeEnum.Texture2D)
                            {
                                yield return StartCoroutine(loadAssetCoroutine<Sprite>(addressDefs[i]));
                            }
                            
                        }

                        if(totalAssetCount != 0)
                        {
                            curAssetLoadedCount++;
                            LoadingProgress = curAssetLoadedCount / totalAssetCount;
                        }
                    }
                }
                else
                {
                    Debug.LogFormat("Group: {0} is not exist in setting.", group);
                }
            }

            if(onCompleted != null)
                onCompleted.Invoke();
        }

        private IEnumerator loadAssetCoroutine<T>(AddressDef addressDef)
        {
            var loadAsync = Addressables.LoadAssetAsync<T>(addressDef.Address);

            yield return loadAsync;

            if (loadAsync.Status == AsyncOperationStatus.Succeeded)
            {
                assetPool[addressDef.Address] = loadAsync.Result;

                //Debug.Log("Load " + addressDef.Address + " success!");
            }
            else
            {
                Debug.Log("Load " + addressDef.Address + " error! - " + loadAsync.Status.ToString());
            }
        }

        /// <summary>
        /// 釋放資源，若input == null，釋放現在所有資源
        /// </summary>
        /// <param name="groups">隸屬於哪個group</param>
        /// <param name="addresses">單個的資源</param>
        /// <param name=""></param>
        public void ReleaseAssets(List<string> groups)
        {
            string address = "";

            if(groups != null)
            {
                foreach (string group in groups)
                {
                    for (int i = 0; i < groupSettingMap[group].Count; ++i)
                    {
                        address = groupSettingMap[group][i].Address;
                        if (assetPool.ContainsKey(address))
                        {
                            Addressables.Release<object>(assetPool[address]);
                            assetPool.Remove(address);
                            //Debug.Log("Unload: " + address);
                        }
                    }
                }
            }
            else
            {
                assetPool.Clear();
            }

            Resources.UnloadUnusedAssets();
            GC.Collect();
        }

        public void ReleaseAsset(string assetName)
        {
            if(assetPool.ContainsKey(assetName))
            {
                Addressables.Release<object>(assetPool[assetName]);
                assetPool.Remove(assetName);
            }
            Resources.UnloadUnusedAssets();
        }
    }

    public class LoadResult<T>
    {
        public T Asset;
        public string AssetName = null;
        public string ErrorMsg = null;
    }

    [Serializable]
    public class ResourceContainer
    {
        public string AssetName;
        public Sprite Asset;
    }
}

