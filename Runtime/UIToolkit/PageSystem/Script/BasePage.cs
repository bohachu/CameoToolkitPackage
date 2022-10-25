using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cameo.UI
{
    public abstract class BasePage : MonoBehaviour
    {
        private BasePageAnimation[] pageAnimations;

        protected BasePageManager pageManager;

        protected Dictionary<string, object> paramMapping;

        private void Awake()
        {
            pageAnimations = GetComponents<BasePageAnimation>();
        }
        
        public void Initialize(BasePageManager pageManager, bool setAppearCompleted, Dictionary<string, object> paramMapping = null)
        {
            this.pageManager = pageManager;
            this.paramMapping = paramMapping;
            Debug.Log(name + "is param null:" + (paramMapping == null).ToString());
            if(pageAnimations != null)
            {
                for (int i = 0; i < pageAnimations.Length; ++i)
                {
                    if (!setAppearCompleted)
                        pageAnimations[i].SetFrom();
                    else
                        pageAnimations[i].SetTo();
                }
            }
        }

        public void Open(float animTime)
        {
            if (pageAnimations != null)
            {
                for (int i = 0; i < pageAnimations.Length; ++i)
                {
                    pageAnimations[i].Play(animTime, false);
                }
            }
        }

        public void Close(float animTime)
        {
            if (pageAnimations != null)
            {
                for (int i = 0; i < pageAnimations.Length; ++i)
                {
                    pageAnimations[i].Play(animTime, true);
                }
            }
        }

        public virtual void OnOpen()
        {
            
        }

        public virtual void OnClose()
        {
            
        }

        public virtual void OnOpened()
        {
           
        }

        public virtual void OnClosed()
        {
            
        }
    }
}