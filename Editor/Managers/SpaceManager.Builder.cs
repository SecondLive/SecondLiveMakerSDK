using SecondLive.Sdk.Sapce;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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

                var outdir = EditorUtility.SaveFolderPanel("Save", "", "");
                if (string.IsNullOrEmpty(outdir))
                    return;
                var tempScenePath = $"Assets/{SystemHelper.NewGUID()}.unity";
                PreProcessScene(activeScene, info, tempScenePath);
                try
                {

                    BuildAssetBundles(tempScenePath, info, outdir);
                    SaveSapceDescriptor(info, outdir);
                }
                finally
                {
                    FileUtil.DeleteFileOrDirectory(tempScenePath);
                    AssetDatabase.Refresh();
                }
            }

            public static async Task BuildPipline(SpaceInfo info)
            {
                s_AssetBundleFilePathInfos.Clear();
                var activeScene = SceneManager.GetActiveScene();
                if (activeScene.isDirty && !EditorSceneManager.SaveScene(activeScene))
                    return;
                var outdir = $"{Application.temporaryCachePath}/{info.guid}";
                if (Directory.Exists((outdir)))
                    Directory.Delete(outdir, true);
                if (!CheckScene(activeScene))
                    return;
                var tempScenePath = $"Assets/{SystemHelper.NewGUID()}.unity";
                PreProcessScene(activeScene, info, tempScenePath);
                try
                {
                    BuildAssetBundles(tempScenePath, info, outdir);
                    await UploadSpaceResources(activeScene, info, outdir);
                }
                finally
                {
                    FileUtil.DeleteFileOrDirectory(tempScenePath);
                    AssetDatabase.Refresh();
                }
            }

            static bool CheckScene(Scene scene)
            {
                Component[] allComponents = FindObjectsOfType<Component>(true);
                UnityEngine.Object[] dependencieObjs = new UnityEngine.Object[1];
                List<string> textureSizePaths = new List<string>();
                List<string> customComponentPaths = new List<string>();
                int allTriangles = 0;
                foreach (Component component in allComponents)
                {
                    if (component as Transform)
                        continue;

                    dependencieObjs[0] = component;
                    var dependencies = EditorUtility.CollectDependencies(dependencieObjs);
                    foreach (UnityEngine.Object obj in dependencies)
                    {
                        if ((obj is Texture tTexture && (tTexture.width > 2048 || tTexture.height > 2048))
                            || (obj is Sprite sprite && (sprite.texture.width > 2048 || sprite.texture.height > 2048)))
                        {
                            textureSizePaths.Add(GetPath(component.transform) + "/" + component.name);
                        }
                    }

                    if (component is MonoBehaviour monoScript && AssetDatabase.GetAssetPath(MonoScript.FromMonoBehaviour(monoScript)).StartsWith("Assets"))
                        customComponentPaths.Add(GetPath(component.transform) + "/" + monoScript.GetType().Name);

                    if (component is MeshFilter meshFilter)
                    {
                        if (meshFilter.sharedMesh)
                            allTriangles += meshFilter.sharedMesh.triangles.Length;
                    }
                }

                if (textureSizePaths.Count > 0)
                {
                    StringBuilder msg = new StringBuilder(Define.Text.TEXTURE_SIZE_TO_LARGE);
                    msg.AppendLine();
                    foreach (string path in textureSizePaths)
                        msg.Append(path + ";");
                    EditorUtility.DisplayDialog(Define.Text.UPLOAD_SPACE_DIALOG_TITLE, msg.ToString(), Define.Text.DIALOG_BUTTON_OK);
                    return false;
                }

                if (customComponentPaths.Count > 0)
                {
                    StringBuilder msg = new StringBuilder(Define.Text.HAD_CUSTOM_MONO);
                    msg.AppendLine();
                    foreach (string path in customComponentPaths)
                        msg.Append(path + ";");
                    EditorUtility.DisplayDialog(Define.Text.UPLOAD_SPACE_DIALOG_TITLE, msg.ToString(), Define.Text.DIALOG_BUTTON_OK);
                    return false;
                }

                if (allTriangles > 1000000)
                {
                    EditorUtility.DisplayDialog(Define.Text.UPLOAD_SPACE_DIALOG_TITLE, $"{Define.Text.TRIANGLES_COUNT_TO_LARGE}{allTriangles}", Define.Text.DIALOG_BUTTON_OK);
                    return false;
                }
                return true;
            }

            static string GetPath(Transform trans)
            {
                if (!trans) return string.Empty;
                if (!trans.parent) return trans.name;
                return GetPath(trans.parent) + "/" + trans.name;
            }

            static void PreProcessScene(Scene activeScene, SpaceInfo info, string tempScenePath)
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

            static void BuildAssetBundles(string scenePath, SpaceInfo info, string outdir)
            {
                var currentTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
                var currentBuildTarget = EditorUserBuildSettings.activeBuildTarget;

                BuildAssetBundle(scenePath, outdir, info, BuildTarget.StandaloneWindows);
                BuildAssetBundle(scenePath, outdir, info, BuildTarget.StandaloneOSX);
                BuildAssetBundle(scenePath, outdir, info, BuildTarget.WebGL);
                BuildAssetBundle(scenePath, outdir, info, BuildTarget.iOS);
                BuildAssetBundle(scenePath, outdir, info, BuildTarget.Android);

                EditorUserBuildSettings.SwitchActiveBuildTarget(currentTargetGroup, currentBuildTarget);
            }

            static void BuildAssetBundle(string scenePath, string outdir, SpaceInfo info, BuildTarget target)
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

            static async Task UploadSpaceResources(Scene scene, SpaceInfo info, string outdir)
            {
                EditorUtility.DisplayProgressBar("Upload Aasset", "", 0);

                var s3credentials = await UploadManager.instance.CheckS3Credentials(info.guid);
                if (s3credentials == null)
                    return;

                var delRequest = await RPCService.instance.DeleteSpaceFile(info.guid, 0);
                Debug.Log(delRequest.code);

                for (var i = 0; i < s_AssetBundleFilePathInfos.Count; i++)
                {
                    var succeed = await UploadSceneBundle(s3credentials,s_AssetBundleFilePathInfos[i], i, s_AssetBundleFilePathInfos.Count);
                    if (succeed) continue;
                    EditorUtility.DisplayDialog("Error", $"Upload {s_AssetBundleFilePathInfos[i].filePath} fail!", "OK");
                    return;
                }
                var json = SaveSapceDescriptor(info, outdir);
                var message = await UploadSpaceDescriptor(json,info);
                if (!string.IsNullOrEmpty(message))
                    EditorUtility.DisplayDialog("Upload Error", message, "OK");
                else
                    EditorUtility.DisplayDialog("Upload", "Space resources uploaded!", "OK");
                return;
            }

            static async Task<bool> UploadSceneBundle(S3Credentials s3credentials, AssetBundleFileInfo info, int index, int maxCount)
            {
                if (!File.Exists(info.filePath))
                {
                    EditorUtility.DisplayDialog("Upload Error", $"{info.filePath} not found.", "OK");
                    return false;
                }

                var remotePath = await UploadManager.instance.UploadFileToS3Async(info.filePath, s3credentials,
                    (totla, current) =>
                    {
                        EditorUtility.DisplayProgressBar("Upload Asset",
                            $"Uploading asset package ({index + 1}/{maxCount}) ...", current / (float)totla);
                    },
                    (error) =>
                    {
                        EditorUtility.DisplayDialog("Upload Error", error, "OK");
                    });
                EditorUtility.ClearProgressBar();

                if (string.IsNullOrEmpty(remotePath))
                    return false;
                info.url = remotePath;
                return true;
            }

            static string SaveSapceDescriptor(SpaceInfo info, string ourdir)
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

            static async Task<string> UploadSpaceDescriptor(string json,SpaceInfo info)
            {
                var response = await RPCService.instance.UploadSceneConfigBundle(info.guid, json);
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
