using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class UILoop_liveBreath : MonoBehaviour
{
    // Start is called before the first frame update
    public float scaleSize = 1.2f;
    public float duration = 1;
    void Start()
    {
        LeanTween.scale(gameObject.GetComponent<RectTransform>(), new Vector3(scaleSize, scaleSize, scaleSize), duration).setLoopPingPong();//.setLoopType(LeanTweenType.pingPong);
    }

}
