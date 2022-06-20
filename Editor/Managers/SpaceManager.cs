using SecondLive.Sdk.Sapce;
using System.IO;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SecondLive.Maker.Editor
{
    public partial class SpaceManager : ScriptableSingleton<SpaceManager>
    {
        private Object m_2dFramePrefab;
        private Object m_AnimatorPedestalPrefab;
        private SpaceWindowModel m_SpaceWindowModel;

        public void ShowSpacePanel()
        {
            m_SpaceWindowModel = ScriptableObject.CreateInstance<SpaceWindowModel>();
            m_SpaceWindowModel.SpaceInfos = UserManager.instance.UserInfo.SpaceInfos;

            var window = EditorWindow.GetWindow<SpaceWindow>("Space ");
            window.minSize = new Vector2(700, 380);
            window.Bind(m_SpaceWindowModel);

            window.NewSpaceButtonClick += async () =>
            {
                var response = await RPCService.instance.CreateSpace();
                if (response.code == 0)
                {
                    m_SpaceWindowModel.AddSpace(response.data);
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


                if (!string.IsNullOrEmpty(spaceInfo.image_url))
                {
                    window.SetCoverImage(null, "loading...");
                    var image = await ImageManager.instance.LoadTexture(spaceInfo.image_url);
                    window.SetCoverImage(image,null);
                }
                else
                {
                    window.SetCoverImage(null,"Please setting cover image");
                }
                window.SetInfoPanelVisible(true);
            };

            window.RevertButtonClick += () =>
            {
                m_SpaceWindowModel.NameInput = m_SpaceWindowModel.CurrentSpaceInfo.name;
                m_SpaceWindowModel.IntroduceInput = m_SpaceWindowModel.CurrentSpaceInfo.introduce;
            };

            window.SpaceUploadButtonClick += async () =>
            {
                Builder.BuildPipline(m_SpaceWindowModel.CurrentSpaceInfo);
            };

            window.DeleteSpaceButtonClick += async ()=>
            {
               if(EditorUtility.DisplayDialog(Define.Text.DELETE_SPACE_DIALOG_TITLE,
                    string.Format(Define.Text.DELETE_SPACE_MESSAGE, m_SpaceWindowModel.CurrentSpaceInfo.name),
                    Define.Text.DIALOG_BUTTON_OK, Define.Text.DIALOG_BUTTON_CANCEL))
                {
                    var response = await RPCService.instance.DeleteSpace(m_SpaceWindowModel.CurrentSpaceInfo.guid);
                    if (response == null)
                        return;
                    if(response.code == 0)
                    {
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
                    m_SpaceWindowModel.CurrentSpaceInfo.guid,imagePath,
                    (progress)=> {  });

                if (response.code == 0)
                {
                    m_SpaceWindowModel.CurrentSpaceInfo.image_url = response.data;
                    var image = await ImageManager.instance.LoadTexture(m_SpaceWindowModel.CurrentSpaceInfo.image_url);
                    window.SetCoverImage(image,null);
                }
                else
                {
                    window.SetCoverImage(null, "Please select cover image");
                    EditorUtility.DisplayDialog("Error", response.message, "OK");
                }
            };

            window.Destroyed += () =>
            {
                UserManager.instance.UserInfo.SpaceInfos = m_SpaceWindowModel.SpaceInfos;
                UnityEngine.Object.DestroyImmediate(m_SpaceWindowModel);
            };
        }

        public SL_Space CreateSpace()
        {
            var go = new GameObject("SL_Space");
            return go.AddComponent<SL_Space>();
        }

        public void AddFrame()
        {
            if(m_2dFramePrefab == null)
                m_2dFramePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Packages/com.secondlive.maker/PackageResources/Prefabs/Live2dFrame.prefab");
            var go = PrefabUtility.InstantiatePrefab(m_2dFramePrefab);
            go.name = m_2dFramePrefab.name;
            if (Selection.activeGameObject != null && Selection.activeGameObject.activeInHierarchy)
                ((GameObject)go).transform.SetParent(Selection.activeGameObject.transform,false);
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
