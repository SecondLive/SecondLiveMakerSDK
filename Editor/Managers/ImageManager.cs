using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace SecondLive.Maker.Editor
{
    public class ImageManager : ScriptableSingleton<ImageManager>
    {
        Dictionary<string, Texture2D> m_TextureCache = new Dictionary<string, Texture2D>();

        public Task<Texture2D> LoadTexture(string url)
        {
            TaskCompletionSource<Texture2D> tcs = new TaskCompletionSource<Texture2D>();

            if(m_TextureCache.TryGetValue(url,out Texture2D texture))
            {
                if(texture != null)
                {
                    tcs.TrySetResult(texture);
                    return tcs.Task;
                }
            }

            var request = UnityWebRequestTexture.GetTexture(url);
            request.timeout = 60;
            var opt = request.SendWebRequest();
            opt.completed += o =>
            {
                if (request.result == UnityWebRequest.Result.Success)
                {
                    var download = (DownloadHandlerTexture)request.downloadHandler;
                    tcs.TrySetResult(download.texture);
                    if (m_TextureCache.ContainsKey(url))
                        m_TextureCache[url] = texture;
                    else
                        m_TextureCache.Add(url, download.texture);
                }
                else
                {
                    EditorUtility.DisplayDialog("Error", request.error, "OK");
                }
            };
            return tcs.Task;
        }

        public void Clear()
        {
            m_TextureCache.Clear();
        }
    }
}
