using UnityEngine;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;

namespace Cameo.UI
{
    public static class ScriptableObjectUtility
    {
        public static T CreateAsset<T>(string fileName = null) where T : ScriptableObject
        {
            T asset = ScriptableObject.CreateInstance<T>();

            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (path == "")
            {
                path = "Assets";
            }
            else if (Path.GetExtension(path) != "")
            {
                path = path.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
            }

            string assetPathAndName = (string.IsNullOrEmpty(fileName)) ? 
                AssetDatabase.GenerateUniqueAssetPath(Path.Combine(path, "New " + typeof(T).ToString() + ".asset")) : 
                AssetDatabase.GenerateUniqueAssetPath(Path.Combine(path, fileName + ".asset"));

            AssetDatabase.CreateAsset(asset, assetPathAndName);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = asset;

            return asset;
        }
    }
}

#endif