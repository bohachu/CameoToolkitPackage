using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Threading.Tasks;
using System.Linq;
/// <summary>
/// 以coroutine與task的方式下載影像檔案
/// </summary>
public class ImageDownloadHelper
{
    Texture2D imageTexture;
    public Dictionary<string, Sprite> LoadedImages;

    public async Task<Sprite> DownloadImage(string imageURL)
    {
        if (string.IsNullOrEmpty(imageURL))
        {
            Debug.LogError("影像檔案 URL 為空");
            return null;
        }
        LoadedImages ??= new Dictionary<string, Sprite>();
        
        if(LoadedImages.TryGetValue(imageURL, out Sprite cachedSprite))
        {
            return cachedSprite;
        }
        try
        {
            Sprite sprite = await DownloadImageAsync(imageURL);
            if (sprite != null)
                LoadedImages[imageURL] = sprite;
            return sprite;
        }
        catch (Exception e)
        {
            Debug.LogError($"下載圖片失敗: {imageURL}, 錯誤: {e.Message}");
            return null;
        }
    }

    public static async Task<Sprite> DownloadImageAsync(string imageURL)
    {
        if (string.IsNullOrEmpty(imageURL))
        {
            Debug.LogError("影像檔案url為空");
            return null;
        }
        UnityWebRequest www = UnityWebRequestTexture.GetTexture(imageURL);
        try
        {
            await www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"下載影像失敗 URL: {imageURL}, 錯誤: {www.error}");
                return null;
            }
            Debug.Log($"下載完成:{imageURL}");
            var loadedTexture = ((DownloadHandlerTexture)www.downloadHandler).texture;
            return TextureToSprite(loadedTexture);
        }
        catch (Exception e)
        {
            Debug.LogError($"下載過程錯誤 URL: {imageURL}, 錯誤: {e.Message}");
            return null;
        }
    }
    
    public static IEnumerator DownloadImages(List<string> imageURLs, Action<List<Sprite>> onComplete)
    {
        if (imageURLs == null || imageURLs.Count == 0)
        {
            onComplete?.Invoke(new List<Sprite>());
            yield break;
        }

        var sprites = new List<Sprite>(imageURLs.Count);
        const int batchSize = 3;

        for (int i = 0; i < imageURLs.Count; i += batchSize)
        {
            var downloadTasks = new List<UnityWebRequest>(batchSize);
            
            for (int j = 0; j < batchSize && i + j < imageURLs.Count; j++)
            {
                string url = imageURLs[i + j];
                if (string.IsNullOrEmpty(url))
                    continue;

                var www = UnityWebRequestTexture.GetTexture(url);
                downloadTasks.Add(www);
                www.SendWebRequest();
            }
            while (downloadTasks.Any(www => !www.isDone))
            {
                yield return null;
            }
            foreach (var www in downloadTasks)
            {
                try
                {
                    if (www.result == UnityWebRequest.Result.Success)
                    {
                        var texture = ((DownloadHandlerTexture)www.downloadHandler).texture;
                        if (texture != null)
                        {
                            sprites.Add(TextureToSprite(texture));
                        }
                    }
                    else
                    {
                        Debug.LogError($"下載失敗: {www.error}");
                    }
                }
                finally
                {
                    www.Dispose();
                }
            }
        }
        onComplete?.Invoke(sprites);
    }
    public IEnumerator DownloadImages(List<string> imageURL)
    {
        LoadedImages = new Dictionary<string, Sprite>(imageURL.Count);
        
        const int batchSize = 3;
        for (int i = 0; i < imageURL.Count; i += batchSize)
        {
            var downloadTasks = new List<UnityWebRequestAsyncOperation>(batchSize);
            var urlBatch = new List<string>(batchSize);
            
            for (int j = 0; j < batchSize && i + j < imageURL.Count; j++)
            {
                string url = imageURL[i + j];
                if (string.IsNullOrEmpty(url) || LoadedImages.ContainsKey(url))
                    continue;

                var www = UnityWebRequestTexture.GetTexture(url);
                downloadTasks.Add(www.SendWebRequest());
                urlBatch.Add(url);
            }

            while (downloadTasks.Any(task => !task.isDone))
            {
                yield return null;
            }

            for (int j = 0; j < downloadTasks.Count; j++)
            {
                var www = downloadTasks[j].webRequest;
                string url = urlBatch[j];

                try
                {
                    if (www.result == UnityWebRequest.Result.Success)
                    {
                        using (var downloadHandler = (DownloadHandlerTexture)www.downloadHandler)
                        {
                            var texture = downloadHandler.texture;
                            if (texture != null)
                            {
                                var sprite = TextureToSprite(texture);
                                if (!LoadedImages.ContainsKey(url))
                                {
                                    LoadedImages.Add(url, sprite);
                                }
                            }
                        }
                    }
                    else
                    {
                        Debug.LogError($"下載影像失敗 URL: {url}, 錯誤: {www.error}");
                    }
                }
                finally
                {
                    www.Dispose();
                }
            }
        }
    }

    public static Sprite TextureToSprite(Texture2D texture)
    {
        if (texture == null) return null;
        
        try
        {
            var sprite = Sprite.Create(
                texture,
                new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f),
                100
            );
            
            return sprite;
        }
        catch (Exception e)
        {
            Debug.LogError($"轉換Texture到Sprite失敗: {e.Message}");
            return null;
        }
    }
}
