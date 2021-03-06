using SecondLive.Sdk.Sapce;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Codice.Client.BaseCommands;
using Codice.CM.Common.Tree.Partial;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SecondLive.Maker.Editor
{
    public partial class SpaceManager : ScriptableSingleton<SpaceManager>
    {
        public static class Builder
        {
            readonly static List<AssetBundleFileInfo> s_AssetBundleFilePathInfos = new List<AssetBundleFileInfo>();

            public static void BuildLocalPipline(SpaceInfo info)
            {
                s_AssetBundleFilePathInfos.Clear();
                var activeScene = SceneManager.GetActiveScene();
                if (activeScene.isDirty && !EditorSceneManager.SaveScene(activeScene))
                    return;
                if (!CheckScene(activeScene))
                    return;

                var outdir =  EditorUtility.SaveFolderPanel("Save", "", "");
                if (string.IsNullOrEmpty(outdir))
                    return;
                var tempScenePath = $"Assets/{SystemHelper.NewGUID()}.unity";
                PreProcessScene(activeScene, info,tempScenePath);
                try
                {

                    BuildAssetBundles(tempScenePath, info,outdir);
                    SaveSapceDescriptor(info,outdir);
                }
                finally
                {
                    FileUtil.DeleteFileOrDirectory(tempScenePath);
                    AssetDatabase.Refresh();
                } 
            }

            public static async void  BuildPipline(SpaceInfo info)
            {
                s_AssetBundleFilePathInfos.Clear();
                var activeScene = SceneManager.GetActiveScene();
                if (activeScene.isDirty && !EditorSceneManager.SaveScene(activeScene))
                    return;
                var outdir = $"{Application.temporaryCachePath}/{info.guid}";
                if(Directory.Exists((outdir)))
                    Directory.Delete(outdir,true);
                if (!CheckScene(activeScene))
                    return;
                var tempScenePath = $"Assets/{SystemHelper.NewGUID()}.unity";
                PreProcessScene(activeScene, info,tempScenePath);
                try
                {
                    BuildAssetBundles(tempScenePath, info,outdir);
                    await UploadSpaceResources(activeScene, info,outdir);
                }
                finally
                {
                    FileUtil.DeleteFileOrDirectory(tempScenePath);
                    AssetDatabase.Refresh();
                }
            }

            static bool CheckScene(Scene scene)
            {
                return true;
            }

            static void PreProcessScene(Scene activeScene, SpaceInfo info,string tempScenePath)
            {
                var filename = $"{activeScene.name }_{ info.guid}".ToLower();

                if (File.Exists(tempScenePath))
                    File.Delete(tempScenePath);
                if (!AssetDatabase.CopyAsset(activeScene.path, tempScenePath))
                    throw new Exception($"Fail copy asset, {activeScene.path} to {tempScenePath}");
                AssetDatabase.Refresh();

                EditorSceneManager.OpenScene(tempScenePath, OpenSceneMode.Additive);
                var scene = SceneManager.GetSceneByPath(tempScenePath);
                var rootGameObjects = scene.GetRootGameObjects();
                foreach (var root in rootGameObjects)
                {
                    var frames = root.GetComponentsInChildren<SL_Frame>();
                    foreach (var frame in frames)
                    {
                        frame.guid = SystemHelper.NewGUID();
                        EditorUtility.SetDirty(frame);
                    }
                    var pedestals = root.GetComponentsInChildren<SL_AnimatorPedestal>();
                    foreach (var pedestal in pedestals)
                    {
                        pedestal.guid = SystemHelper.NewGUID();
                        EditorUtility.SetDirty(pedestal);
                    }
                }
                if (scene.isDirty && !EditorSceneManager.SaveScene(scene))
                    throw new Exception($"Fail save scene, {scene.path}");

                EditorSceneManager.CloseScene(scene, true);
            }

            static void BuildAssetBundles(string scenePath,SpaceInfo info,string outdir)
            {
                var currentTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
                var currentBuildTarget = EditorUserBuildSettings.activeBuildTarget;

                BuildAssetBundle(scenePath,outdir,info, BuildTarget.StandaloneWindows);
                BuildAssetBundle(scenePath,outdir,info, BuildTarget.StandaloneOSX);
                BuildAssetBundle(scenePath,outdir,info, BuildTarget.WebGL);
                //BuildAssetBundle(scene,info, BuildTarget.iOS);
                //BuildAssetBundle(scene,info, BuildTarget.Android);

                EditorUserBuildSettings.SwitchActiveBuildTarget(currentTargetGroup, currentBuildTarget);
            }

            static void BuildAssetBundle(string scenePath,string outdir,SpaceInfo info, BuildTarget target)
            {
                var targetName = GetPlatformName(target);
                var exportDirPath = $"{outdir}";
                Debug.Log($"export dir = {exportDirPath}");
                var assetBundleName = $"{Path.GetFileNameWithoutExtension(scenePath)}_{targetName}.ab";
                var assetBundleFilePath = $"{exportDirPath}/{assetBundleName}";
                if (!Directory.Exists(exportDirPath))
                {
                    Directory.CreateDirectory(exportDirPath);
                }
                if (File.Exists(assetBundleFilePath))
                {
                    File.Delete(assetBundleFilePath);
                }
                var abb = new AssetBundleBuild
                {
                    assetBundleName = assetBundleName,
                    assetNames = new string[] { scenePath }
                };
                Debug.Log($"Building to {assetBundleFilePath}");
                var options = BuildAssetBundleOptions.ChunkBasedCompression;
                var manifest = BuildPipeline.BuildAssetBundles(exportDirPath, new AssetBundleBuild[] { abb }, options, target);
                if (manifest == null)
                {
                    throw new Exception($"Fail build assetbundle, {assetBundleFilePath}");
                }
                PostProcessBuildAssetBundle(assetBundleFilePath, targetName);
            }

            static void PostProcessBuildAssetBundle(string assetBundleFileName, string target)
            {
                var assetBundleFileInfo = new AssetBundleFileInfo
                {
                    filePath = assetBundleFileName,
                    platform = target,
                };
                
                BuildPipeline.GetHashForAssetBundle(assetBundleFileName, out assetBundleFileInfo.hash);
                BuildPipeline.GetCRCForAssetBundle(assetBundleFileName, out assetBundleFileInfo.crc);
                s_AssetBundleFilePathInfos.Add(assetBundleFileInfo);
            }

            static async Task UploadSpaceResources(Scene scene, SpaceInfo info,string outdir)
            {
                EditorUtility.DisplayProgressBar("Upload Aasset","",0);
                for(var i = 0; i < s_AssetBundleFilePathInfos.Count; i++)
                {
                    var succeed = await UploadSceneBundle(s_AssetBundleFilePathInfos[i], i, s_AssetBundleFilePathInfos.Count);
                    if (succeed) continue;
                    EditorUtility.DisplayDialog("Error",$"Upload {s_AssetBundleFilePathInfos[i].filePath} fail!", "OK");
                    return;
                }
                var json = SaveSapceDescriptor(info,outdir);
                var message = await UploadSpaceDescriptor(json);
                if(!string.IsNullOrEmpty(message))
                    EditorUtility.DisplayDialog("Upload Error", message, "OK");
                else
                    EditorUtility.DisplayDialog("Upload", "Upload space assets done!", "OK");
                return;
            }

            static async Task<bool> UploadSceneBundle(AssetBundleFileInfo info,int index, int maxCount)
            {
                if (!File.Exists(info.filePath))
                {
                    EditorUtility.DisplayDialog("Upload Error",$"{info.filePath} not found.", "OK");
                    return false;
                }
                var s3credentials = await UploadManager.instance.CheckS3Credentials(
                    SpaceManager.instance.m_SpaceWindowModel.CurrentSpaceInfo.guid);
                if (s3credentials == null)
                    return false;

                var remotePath =  await UploadManager.instance.UploadFileToS3Async(info.filePath, s3credentials,
                    (totla, current) =>
                    {
                        EditorUtility.DisplayProgressBar("Upload Aasset",
                            $"Uploading assetBundle ({index}/{maxCount}) ...", current / (float)totla);
                    },
                    (error) =>
                    {
                        EditorUtility.DisplayDialog("Upload Error", error, "OK");
                    }
                    );
                EditorUtility.ClearProgressBar();

                if (string.IsNullOrEmpty(remotePath))
                    return false;
                info.url = remotePath;
                return true;
            }

            static string SaveSapceDescriptor(SpaceInfo info,string ourdir)
            {
                var assetBundleUrls = new SpaceDescriptor.AssetBundleUrl[s_AssetBundleFilePathInfos.Count];
                for (int i = 0; i < s_AssetBundleFilePathInfos.Count; i++)
                {
                    assetBundleUrls[i] = new SpaceDescriptor.AssetBundleUrl();
                    assetBundleUrls[i].url = s_AssetBundleFilePathInfos[i].url;
                    assetBundleUrls[i].platform = s_AssetBundleFilePathInfos[i].platform;
                    assetBundleUrls[i].hash = s_AssetBundleFilePathInfos[i].hash.ToString();
                    assetBundleUrls[i].crc = s_AssetBundleFilePathInfos[i].crc;
                }
                var pacakgeInfo = UnityEditor.PackageManager.PackageInfo.FindForAssetPath("Packages/com.secondlive.maker/package.json");
                var versionInfo = new VersionInfo
                {
                    unityVersion = Application.unityVersion,
                    makerVersion = pacakgeInfo.version
                };
                var descriptor = new SpaceDescriptor
                {
                    version = versionInfo,
                    assetBundleUrls = assetBundleUrls
                };
                var json = LitJson.JsonMapper.ToJson(descriptor);
                var jsonFilePath = $"{ourdir}/descriptor.json";
                if (File.Exists(jsonFilePath))
                    File.Delete(jsonFilePath);
                File.WriteAllText(jsonFilePath, json);
                return json;
            }

            static async Task<string> UploadSpaceDescriptor(string json)
            {
                var response = await RPCService.instance.UploadSceneConfigBundle(
                    SpaceManager.instance.m_SpaceWindowModel.CurrentSpaceInfo.guid, json);
                if (response.code != 0)
                    return response.message;
                return string.Empty;
            }

            static string GetPlatformName(BuildTarget target)
            {
                if (target == BuildTarget.StandaloneWindows)
                    return "win";
                if (target == BuildTarget.StandaloneOSX)
                    return "mac";
                if (target == BuildTarget.Android)
                    return "android";
                if (target == BuildTarget.iOS)
                    return "ios";
                if (target == BuildTarget.WebGL)
                    return "web";
                return string.Empty;
            }
        }
    }
}
