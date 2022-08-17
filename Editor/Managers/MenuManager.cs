using SecondLive.Sdk.Sapce;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace SecondLive.Maker.Editor
{
    public class MenuManager
    {
        private const string SECONDLIVE_MAKER_MENU = "SecondLive";

        private const string SECONDLIVE_CREATE_SPACE = "GameObject/" + SECONDLIVE_MAKER_MENU + "/Space";
        private const string SECONDLIVE_ADD_FRAME = "GameObject/" + SECONDLIVE_MAKER_MENU + "/Frame";
        private const string SECONDLIVE_ADD_ANIMATOR_PEDESTAL = "GameObject/" + SECONDLIVE_MAKER_MENU + "/AnimatorPedestal";

        private const string SECONDLIVE_AUTHORIZATION = SECONDLIVE_MAKER_MENU + "/Authorization";
        private const string SECONDLIVE_SPACE_PANEL = SECONDLIVE_MAKER_MENU + "/Space Panel";
        private const string SECONDLIVE_CREATOR_ABOUT = SECONDLIVE_MAKER_MENU + "/About";


        [MenuItem(SECONDLIVE_CREATE_SPACE, false, 1)]
        private static void CreateSpace()
        {
            var space = GameObject.FindObjectOfType<SL_Space>();
            if (space != null)
            {
                EditorUtility.DisplayDialog(Define.Text.DIALOG_TITLE_ERROR,
                    Define.Text.CREATE_SPACE_ALREADY_EXISTS,
                    Define.Text.DIALOG_BUTTON_OK);
            }
            else
            {
                space = SpaceManager.instance.CreateSpace();
            }
            Selection.activeObject = space;
        }



        [MenuItem(SECONDLIVE_ADD_FRAME, false, 1)]
        private static void AddFrame()
        {
            SpaceManager.instance.AddFrame();
        }

        [MenuItem(SECONDLIVE_ADD_ANIMATOR_PEDESTAL, false, 1)]
        private static void AddAnimatorPedestal()
        {
            SpaceManager.instance.AddAnimatorPedestal();
        }

        [MenuItem(SECONDLIVE_AUTHORIZATION, false, 100)]
        private static void ShowAccountWindow()
        {
            UserManager.instance.ShowAccountWindow();
        }

        [MenuItem(SECONDLIVE_SPACE_PANEL, true, 101)]
        private static bool CheckShowSpacePanel()
        {
            return UserManager.instance.UserInfo.Logined;
        }

        [MenuItem(SECONDLIVE_SPACE_PANEL, false, 101)]
        private static void ShowSpacePanel()
        {
            SpaceManager.instance.ShowSpacePanel();
        }

        [MenuItem(SECONDLIVE_CREATOR_ABOUT, false)]
        private static void About()
        {
            EditorWindow.GetWindow<AboutWindow>(Define.Text.ABOUT_WINDOW_NAME);
        }
    }
}
