using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
namespace Cameo
{
    public class AddressableTool 
    {
       
        private static string SettingRelativePath
        {
            get
            {
                return AddressableToolSetting.RelativePath;
            }
        }

        private const string EXT = "asset";

       private static string SettingFilePathInAsset
        {
            get
            {
                return Path.Combine("Assets/Resources", SettingRelativePath + "." + EXT);
            }
        }

        private static string SettingFolderPath
        {
            get
            {
                return Path.Combine("Assets/Resources", "AddressableTool");
            }
        }


        private static AddressableToolSetting setting;

        public AddressableTool()
        {
        }

        [MenuItem("Cameo/Addressable/Setting")]
        private static void selectAddressableGroupSetting()
        {
            if(setting == null)
            {
                AddressableToolSetting setting = AssetDatabase.LoadAssetAtPath<AddressableToolSetting>(SettingFilePathInAsset);

                if (setting == null)
                { 
                    setting = ScriptableObject.CreateInstance<AddressableToolSetting>();

                    if(!Directory.Exists(SettingFolderPath))
                    {
                        Directory.CreateDirectory(SettingFolderPath);
                    }

                    AssetDatabase.CreateAsset(setting, SettingFilePathInAsset);
                    EditorUtility.SetDirty(setting);
                    AssetDatabase.SaveAssets();
                }
                //If not exist, create setting
                Selection.objects = new Object[] { setting };
            }
        }

        [MenuItem("Cameo/Addressable/Make assets addressable")]
        private static void setAssetAddressable()
        {
            AddressableToolSetting setting = AssetDatabase.LoadAssetAtPath<AddressableToolSetting>(SettingFilePathInAsset);
            Debug.Log(SettingFilePathInAsset);
            setting.Groups = new List<GroupSetting>();

            var addressableSetting = AddressableAssetSettingsDefaultObject.Settings;

            if (setting == null)
            {
                return;
            }

            foreach (string folderPath in setting.Folders)
            {
                string groupName = GetGroupName(folderPath);

                AddressableAssetGroup group = addressableSetting.FindGroup(groupName);

                if (group == null)
                {
                    group = addressableSetting.CreateGroup(groupName, false, false, false, addressableSetting.DefaultGroup.Schemas);
                }

                BundledAssetGroupSchema schema = group.GetSchema<BundledAssetGroupSchema>();

                schema.BundleNaming = BundledAssetGroupSchema.BundleNamingStyle.OnlyHash;

                GroupSetting groupSetting = new GroupSetting();

                groupSetting.GroupName = groupName;

                setting.Groups.Add(groupSetting);

                string assetFolderPath = Path.Combine(Application.dataPath.Substring(Application.dataPath.LastIndexOf('/') + 1), folderPath);

                string[] assetPaths = Directory.GetFiles(assetFolderPath);

                foreach (string assetPath in assetPaths)
                {
                    //Debug.Log(assetPath);

                    Object asset = AssetDatabase.LoadAssetAtPath(assetPath, typeof(Object));

                    if(asset != null)
                    {
                        var guid = AssetDatabase.AssetPathToGUID(assetPath);

                        var entry = addressableSetting.CreateOrMoveEntry(guid, group);

                        entry.address = Path.GetFileNameWithoutExtension(assetPath);

                        addressableSetting.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, entry, true);

                        string type = AssetDatabase.GetMainAssetTypeAtPath(assetPath).ToString();

                        //Debug.Log(type);

                        if(AddressDef.TypeMapping.ContainsKey(type))
                        {
                            AddressDef addressDef = new AddressDef()
                            {
                                Address = entry.address,
                                Type = AddressDef.TypeMapping[type]
                            };

                            groupSetting.AddressDefs.Add(addressDef);
                        }
                    }
                }
            }

            foreach(string scenePath in setting.Scenes)
            {
                string groupName = GetSceneGroupName(scenePath);

                AddressableAssetGroup group = addressableSetting.FindGroup(groupName);

                if (group == null)
                {
                    group = addressableSetting.CreateGroup(groupName, false, false, false, addressableSetting.DefaultGroup.Schemas);
                }

                SceneAsset asset = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);

                if (asset != null)
                {
                    var guid = AssetDatabase.AssetPathToGUID(scenePath);

                    var entry = addressableSetting.CreateOrMoveEntry(guid, group);

                    entry.address = Path.GetFileNameWithoutExtension(scenePath);

                    addressableSetting.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, entry, true);
                }   
            }

            EditorUtility.SetDirty(addressableSetting);

            AssetDatabase.SaveAssets();
        }

        public static string GetGroupName(string folder) { return folder.Replace('/', '-').Replace(' ', '_'); }

        public static string GetSceneGroupName(string scenePath)
        {
            return Path.GetFileNameWithoutExtension(scenePath).Replace(' ', '_');
        }

        private static string SceneSettingRelativePath
        {
            get
            {
                return AssetSceneSetting.RelativePath;
            }
        }

        private static string SceneSettingFilePathInAsset
        {
            get
            {
                return Path.Combine("Assets/Resources", SceneSettingRelativePath + "." + EXT);
            }
        }

        [MenuItem("Cameo/Addressable/Scene setting")]
        private static void selectSceneResourcesSetting()
        {
            if (setting == null)
            {
                AssetSceneSetting setting = AssetDatabase.LoadAssetAtPath<AssetSceneSetting>(SceneSettingFilePathInAsset);

                if (setting == null)
                {
                    setting = ScriptableObject.CreateInstance<AssetSceneSetting>();

                    if (!Directory.Exists(SettingFolderPath))
                    {
                        Directory.CreateDirectory(SettingFolderPath);
                    }

                    AssetDatabase.CreateAsset(setting, SceneSettingFilePathInAsset);
                    EditorUtility.SetDirty(setting);
                    AssetDatabase.SaveAssets();
                }
                //If not exist, create setting
                Selection.objects = new Object[] { setting };
            }
        }
    }

}
