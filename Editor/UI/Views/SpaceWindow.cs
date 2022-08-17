using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace SecondLive.Maker.Editor
{
    public class SpaceWindow : BaseWindow
    {
        private ListView m_SpaceListView;
        private VisualElement m_InfoPanel;
        private Button m_Apply_Button, m_Revert_Button;
        private TextField m_NameInput, m_IntroduceInput, m_GuidInput;
        private Label m_Image_Label;
        List<SpaceInfo> searchSpaces;
        Dictionary<SpaceInfo.SpaceStatus, Color> stateColor = new Dictionary<SpaceInfo.SpaceStatus, Color>();

        public Button deleteButton;
        public Button uploadButton;

        public event UnityAction SMWebButtonClick;
        public event UnityAction NewSpaceButtonClick;
        public event UnityAction ApplyButtonClick;
        public event UnityAction RevertButtonClick;
        public event UnityAction OpenImageButtonClick;
        public event UnityAction SpaceUploadButtonClick;
        public event UnityAction PreviewButtonClick;
        public event UnityAction DeleteSpaceButtonClick;

        public event UnityAction<int> SelectionChange;

        private void CreateGUI()
        {
            stateColor.Add(SpaceInfo.SpaceStatus.Upload, Color.white);
            stateColor.Add(SpaceInfo.SpaceStatus.Pending, Color.yellow);
            stateColor.Add(SpaceInfo.SpaceStatus.Reviewing, Color.yellow);
            stateColor.Add(SpaceInfo.SpaceStatus.Approved, Color.green);

            titleContent = new GUIContent("Space Creator");

            var template = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/com.secondlive.maker/Editor/UI/Uxml/SpaceWindow.uxml");
            var view = template.CloneTree();

            SpaceManager.instance.RefreshSpaceInfos();

            m_GuidInput = view.Q<TextField>("guid_input");
            m_GuidInput.bindingPath = "m_Guid";
            m_GuidInput.SetEnabled(false);

            {
                uploadButton = view.Q<Button>("upload_button");
                uploadButton.clicked += () => SpaceUploadButtonClick?.Invoke();

                var previewButton = view.Q<Button>("preview_button");
                previewButton.clicked += () => PreviewButtonClick?.Invoke();

                deleteButton = view.Q<Button>("delete_button");
                deleteButton.clicked += () => DeleteSpaceButtonClick?.Invoke();
            }

            var newSpaceButton = view.Q<Button>("new_space_button");
            newSpaceButton.clicked += () => NewSpaceButtonClick?.Invoke();

            var managementButton = view.Q<Button>("management_button");
            managementButton.clicked += () => SMWebButtonClick?.Invoke();

            ToolbarSearchField searchField = view.Q<ToolbarSearchField>("search");
            searchField.RegisterValueChangedCallback(searchContent =>
            {
                searchSpaces ??= new List<SpaceInfo>();
                searchSpaces.Clear();
                if (!string.IsNullOrEmpty(searchContent.newValue))
                {
                    foreach (SpaceInfo item in UserManager.instance.UserInfo.SpaceInfos)
                    {
                        if (item.name.Contains(searchContent.newValue))
                            searchSpaces.Add(item);
                    }
                    if (BindObject.targetObject is SpaceWindowModel model)
                        model.SpaceInfos = searchSpaces.ToArray();
                }
                else
                {
                    if (BindObject.targetObject is SpaceWindowModel model)
                        model.SpaceInfos = UserManager.instance.UserInfo.SpaceInfos;
                }
                m_SpaceListView.bindingPath = "m_SpaceList";
            });

            var openImageButton = view.Q<Button>("open_image_button");
            openImageButton.clicked += () => OpenImageButtonClick?.Invoke();

            m_InfoPanel = view.Q<VisualElement>("info_panel");
            m_InfoPanel.visible = false;

            m_Image_Label = view.Q<Label>("image_label");

            m_Apply_Button = view.Q<Button>("apply_button");
            m_Apply_Button.clicked += () => ApplyButtonClick?.Invoke();

            m_Revert_Button = view.Q<Button>("revert_button");
            m_Revert_Button.clicked += () => RevertButtonClick?.Invoke();

            m_NameInput = view.Q<TextField>("name_input");
            m_NameInput.bindingPath = "m_NameInput";
            m_NameInput.RegisterValueChangedCallback(ev =>
            {
                var model = (SpaceWindowModel)BindObject.targetObject;
                bool changed = false;
                if (model == null || model.CurrentSpaceInfo == null)
                    changed = false;
                else
                    changed = (ev.newValue != model.CurrentSpaceInfo.name);

                m_Apply_Button.SetEnabled(changed);
                m_Revert_Button.SetEnabled(changed);
            });

            m_IntroduceInput = view.Q<TextField>("introduce_input");
            m_IntroduceInput.bindingPath = "m_IntroduceInput";
            m_IntroduceInput.RegisterValueChangedCallback(ev =>
            {
                var model = (SpaceWindowModel)BindObject.targetObject;
                bool changed = false;
                if (model == null || model.CurrentSpaceInfo == null)
                    changed = false;
                else
                    changed = (ev.newValue != model.CurrentSpaceInfo.introduce);

                m_Apply_Button.SetEnabled(changed);
                m_Revert_Button.SetEnabled(changed);
            });

            m_SpaceListView = view.Q<ListView>("space_list");
            m_SpaceListView.bindingPath = "m_SpaceList";
            m_SpaceListView.makeItem = OnSpaceListMakeItem;
            m_SpaceListView.bindItem = OnSpaceListBindItem;
            m_SpaceListView.onSelectionChange += OnSpaceListSelectionChange;
            rootVisualElement.Clear();

            rootVisualElement.Add(view);
        }

        private VisualElement OnSpaceListMakeItem()
        {
            IMGUIContainer item = new IMGUIContainer();
            item.style.flexDirection = FlexDirection.Row;
            item.style.justifyContent = Justify.SpaceBetween;

            var itemView = new Label();
            itemView.style.unityTextAlign = TextAnchor.MiddleLeft;
            item.Add(itemView);

            var itemView2 = new Label();
            itemView2.style.unityTextAlign = TextAnchor.MiddleRight;
            item.Add(itemView2);
            return item;
        }

        private void OnSpaceListBindItem(VisualElement element, int index)
        {
            VisualElement[] elements = element.Children().ToArray();
            SerializedProperty array = BindObject.FindProperty("m_SpaceList");
            if (index >= array.arraySize) //this is uielement bug ?
                element.visible = false;
            else
            {
                SerializedProperty spaceinfo = array.GetArrayElementAtIndex(index);
                Label itemView = elements[0] as Label;
                itemView.text = spaceinfo.FindPropertyRelative("name").stringValue;

                Label itemView2 = elements[1] as Label;
                SpaceInfo.SpaceStatus spaceStatus = ((SpaceWindowModel)spaceinfo.serializedObject.targetObject).SpaceInfos[index].status;
                if (spaceStatus == SpaceInfo.SpaceStatus.ReviewFailed)
                    spaceStatus = SpaceInfo.SpaceStatus.Upload;
                itemView2.text = spaceStatus.ToString();
                itemView2.style.color = stateColor[spaceStatus];

                element.visible = true;
            }
        }

        private void OnSpaceListSelectionChange(IEnumerable<object> items)
        {
            SelectionChange?.Invoke(m_SpaceListView.selectedIndex);
        }

        public void SetInfoPanelVisible(bool visible)
        {
            m_InfoPanel.visible = visible;
        }

        public void SetCoverImage(Texture2D image, string tip)
        {
            if (image == null)
                m_Image_Label.text = tip;
            else
                m_Image_Label.text = "";
            m_Image_Label.style.backgroundImage = image;
        }

        public override void Rebuild()
        {
            base.Rebuild();
            var model = (SpaceWindowModel)BindObject.targetObject;
            bool changed;
            if (model == null || model.CurrentSpaceInfo == null)
                changed = false;
            else
                changed = (model.NameInput != model.CurrentSpaceInfo.name)
                    || (model.IntroduceInput != model.CurrentSpaceInfo.introduce);

            m_Apply_Button.SetEnabled(changed);
            m_Revert_Button.SetEnabled(changed);

            m_SpaceListView?.Rebuild();
        }
    }
}
