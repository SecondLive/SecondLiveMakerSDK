using Amazon.Runtime;
using Amazon.S3;
using SecondLive.Maker.Editor;
using SecondLive.Sdk.Sapce;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

public class DownloadManager
{
    public static async Task<byte[]> DownloadSpaceFile(ulong spaceid, int repeatedCount = 0)
    {
        GetSpaceConfig getSpace = await RPCService.instance.GetSpaceConfig(spaceid);
        if (getSpace != null)
        {
            EditorUtility.DisplayProgressBar(Define.Text.PREVIEW_SPACE_DIALOG_TITLE, "downloading", 0);
            UnityWebRequest webRequest = UnityWebRequest.Get(getSpace.data);
            await webRequest.SendWebRequest();
            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                SpaceDescriptor spaceDescriptor = LitJson.JsonMapper.ToObject<SpaceDescriptor>(webRequest.downloadHandler.text);
                if (spaceDescriptor != null)
                {
                    string url = null;
                    for (int i = 0; i < spaceDescriptor.assetBundleUrls.Length; i++)
                    {
                        if ((Application.platform == RuntimePlatform.WindowsEditor && spaceDescriptor.assetBundleUrls[i].platform == "win") ||
                            (Application.platform == RuntimePlatform.OSXEditor && spaceDescriptor.assetBundleUrls[i].platform == "mac"))
                        {
                            url = spaceDescriptor.assetBundleUrls[i].url;
                            break;
                        }
                    }

                    if (!string.IsNullOrEmpty(url))
                    {
                        UnityWebRequest webRequestAssetBundle = UnityWebRequest.Get(url);
                        await webRequestAssetBundle.SendWebRequest();
                        if (webRequestAssetBundle.result == UnityWebRequest.Result.Success)
                        {
                            while (!webRequestAssetBundle.isDone)
                            {
                                await Task.Delay(100);
                            }
                            return webRequestAssetBundle.downloadHandler.data;
                        }
                    }
                }
            }
        }
        repeatedCount++;
        if (repeatedCount < 3)
            return await DownloadSpaceFile(spaceid, repeatedCount);
        else
        {
            EditorUtility.DisplayDialog(Define.Text.PREVIEW_SPACE_DIALOG_TITLE, "Preview Failed", Define.Text.DIALOG_BUTTON_OK);
            return null;
        }
    }
}

public static class ExtensionMethods
{
    public static System.Runtime.CompilerServices.TaskAwaiter<object> GetAwaiter(this UnityWebRequestAsyncOperation op)
    {
        var tcs = new TaskCompletionSource<object>();
        op.completed += (obj) => tcs.SetResult(null);
        return tcs.Task.GetAwaiter();
    }
}