using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Cameo
{
    public class AddressableToolSetting : ScriptableObject
    {
        public const string RelativePath = "AddressableTool/GroupDef";
        
        /// <summary>
        /// 每個folder會建立一個Group來設定addressable asset
        /// </summary>
        public List<string> Folders;
        public List<GroupSetting> Groups;
        public List<string> Scenes;
    }

    [Serializable]
    public class GroupSetting
    {
        public string GroupName;
        public List<AddressDef> AddressDefs = new List<AddressDef>();
    }

    [Serializable]
    public class AddressDef
    {
        public static Dictionary<string, AssetTypeEnum> TypeMapping = new Dictionary<string, AssetTypeEnum>()
        {
            { "UnityEngine.Texture2D", AssetTypeEnum.Texture2D},
            { "Cameo.UI.ConversationData", AssetTypeEnum.Conversation},
            { "SeedHunter.SpriteMapInfo", AssetTypeEnum.AvatarMapInfo },
            { "UnityEngine.GameObject", AssetTypeEnum.Prefab }
        };


        public string Address;
        public AssetTypeEnum Type;
    }

    public enum AssetTypeEnum:int
    {
        Texture2D = 0,
        Conversation = 1,
        AvatarMapInfo = 2,
        Prefab = 3,
    }
}

