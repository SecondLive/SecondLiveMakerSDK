using LitJson;
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
    public partial class RPCService : ScriptableSingleton<RPCService>
    {

        const string TOKEN_HEADER_KEY = "Authorization";

        public Task<T> Call<T>(string url, string method, string token, string json)
        {
            EditorUtility.DisplayProgressBar(null, "Waiting", 0);

            Debug.Log($"Rpc url:{url}");
            var request = new UnityWebRequest(url);
            request.method = method;
            request.SetRequestHeader(TOKEN_HEADER_KEY, token);
            if(!string.IsNullOrEmpty(json))
                request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(json));
            request.downloadHandler = new DownloadHandlerBuffer();
            var opt = request.SendWebRequest();
            TaskCompletionSource<T> tcs = new();
            opt.completed += o => Result<T>(request, tcs);
            if (request.isDone)Result<T>(request, tcs);
            return tcs.Task;
        }

        private void Result<T>(UnityWebRequest request,TaskCompletionSource<T> tcs)
        {
            EditorUtility.ClearProgressBar();
            if (request.result != UnityWebRequest.Result.Success)
                tcs.TrySetException(new Exception(request.error));
            else
            {
                var json = request.downloadHandler.text;
                tcs.TrySetResult(JsonMapper.ToObject<T>(json));
            }
        }

        public async Task<T> Call<T>(string url,string token,WWWForm formdata,Action<float> progressCallback)
        {
            var request = UnityWebRequest.Post(url, formdata);
            request.SetRequestHeader(TOKEN_HEADER_KEY, token);
            var opt = request.SendWebRequest();

            while (!opt.isDone)
            {
                progressCallback?.Invoke(request.uploadProgress);
                await Task.Delay(50);
            }

            if (request.result != UnityWebRequest.Result.Success)
                throw new Exception(request.error);

            var json = request.downloadHandler.text;
            request.Dispose();

            return JsonMapper.ToObject<T>(json);
        }
    }
}
