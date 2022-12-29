using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Cameo;
using UnityEditor;
using UnityEngine;

namespace Cameo
{
    public class IResourceLoaderDefault : IDataLoader<ResourceContainer[]>
    {
        [SerializeField]
        private string[] folderPaths;

        public ResourceContainer[] containers;

#if UNITY_EDITOR

        //[Button]
        private void loadContainers()
        { 
            List<ResourceContainer> containerList = new List<ResourceContainer>();

            foreach(string folderPath in folderPaths)
            {
                string assetFolderPath = Path.Combine(Application.dataPath.Substring(Application.dataPath.LastIndexOf('/')+1), folderPath);

                string[] assetPaths = Directory.GetFiles(assetFolderPath);

                foreach(string assetPath in assetPaths)
                {
                    Debug.Log(assetPath);

                    Sprite asset = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);

                    if(asset != null)
                    {
                        string name = Path.GetFileNameWithoutExtension(assetPath);

                        containerList.Add(new ResourceContainer() { Asset = asset, AssetName = name });

                    }
                }
            }

            containers = containerList.ToArray();
        }

#endif

        public override async Task<ResourceContainer[]> Load()
        {
            await Task.Yield();
            return containers;
        }
    }

}
