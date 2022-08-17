using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditor.UIElements;
using System;

namespace SecondLive.Maker.Editor
{
    public class UserManager : ScriptableSingleton<UserManager>
    {
        [SerializeField] UserInfo m_UserInfo = new UserInfo();

        public UserInfo UserInfo => m_UserInfo;

        public void ShowAccountWindow()
        {
            var viewModel = CreateInstance<AccountWindowModel>();
            viewModel.Token = m_UserInfo.Token;
            viewModel.Username = m_UserInfo.Username;
            viewModel.Message = string.Empty;

            var window = EditorWindow.GetWindow<AccountWindow>(Define.Text.ACCOUNT_WINDOW_NAME);
            window.minSize = new Vector2(805, 630);
            window.maxSize = new Vector2(805, 630);
            window.Bind(viewModel);
            window.LoginButtonClick += async () =>
            {
                viewModel.Message = Define.Text.CHECKING_TOKEN;
                var response = await RPCService.instance.Login(viewModel.Token);
                if (response.code == 0)
                {
                    m_UserInfo.Token = viewModel.Token;
                    m_UserInfo.Username = response.data.name;
                    viewModel.Username = m_UserInfo.Username;

                    viewModel.Message = Define.Text.LOADING_SPACE_INFORMATION;
                    var gresponse = await RPCService.instance.GetSpaceInfos();
                    if (gresponse.code == 0)
                    {
                        m_UserInfo.SpaceInfos = gresponse.data;
                        m_UserInfo.Logined = true;
                        viewModel.Message = string.Empty;
                        window.Rebuild();
                        //EditorUtility.DisplayDialog("Welcome", "TestTestTestTestTestTest", "OK");
                    }
                    else
                    {
                        viewModel.Message = string.Format(Define.Text.ERROR, gresponse.message);
                    }
                }
                else
                {
                    viewModel.Message = string.Format(Define.Text.ERROR, response.message);
                }
            };

            window.LogoutButtonClick += () =>
            {
                m_UserInfo.Logined = false;
                m_UserInfo.Token = string.Empty;
                m_UserInfo.SpaceInfos = null;
                window.Rebuild();
                EditorWindow.GetWindow<SpaceWindow>(Define.Text.SPACE_WINDOW_NAME).Close();
            };


            window.GetTokenButtonClick += () =>
            {
                Application.OpenURL(Define.Url.SECRETKEY_URL);
            };

            window.Destroyed += () =>
            {
                UnityEngine.Object.DestroyImmediate(viewModel);
            };

        }
    }

}
