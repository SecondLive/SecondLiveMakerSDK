using SecondLive.Sdk.Sapce;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace SecondLive.Maker.Editor
{
    public partial class SpaceManager : ScriptableSingleton<SpaceManager>
    {
        private Object m_2dFramePrefab;
        private Object m_AnimatorPedestalPrefab;
        private SpaceWindowModel m_SpaceWindowModel;
        int currentSelectedIndex;

        public void ShowSpacePanel()
        {
            RegisterEvent(Init());
        }

        SpaceWindow Init()
        {
            if (m_SpaceWindowModel == null)
                m_SpaceWindowModel = CreateInstance<SpaceWindowModel>();
            m_SpaceWindowModel.SpaceInfos = UserManager.instance.UserInfo.SpaceInfos;

            SpaceWindow window = EditorWindow.GetWindow<SpaceWindow>(Define.Text.SPACE_WINDOW_NAME);
            window.minSize = new Vector2(850, 520);
            window.Bind(m_SpaceWindowModel);
            return window;
        }

        void RegisterEvent(SpaceWindow window)
        {
            window.NewSpaceButtonClick += async () =>
            {
                var response = await RPCService.instance.CreateSpace();
                if (response.code == 0)
                {
                    m_SpaceWindowModel.AddSpace(response.data);
                    List<SpaceInfo> sis = new(UserManager.instance.UserInfo.SpaceInfos) { response.data };
                    UserManager.instance.UserInfo.SpaceInfos = sis.ToArray();
                    window.Rebuild();
                }
                else
                {
                    EditorUtility.DisplayDialog("Error", response.message, "OK");
                }
            };

            window.SMWebButtonClick += () =>
            {
                var url = $"{Define.Url.SECRETKEY_URL}?token={UserManager.instance.UserInfo.Token}";
                Application.OpenURL(url);
            };

            window.ApplyButtonClick += async () =>
            {
                var response = await RPCService.instance.ApplySpaceInfo(
                    m_SpaceWindowModel.CurrentSpaceInfo.guid,
                    m_SpaceWindowModel.NameInput,
                    m_SpaceWindowModel.IntroduceInput);
                if (response.code == 0)
                {
                    m_SpaceWindowModel.CurrentSpaceInfo.name = response.data.name;
                    m_SpaceWindowModel.CurrentSpaceInfo.introduce = response.data.introduce;
                    window.Rebuild();
                }
                else
                {
                    EditorUtility.DisplayDialog("Error", response.message, "OK");
                }
            };

            window.SelectionChange += async (index) =>
            {
                currentSelectedIndex = index;
                if (index < 0 || index >= m_SpaceWindowModel.SpaceInfos.Length)
                {
                    window.SetInfoPanelVisible(false);
                    return;
                }

                var spaceInfo = m_SpaceWindowModel.SpaceInfos[index];
                m_SpaceWindowModel.CurrentSpaceInfo = spaceInfo;
                m_SpaceWindowModel.NameInput = spaceInfo.name;
                m_SpaceWindowModel.IntroduceInput = spaceInfo.introduce;
                m_SpaceWindowModel.Guid = $"guid:{spaceInfo.guid}";

                window.uploadButton.SetEnabled(spaceInfo.status == SpaceInfo.SpaceStatus.Pending
                    || spaceInfo.status == SpaceInfo.SpaceStatus.Upload
                    || spaceInfo.status == SpaceInfo.SpaceStatus.ReviewFailed);
                window.deleteButton.SetEnabled(spaceInfo.status == SpaceInfo.SpaceStatus.Upload
                    || spaceInfo.status == SpaceInfo.SpaceStatus.ReviewFailed);

                if (!string.IsNullOrEmpty(spaceInfo.image_url))
                {
                    window.SetCoverImage(null, "loading...");
                    var image = await ImageManager.instance.LoadTexture(spaceInfo.image_url);
                    window.SetCoverImage(image, null);
                }
                else
                    window.SetCoverImage(null, "Please set cover image");
                window.SetInfoPanelVisible(true);
            };

            window.RevertButtonClick += () =>
            {
                m_SpaceWindowModel.NameInput = m_SpaceWindowModel.CurrentSpaceInfo.name;
                m_SpaceWindowModel.IntroduceInput = m_SpaceWindowModel.CurrentSpaceInfo.introduce;
            };

            window.SpaceUploadButtonClick += async () =>
            {
                await Builder.BuildPipline(m_SpaceWindowModel.CurrentSpaceInfo);
                m_SpaceWindowModel.CurrentSpaceInfo.status = SpaceInfo.SpaceStatus.Pending;
                Init();
            };

            window.PreviewButtonClick += () =>
            {
                Application.OpenURL($"{Define.Url.URL}/decoration?spaceid={instance.m_SpaceWindowModel.CurrentSpaceInfo.guid}&preview=1");
                //if (!EditorApplication.isPlaying)
                //{
                //    EditorUtility.DisplayDialog(Define.Text.PREVIEW_SPACE_DIALOG_TITLE, "Please preview after play", Define.Text.DIALOG_BUTTON_OK);
                //    return;
                //}
                //string path = $"{Application.dataPath}/{instance.m_SpaceWindowModel.CurrentSpaceInfo.guid}.ab";
                //if (!File.Exists(path))
                //{
                //    byte[] assetBundleData = await DownloadManager.DownloadSpaceFile(instance.m_SpaceWindowModel.CurrentSpaceInfo.guid);
                //    File.WriteAllBytes(path, assetBundleData);
                //}
                //AssetBundle assetBundle = AssetBundle.LoadFromFile(path);
                //string[] scenePaths = assetBundle.GetAllScenePaths();
                //if (scenePaths.Length > 0)
                //    SceneManager.LoadScene(scenePaths[0]);
                //assetBundle.Unload(false);
                //EditorUtility.ClearProgressBar();
            };

            window.DeleteSpaceButtonClick += async () =>
            {
                if (EditorUtility.DisplayDialog(Define.Text.DELETE_SPACE_DIALOG_TITLE,
                    string.Format(Define.Text.DELETE_SPACE_MESSAGE, m_SpaceWindowModel.CurrentSpaceInfo.name),
                    Define.Text.DIALOG_BUTTON_OK, Define.Text.DIALOG_BUTTON_CANCEL))
                {
                    var response = await RPCService.instance.DeleteSpace(m_SpaceWindowModel.CurrentSpaceInfo.guid);
                    if (response == null)
                        return;
                    if (response.code == 0)
                    {
                        UserManager.instance.UserInfo.SpaceInfos =
                            UserManager.instance.UserInfo.SpaceInfos.Where(si => si != m_SpaceWindowModel.CurrentSpaceInfo).ToArray();
                        m_SpaceWindowModel.RemoveSpace(m_SpaceWindowModel.CurrentSpaceInfo);
                        m_SpaceWindowModel.CurrentSpaceInfo = null;
                        window.SetInfoPanelVisible(false);
                        window.Rebuild();
                    }
                    else
                    {
                        EditorUtility.DisplayDialog(Define.Text.DELETE_SPACE_DIALOG_TITLE, $"{response.message} ({response.code})", Define.Text.DIALOG_BUTTON_OK);
                    }
                }
            };

            window.OpenImageButtonClick += async () =>
            {
                var imagePath = EditorUtility.OpenFilePanelWithFilters("Selection Image", "", new[] { "Image files", "png,jpg,jpeg", "All files", "*" });
                if (string.IsNullOrEmpty(imagePath))
                    return;

                window.SetCoverImage(null, "loading...");
                var response = await RPCService.instance.UploadCoverImage(
                    m_SpaceWindowModel.CurrentSpaceInfo.guid, imagePath,
                    (progress) => { });

                if (response.code == 0)
                {
                    m_SpaceWindowModel.CurrentSpaceInfo.image_url = response.data;
                    var image = await ImageManager.instance.LoadTexture(m_SpaceWindowModel.CurrentSpaceInfo.image_url);
                    window.SetCoverImage(image, null);
                }
                else
                {
                    window.SetCoverImage(null, "Please select cover image");
                    EditorUtility.DisplayDialog("Error", response.message, "OK");
                }
            };

            window.Destroyed += () => DestroyImmediate(m_SpaceWindowModel);
        }

        public async void RefreshSpaceInfos()
        {
            var gresponse = await RPCService.instance.GetSpaceInfos();
            if (gresponse.code == 0)
                UserManager.instance.UserInfo.SpaceInfos = gresponse.data;
        }

        public SL_Space CreateSpace()
        {
            var go = new GameObject("SL_Space");
            return go.AddComponent<SL_Space>();
        }

        public void AddFrame()
        {
            if (m_2dFramePrefab == null)
                m_2dFramePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Packages/com.secondlive.maker/PackageResources/Prefabs/Live2dFrame.prefab");
            var go = PrefabUtility.InstantiatePrefab(m_2dFramePrefab);
            go.name = m_2dFramePrefab.name;
            if (Selection.activeGameObject != null && Selection.activeGameObject.activeInHierarchy)
                ((GameObject)go).transform.SetParent(Selection.activeGameObject.transform, false);
            Selection.activeObject = go;
        }

        public void AddAnimatorPedestal()
        {
            if (m_AnimatorPedestalPrefab == null)
                m_AnimatorPedestalPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Packages/com.secondlive.maker/PackageResources/Prefabs/AnimatorPedestal.prefab");
            var go = PrefabUtility.InstantiatePrefab(m_AnimatorPedestalPrefab);
            go.name = m_AnimatorPedestalPrefab.name;
            if (Selection.activeGameObject != null && Selection.activeGameObject.activeInHierarchy)
                ((GameObject)go).transform.SetParent(Selection.activeGameObject.transform, false);
            Selection.activeObject = go;
        }
    }
}
