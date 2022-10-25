using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
/// <summary>
/// 沒有被觸控的話，五秒後顯示手的動畫
/// 有被觸控過的話，不再顯示
/// </summary>
public class DragableHint : MonoBehaviour
{
    public Vector3 MoveDisaplce;
    [SerializeField]
    Image image;
    [SerializeField]
    float WaitingTime=5;
    LTDescr tween;
    bool isTouched = false;

    void ShowDragable()
    {
        if (isTouched) return;
        image.enabled = true;
        Vector3 targetPosition = transform.position + MoveDisaplce;
        if (tween == null)
            tween = LeanTween.move(gameObject.GetComponent<RectTransform>(), targetPosition, 1).setLoopType(LeanTweenType.easeInBounce);
        else
            tween.resume();
    }
    [Button]
    public void StopShowAnyMore()
    {
        isTouched = true;
        image.enabled = false;
        if(tween!=null)
            tween.pause();
    }
    // Start is called before the first frame update
    void Start()
    {
        image.enabled = false;
        Invoke("ShowDragable", WaitingTime);
    }
   
}
