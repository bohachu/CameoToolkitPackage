using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
namespace Cameo
{
    public class DialogueMultiMediaPlayer : MonoBehaviour
    {
        //讓其他延伸媒體播放器可以接收到
        public Action<string> MultiPlayerAddOnEvent;
        public Action CloseMediaEvent;
        string preUrl;
        public void PlayMedia(string url)
        {
            //如果是隱藏指令，就關閉
            if(url.EndsWith(DialogueDataSet.PresetImageCommand.Hide.ToString()))
            {
                if(CloseMediaEvent!=null)
                    CloseMediaEvent();
                return;
            }
            if (string.IsNullOrEmpty(url))
            {
                Debug.LogError("對白多媒體播放url is null or empty");
                return;
            }
            MultiPlayerAddOnEvent.Invoke(url);
            preUrl=url;
        }
        public void IsShow(bool isShow)
        {
            if(isShow)
            {
                PlayMedia(preUrl);
            }else
            {
                if(CloseMediaEvent!=null)
                    CloseMediaEvent();
            }
        }
        public void Reset()
        {
            preUrl = "";
            if(CloseMediaEvent!=null)
                CloseMediaEvent();
        }
        public static bool isImage(string url)
        {
            return url.EndsWith(".jpg", System.StringComparison.OrdinalIgnoreCase) || url.EndsWith(".png", System.StringComparison.OrdinalIgnoreCase) || url.EndsWith(".jpeg", System.StringComparison.OrdinalIgnoreCase);
        }
        public static bool isVideo(string url)
        {
            return url.EndsWith(".mp4", System.StringComparison.OrdinalIgnoreCase) || url.EndsWith(".mov", System.StringComparison.OrdinalIgnoreCase) || url.EndsWith(".avi", System.StringComparison.OrdinalIgnoreCase);
        }
        public static bool isAudio(string url)
        {
            return url.EndsWith(".mp3", System.StringComparison.OrdinalIgnoreCase) || url.EndsWith(".wav", System.StringComparison.OrdinalIgnoreCase) || url.EndsWith(".ogg", System.StringComparison.OrdinalIgnoreCase);
        }
        public static bool isIframe(string url)
        {
            try
            {

                Newtonsoft.Json.Linq.JObject jObject = Newtonsoft.Json.Linq.JObject.Parse(url);
                return jObject.ContainsKey("iframe");
            }
            catch (System.Exception e)
            {
                
                return false;
            }
            
           
        }
    }
}