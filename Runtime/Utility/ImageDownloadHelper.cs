using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Threading.Tasks;
/// <summary>
/// 以coroutine與task的方式下載影像檔案
/// </summary>
public class ImageDownloadHelper
{
    Texture2D imageTexture;
    public Dictionary<string, Sprite> LoadedImages;
    public static async Task<Sprite> DownloadImageAsync(string imageURL)
    {
        if (string.IsNullOrEmpty(imageURL))
        {
            Debug.LogError("影像檔案url為空");
            return null;
        }
        UnityWebRequest www = UnityWebRequestTexture.GetTexture(imageURL);
        await www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log("http image下載url:" + imageURL);
            Debug.LogError("下載影像檔案失敗：" + www.error);
        }
        else
        {
            Debug.Log("http image下載完成:" + imageURL);
            var loadedTexture = ((DownloadHandlerTexture)www.downloadHandler).texture;
            return TextureToSprite(loadedTexture);
        }
        return null;
    }

    public static IEnumerator DownloadImage(string imageURL, Action<Sprite> OnComplete)
    {
        if(string.IsNullOrEmpty(imageURL))
        {
            Debug.LogError("影像檔案url為空");
            yield break;
        }
        UnityWebRequest www = UnityWebRequestTexture.GetTexture(imageURL);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log("http image下載url:"+imageURL);
            Debug.LogError("下載影像檔案失敗："+www.error);
        }
        else
        {
            var loadedTexture = ((DownloadHandlerTexture)www.downloadHandler).texture;
            OnComplete(TextureToSprite(loadedTexture));
        }
    }
    public static IEnumerator DownloadImages(List<string> imageURL,Action<List<Sprite>> OnComplete)
    {
        List<Sprite> sprites = new List<Sprite>();
        foreach (var url in imageURL)
        {
            if(string.IsNullOrEmpty(url))
            {
                Debug.LogError("影像檔案url為空");
                continue;
            }
            UnityWebRequest www = UnityWebRequestTexture.GetTexture(url);
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log("http image下載url:"+url);
                Debug.LogError("下載影像檔案失敗："+www.error);
            }
            else
            {
                var loadedTexture = ((DownloadHandlerTexture)www.downloadHandler).texture;
                sprites.Add(TextureToSprite(loadedTexture));
            }
        }
        OnComplete(sprites);
    }
    public IEnumerator DownloadImages(List<string> imageURL)
    {
        LoadedImages = new Dictionary<string, Sprite>();
        foreach (var url in imageURL)
        {
            if(string.IsNullOrEmpty(url))
            {
                Debug.LogError("影像檔案url為空");
                continue;
            }
            if(!LoadedImages.ContainsKey(url))
            {
                UnityWebRequest www = UnityWebRequestTexture.GetTexture(url);
                 yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success)
                {
                    Debug.Log("http image下載url:"+url);
                    Debug.LogError("下載影像檔案失敗："+www.error);
                }
                else
                {
                    imageTexture = ((DownloadHandlerTexture)www.downloadHandler).texture;
                    LoadedImages.Add(url, TextureToSprite(imageTexture));
                }
            }
        }
        yield return null;
    }
    static Sprite TextureToSprite(Texture2D texture)
    {
        var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100);
        return sprite;
    }
    
}
