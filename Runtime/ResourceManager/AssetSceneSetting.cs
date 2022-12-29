using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cameo
{
    public class AssetSceneSetting : ScriptableObject
    {
        public const string RelativePath = "AddressableTool/SceneDef";

        public List<SceneResourceDef> ResourcesDefs;

        public List<string> AddressableScenes;

        private Dictionary<string, List<GroupDef>> resourcesMap;

        public Dictionary<string, List<GroupDef>> ResourcesMap
        {
            get
            {
                if(resourcesMap == null)
                {
                    resourcesMap = new Dictionary<string, List<GroupDef>>();
                    foreach(SceneResourceDef resourceDef in ResourcesDefs)
                    {
                        resourcesMap[resourceDef.Scene] = resourceDef.Groups;
                    }
                }
                return resourcesMap;
            }
        }

        public List<string> CollectUnloadGroup(string scene)
        {
            List<string> groups = new List<string>();
            for (int i = 0; i < ResourcesMap[scene].Count; ++i)
            {
                if(ResourcesMap[scene][i].Unload)
                {
                    groups.Add(ResourcesMap[scene][i].Group);
                }
            }
            return groups;
        }

        public List<string> CollectLoadGroups(string scene)
        {
            List<string> groups = new List<string>();
            for(int i=0; i< ResourcesMap[scene].Count; ++i)
            {
                if (ResourcesMap[scene][i].Load)
                {
                    groups.Add(ResourcesMap[scene][i].Group);
                }
            }
            return groups;
        }

        public bool IsSceneAddressable(string scene)
        {
            return AddressableScenes.Contains(scene);
        }
    }

    [Serializable]
    public class SceneResourceDef
    {
        public string Scene;
        public List<GroupDef> Groups;
    }

    [Serializable]
    public class GroupDef
    {
        public string Group;
        public bool Load = true;
        public bool Unload = true;
    }
}
