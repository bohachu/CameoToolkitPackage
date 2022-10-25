using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cameo.UI
{
    [RequireComponent(typeof(UnityEngine.CanvasGroup))]
    public class FadePageAnimation : BasePageAnimation
    {
        private CanvasGroup group;

        [SerializeField]
        private LeanTweenType easeType;

        [SerializeField]
        private float from;

        [SerializeField]
        private float to;

        private void Awake()
        {
            group = GetComponent<CanvasGroup>();
        }

        public override void Play(float during, bool isReversed)
        {
            if(!isReversed)
            {
                group.alpha = from;
                LeanTween.alphaCanvas(group, to, during).setEase(easeType);
            }
            else
            {
                group.alpha = to;
                LeanTween.alphaCanvas(group, from, during).setEase(easeType);
            }
        }

        public override void SetFrom()
        {
            group.alpha = from;
        }

        public override void SetTo()
        {
            group.alpha = to;
        }
    }
}

