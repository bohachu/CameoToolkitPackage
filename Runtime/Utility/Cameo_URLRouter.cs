using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Cameo
{
    public class Cameo_URLRouter : MonoBehaviour
    {
        //目的是，使用relative url path,當domaine替換的時候，不用修改所有的url
        

        public static string GetURL(string relativePath)
        {
            if(relativePath==null)
                return "";
            if(string.IsNullOrEmpty(relativePath))
                return relativePath;
            if(isRelativePath(relativePath))
            {
                System.Uri resultUri = new System.Uri(new System.Uri(FastAPISettings.GameDataUrl ), relativePath);
                return resultUri.ToString();
            }
            return relativePath;
        }
        public static bool isRelativePath(string url)
        {
            return !url.StartsWith("http");
        }

    }
}

