using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace SecondLive.Maker.Editor
{
    public class AccountWindow : BaseWindow
    {
        VisualElement m_LoginVisualElement;
        VisualElement m_LogoutVisualElement;

        TextField m_TokenBinding;

        public event UnityAction GetTokenButtonClick;
        public event UnityAction LoginButtonClick;
        public event UnityAction SpaceManageButtonClick;
        public event UnityAction LogoutButtonClick;

        private void CreateGUI()
        {
            var template = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/com.secondlive.maker/Editor/UI/Uxml/AccountWindow.uxml");
            var view = template.CloneTree();


            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Packages/com.secondlive.maker/Editor/UI/Uss/AccountWindow.uss");
            view.styleSheets.Add(styleSheet);


            m_LoginVisualElement = view.Q<VisualElement>("login_panel");
            m_LogoutVisualElement = view.Q<VisualElement>("logout_panel");

            var getTokenButton = view.Q<Button>("get_token_button");
            getTokenButton.text = Define.Text.GET_DESIGNER_TOKEN;
            getTokenButton.clickable.clicked += () => GetTokenButtonClick?.Invoke();

            var inputTokenTip = view.Q<Label>("token_input_tip");
            inputTokenTip.text = Define.Text.INPUT_TOKEN_TIP;

            var loginButton = view.Query<Button>("login_button").First();
            loginButton.clicked += () => LoginButtonClick?.Invoke();
            loginButton.SetEnabled(false);

            m_TokenBinding = view.Query<TextField>("token_input").First();
            m_TokenBinding.RegisterValueChangedCallback(ev =>
            {
                loginButton.SetEnabled(!string.IsNullOrEmpty(ev.newValue));
            });

            var tokenLabel = view.Q<Label>("token_label");
            tokenLabel.bindingPath = "m_Token";

            var usernameLabel = view.Q<Label>("username_label");
            usernameLabel.bindingPath = "m_Username";

            var spaceManageButton = view.Q<Button>("spaceManage_button");
            spaceManageButton.clicked += ()=> { SpaceManager.instance.ShowSpacePanel(); this.Close(); } ;

            var logoutButton = view.Q<Button>("logout_button");
            logoutButton.clicked += () => LogoutButtonClick?.Invoke();

            m_LoginVisualElement.style.display = UserManager.instance.UserInfo.Logined ? DisplayStyle.None : DisplayStyle.Flex;
            m_LogoutVisualElement.style.display = UserManager.instance.UserInfo.Logined ? DisplayStyle.Flex : DisplayStyle.None;

            rootVisualElement.Clear();
            rootVisualElement.Add(view);
        }

        public override void Rebuild()
        {
            base.Rebuild();

            m_LoginVisualElement.style.display = UserManager.instance.UserInfo.Logined ? DisplayStyle.None : DisplayStyle.Flex;
            m_LogoutVisualElement.style.display = UserManager.instance.UserInfo.Logined ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
}
