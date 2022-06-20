using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SecondLive.Maker.Editor
{
    public class AboutWindow : EditorWindow
    {

        private void CreateGUI()
        {
            titleContent = new GUIContent("About");

            var template = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/com.secondlive.maker/Editor/Windows/Uxml/AboutWindow.uxml");
            var view = template.CloneTree();

            view.Query<Button>("open-document").ForEach(b =>
                b.clickable.clicked += () => Application.OpenURL("https://secondlive.world/"));
            view.Query<Button>("open-creators-guide").ForEach(b =>
                b.clickable.clicked += () => Application.OpenURL("https://secondlive.world/"));
            //view.Query<Button>("open-settings-window").ForEach(b => b.clickable.clicked += SettingsWindow.ShowWindow);

            rootVisualElement.Clear();
            rootVisualElement.Add(view);
        }

         void OnEnable()
         {
         }

        /* void AwaitRefreshingAndCreateView()
         {
             EditorApplication.update -= AwaitRefreshingAndCreateView;

             if (EditorApplication.isUpdating)
             {
                 EditorApplication.update += AwaitRefreshingAndCreateView;
                 return;
             }

             rootVisualElement.Clear();
             rootVisualElement.Add(CreateView());
         }

         static VisualElement CreateView()
         {
             var template = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                 "Packages/mu.cluster.cluster-creator-kit/Editor/Window/Uxml/AboutWindow.uxml");
             VisualElement view = template.CloneTree();
             var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(
                 "Packages/mu.cluster.cluster-creator-kit/Editor/Window/Uss/AboutWindow.uss");
             view.styleSheets.Add(styleSheet);

             view.Query<Button>("open-document").ForEach(b =>
                 b.clickable.clicked += () => Application.OpenURL("https://docs.cluster.mu/creatorkit/"));
             view.Query<Button>("open-creators-guide").ForEach(b =>
                 b.clickable.clicked += () => Application.OpenURL("https://creator.cluster.mu/"));
             view.Query<Button>("open-settings-window").ForEach(b => b.clickable.clicked += SettingsWindow.ShowWindow);

             return view;
         }*/
    }
}
