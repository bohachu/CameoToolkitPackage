using UnityEngine;
using UnityEditor;
namespace Cameo
{
    public class PageSystemEditor
    {

        [MenuItem("Cameo/UIToolkit/Create Page system")]
        private static void CreatePagesystem()
        {
            var prefab = Resources.Load("CanvasPage", typeof(GameObject)) as GameObject;
            var instance = PrefabUtility.InstantiatePrefab(prefab, Selection.activeTransform);
            Undo.RegisterCreatedObjectUndo(instance, $"Create {instance.name}");
            Selection.activeObject = instance;
         
        }
    }
}
