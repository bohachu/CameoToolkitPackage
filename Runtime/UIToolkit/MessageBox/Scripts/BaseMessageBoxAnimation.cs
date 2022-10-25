using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cameo.UI
{
    public abstract class BaseMessageBoxAnimation : MonoBehaviour
    {
        public abstract void PlayOpen(float animationTime);
        public abstract void PlayClose(float animationTime);
    }
}

