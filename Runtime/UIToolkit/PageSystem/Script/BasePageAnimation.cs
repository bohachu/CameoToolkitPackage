using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cameo.UI
{
    public abstract class BasePageAnimation : MonoBehaviour
    {
        /// <summary>
        /// 播放換頁動畫
        /// </summary>
        /// <param name="during">動畫秒數</param>
        /// <param name="isReversed">是否反向播放</param>
        public abstract void Play(float during, bool isReversed);

        /// <summary>
        /// 當Page產生時初始化Page的顯示，直接設定成動畫初始狀態
        /// </summary>
        public abstract void SetFrom();

        /// <summary>
        /// 當Page產生時初始化Page的顯示，直接設定成動畫完成狀態
        /// </summary>
        public abstract void SetTo();
    }
}

