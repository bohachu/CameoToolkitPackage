using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cameo.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class MessageBoxFadeAnimation : BaseMessageBoxAnimation
    {
        [SerializeField]
        private float openAlpha;

        [SerializeField]
        private float closeAlpha;

        [SerializeField]
        private LeanTweenType easeType;

        [SerializeField]
        private LeanTweenType closeEaseType;

        private CanvasGroup canvasGroup;

        private void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        public override void PlayClose(float animationTime)
        {
            LeanTween.alphaCanvas(canvasGroup, closeAlpha, animationTime).setEase(closeEaseType);
           
        }

        public override void PlayOpen(float animationTime)
        {
            canvasGroup.alpha = closeAlpha;
            LeanTween.alphaCanvas(canvasGroup, openAlpha, animationTime).setEase(easeType);
            
        }
        
    }
}
