using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// 以coroutine的方式下載影像檔案
/// </summary>
public class ImageDownloadHelper
{
    Texture2D imageTexture;
    public Dictionary<string, Sprite> LoadedImages;
    public IEnumerator DownloadImages(List<string> imageURL)
    {
        LoadedImages = new Dictionary<string, Sprite>();
        foreach (var url in imageURL)
        {
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
                // Debug.Log("Finish download image : " + url);
                }
                LoadedImages.Add(url, TextureToSprite(imageTexture));
            }
        }
        yield return null;
    }
    Sprite TextureToSprite(Texture2D texture)
    {
        var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100);
        return sprite;
    }
    
}
