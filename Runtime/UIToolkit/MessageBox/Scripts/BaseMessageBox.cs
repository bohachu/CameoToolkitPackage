using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;

namespace Cameo.UI
{
	[RequireComponent(typeof(CanvasGroup))]
	public abstract class BaseMessageBox : MonoBehaviour 
	{
        public bool isOpen { get; private set; }
        public bool isUsingBackground = true;
        [SerializeField]
		protected float animationTime = 0.2f;

        private BaseMessageBoxAnimation[] _animations;
        protected BaseMessageBoxAnimation[] animations
        {
            get
            {
                if(_animations==null) _animations = GetComponents<BaseMessageBoxAnimation>();
                return _animations;
            }
        }

		protected Dictionary<string, object> paramMapping = null;
		protected MessageBoxManager manager;

        protected bool isInvokeClosedFunc;

        private void Awake()
        {
            isOpen = false;
            //animations = GetComponents<BaseMessageBoxAnimation>();
            
        }

        public void SetParam(Dictionary<string, object> dicParams)
        {
            paramMapping = dicParams;
        }

        public void Open(MessageBoxManager manager)
		{
            isOpen = true;

            this.manager = manager;
			
			onOpen ();
            gameObject.SetActive(true);
            StartCoroutine("playOpenAnimationCoroutine");
		}

        public void Close()
        {
            Close(true);
        }

        public void Close(bool isInvokeClosedFunc)
		{
            this.isInvokeClosedFunc = isInvokeClosedFunc;
            onClose();
            gameObject.SetActive(true);
            StartCoroutine("playCloseAnimationCoroutine");
        }

        protected IEnumerator playOpenAnimationCoroutine()
        {
            
                for (int i = 0; i < animations.Length; ++i)
                {
                    animations[i].PlayOpen(animationTime);
                }
           
            
            yield return new WaitForSeconds(animationTime);
            onOpened();
        }

        protected IEnumerator playCloseAnimationCoroutine()
        {
            for (int i = 0; i < animations.Length; ++i)
            {
                animations[i].PlayClose(animationTime);
            }
            yield return new WaitForSeconds(animationTime);
            if(isInvokeClosedFunc)
            {
                onClosed();
            }
            manager.OnMessageBoxClosed(this);
        }

		protected virtual void onOpen()
		{

		}

		protected virtual void onClose()
		{

		}

		protected virtual void onOpened()
		{
			
		}

		protected virtual void onClosed()
		{
			
		}
	}

	[System.Serializable]
	public class MessageBoxInfo
	{
		public string TypeName;
		public BaseMessageBox MessageBox;
	}
}
