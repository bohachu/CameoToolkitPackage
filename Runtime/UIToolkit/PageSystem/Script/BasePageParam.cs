using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cameo.UI
{
    [Serializable]
    public abstract class BasePageParam : MonoBehaviour
    {
        public abstract Dictionary<string, object> GetParam();
    }
}