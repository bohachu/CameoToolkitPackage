using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cameo.UI;
using UnityEngine.UI;

public class DemoPageParam : BasePageParam
{
    public string textContent;
    public Texture2D imageContent;

    public override Dictionary<string, object> GetParam()
    {
        Dictionary<string, object> paramMapping = new Dictionary<string, object>();
        paramMapping.Add("text", textContent);
        paramMapping.Add("image", imageContent);
        return paramMapping;
    }
}
