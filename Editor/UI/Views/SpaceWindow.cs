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
        private TextField m_NameInput, m_IntroduceInput,m_GuidInput;
        private Label m_Image_Label;
        private IMGUIContainer m_MGUIContainer;

        public event UnityAction SMWebButtonClick;
        public event UnityAction NewSpaceButtonClick;
        public event UnityAction ApplyButtonClick;
        public event UnityAction RevertButtonClick;
        public event UnityAction OpenImageButtonClick;
        public event UnityAction SpaceUploadButtonClick;
        public event UnityAction DeleteSpaceButtonClick;

        public event UnityAction<int> SelectionChange;


        private void CreateGUI()
        {
            titleContent = new GUIContent("Space Creator");

            var template = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/com.secondlive.maker/Editor/UI/Uxml/SpaceWindow.uxml");
            var view = template.CloneTree();

            m_GuidInput = view.Q<TextField>("guid_input");
            m_GuidInput.bindingPath = "m_Guid";
            m_GuidInput.SetEnabled(false);

            m_MGUIContainer = view.Q<IMGUIContainer>("upload_space_container");
            m_MGUIContainer.onGUIHandler = OnMGUIHandler;

            var newSpaceButton = view.Q<Button>("new_space_button");
            newSpaceButton.clicked += ()=> NewSpaceButtonClick?.Invoke();

            var managementButton = view.Q<Button>("management_button");
            managementButton.clicked += () => SMWebButtonClick?.Invoke();

            var openImageButton = view.Q<Button>("open_image_button");
            openImageButton.clicked += () => OpenImageButtonClick?.Invoke();

            m_InfoPanel = view.Q<VisualElement>("info_panel");
            m_InfoPanel.visible = false;

            m_Image_Label = view.Q<Label>("image_label");

            m_Apply_Button =  view.Q<Button>("apply_button");
            m_Apply_Button.clicked += ()=> ApplyButtonClick?.Invoke();

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
                if(model == null || model.CurrentSpaceInfo == null)
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

        private void OnMGUIHandler()
        {
            if (GUILayout.Button("Upload Space Resources"))
                SpaceUploadButtonClick?.Invoke();
            if (GUILayout.Button("Delete Space"))
                DeleteSpaceButtonClick?.Invoke();
        }


        private VisualElement OnSpaceListMakeItem()
        {
            var itemView = new Label();
            itemView.style.unityTextAlign = TextAnchor.MiddleLeft;
            return itemView;
        }

        private void OnSpaceListBindItem(VisualElement element,int index)
        {
            var itemView = element as Label;
            var array = this.BindObject.FindProperty("m_SpaceList");
            if (index >= array.arraySize) //this is uielement bug ?
                itemView.visible = false;
            else
            {
                var spaceinfo = array.GetArrayElementAtIndex(index);
                itemView.text = spaceinfo.FindPropertyRelative("name").stringValue;
                itemView.visible = true;
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

        public void SetCoverImage(Texture2D image,string tip)
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
