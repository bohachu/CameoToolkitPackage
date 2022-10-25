using UnityEngine;
using UnityEditor;
namespace Cameo
{
    public class DialogueTool
    {
     
        [MenuItem("Cameo/UIToolkit/Create Dialogue system")]
        private static void CreateDialoguesystem()
        {
            var prefab = Resources.Load("DialogueCanvas", typeof(GameObject)) as GameObject;
            var instance = PrefabUtility.InstantiatePrefab(prefab, Selection.activeTransform);
            Undo.RegisterCreatedObjectUndo(instance, $"Create {instance.name}");
            Selection.activeObject = instance;
            //  var obj = Instantiate(prefab);


        }
    }
}