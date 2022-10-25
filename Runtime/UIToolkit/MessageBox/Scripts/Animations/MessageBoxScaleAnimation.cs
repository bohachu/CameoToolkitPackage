using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cameo.UI
{
    public class MessageBoxScaleAnimation : BaseMessageBoxAnimation
    {
        [SerializeField]
        private float openScale;

        [SerializeField]
        private float closeScale;

        [SerializeField]
        private LeanTweenType easeType;

        [SerializeField]
        private LeanTweenType closeEaseType;

        public override void PlayClose(float animationTime)
        {
            LeanTween.scale(gameObject, Vector3.one * closeScale, animationTime).setEase(closeEaseType);
        }

        public override void PlayOpen(float animationTime)
        {
            transform.localScale = Vector3.one * closeScale;
            LeanTween.scale(gameObject, Vector3.one * openScale, animationTime).setEase(easeType);
        }
    }
}
