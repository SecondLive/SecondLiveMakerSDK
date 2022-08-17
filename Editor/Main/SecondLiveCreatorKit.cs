using SecondLive.Sdk.Sapce;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.PackageManager.UI;
using UnityEngine;
using UnityEngine.Networking;

namespace SecondLive.Maker.Editor
{
    [InitializeOnLoad]
    public class SecondLiveCreatorKit
    {
        public static SecondLiveCreatorKit Instance { get; private set; }

        static SecondLiveCreatorKit()
        {
            Instance = new SecondLiveCreatorKit();
            EditorApplication.update += Instance.OnUpdate;
            EditorApplication.playModeStateChanged += Instance.OnPerview;
        }

        [InitializeOnLoadMethod]
        static void OnProjectLoadedInEditor()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                /* var space = GameObject.FindObjectOfType<SL_Space>();
                 if (space == null)
                     return;
                 var go = new GameObject("Preview");
                 go.SetActive(false);
                 var preview = go.AddComponent<SLPreview>();
                 preview.space = space;
                 go.SetActive(true);*/
            }
        }

        void OnUpdate()
        {
        }

        void OnPerview(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                var space = GameObject.FindObjectOfType<SL_Space>();
                 if (space == null)
                     return;
                 var go = new GameObject("Preview");
                 go.SetActive(false);
                 var preview = go.AddComponent<SLPreview>();
                 preview.space = space;
                 go.SetActive(true);
            }
        }
    }
}
