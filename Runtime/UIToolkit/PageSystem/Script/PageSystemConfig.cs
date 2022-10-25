using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace Cameo.UI
{
    [Serializable]
    public class PageSystemConfig : ScriptableObject
    {
        public List<PageConfigData> Pages;

        private Dictionary<string, PageConfigData> pageMapping;

        public Dictionary<string, PageConfigData> PageMapping
        {
            get
            {
                if (pageMapping == null)
                {
                    pageMapping = new Dictionary<string, PageConfigData>();
                    for (int i = 0; i < Pages.Count; ++i)
                    {
                        pageMapping.Add(Pages[i].ID, Pages[i]);
                    }
                }
                return pageMapping;
            }
        }

        [Button("Use develop param all")]
        private void useDevelopParamAll()
        {
            for (int i = 0; i < Pages.Count; ++i)
                Pages[i].UseDevelopParam = true;
        }

        [Button("Unuse develop param all")]
        private void unuseDevelopParamAll()
        {
            for (int i = 0; i < Pages.Count; ++i)
                Pages[i].UseDevelopParam = false;
        }
    }

    [Serializable]
    public class PageConfigData
    {
        [PropertyTooltip("切換頁面時的識別ID")]
        public string ID;
        public BasePage Prefab;
        [PropertyTooltip("Depth高的頁面在切換動畫演出時會在上層")]
        public int Depth;
        public bool UseDevelopParam = false;
    }
}