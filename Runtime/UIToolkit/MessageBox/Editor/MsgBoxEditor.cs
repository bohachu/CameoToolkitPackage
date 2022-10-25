using UnityEngine;
using UnityEditor;
namespace Cameo
{
    public class MsgBoxEditor
    {

        [MenuItem("Cameo/UIToolkit/Create MsgBox system")]
        private static void CreateMsgBoxsystem()
        {
            var prefab = Resources.Load("CanvasMsgBox", typeof(GameObject)) as GameObject;
            var instance = PrefabUtility.InstantiatePrefab(prefab, Selection.activeTransform);
            Undo.RegisterCreatedObjectUndo(instance, $"Create {instance.name}");
            Selection.activeObject = instance;
        }
    }
}