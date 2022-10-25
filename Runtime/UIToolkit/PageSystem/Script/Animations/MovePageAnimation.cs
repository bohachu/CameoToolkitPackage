using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace Cameo.UI
{
    public class MovePageAnimation : BasePageAnimation
    {
        [SerializeField]
        private LeanTweenType easeType;

        [SerializeField]
        private Vector3 from;

        [Button("Set from position")]
        private void setFromInEditor()
        {
            from = transform.localPosition;
        }

        [SerializeField]
        private Vector3 to;

        [Button("Set to position")]
        private void setToInEditor()
        {
            to = transform.localPosition;
        }

        private void Awake()
        {

        }

        public override void Play(float during, bool isReversed)
        {
            if (!isReversed)
            {
                gameObject.transform.localPosition = from;
                LeanTween.moveLocal(gameObject, to, during).setEase(easeType);
            }
            else
            {
                gameObject.transform.localPosition = to;
                LeanTween.moveLocal(gameObject, from, during).setEase(easeType);
            }
        }

        public override void SetFrom()
        {
            gameObject.transform.localPosition = from;
        }

        public override void SetTo()
        {
            gameObject.transform.localPosition = to;
        }
    }
}

