using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Cameo.UI
{
    public class ProgressBarController : MonoBehaviour
    {
        [SerializeField]
        private Image foreground;

        [SerializeField]
        private float animationTime = 0.25f;

        [SerializeField]
        private bool onlyUseAnimationWhemIncrease = true;

        private float emptyPos;

        private void Awake()
        {
            emptyPos = -((RectTransform)foreground.rectTransform.parent).rect.width;
        }

        public void SetProgress(float progress)
        {
            float from = foreground.rectTransform.anchoredPosition.x;

            float to = emptyPos - progress * emptyPos;

            float time = (onlyUseAnimationWhemIncrease && from >= to) ? 0 : animationTime;

            LeanTween.value(gameObject, from, to, time).setOnUpdate(onUpdate);
        }

        private void onUpdate(float x)
        {
            foreground.rectTransform.anchoredPosition = new Vector2(x, 0);
        }
    }

}
