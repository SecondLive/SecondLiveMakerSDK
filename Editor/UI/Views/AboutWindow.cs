using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace SecondLive.Maker.Editor
{
    public class AboutWindow : BaseWindow
    {
        StyleLength minWidth = new StyleLength(500), minLength = new StyleLength(450);

        private void CreateGUI()
        {
            titleContent = new GUIContent("About");

            var template = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/com.secondlive.maker/Editor/UI/Uxml/AboutWindow.uxml");
            var view = template.CloneTree();
            
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Packages/com.secondlive.maker/Editor/UI/Uss/AboutWindow.uss");
            view.styleSheets.Add(styleSheet);

            view.Query<Button>("open-document").ForEach(b =>
                b.clickable.clicked += () => Application.OpenURL("https://secondlive.world/"));
            view.Query<Button>("open-example").ForEach(b =>
                b.clickable.clicked += () => Application.OpenURL("https://secondlive.world/"));
            view.Query<Button>("join_discord").ForEach(b =>
                b.clickable.clicked += () => Application.OpenURL("https://secondlive.world/"));

            rootVisualElement.Clear();
            rootVisualElement.Add(view);

            minSize = new Vector2(805, 355);
            maxSize = new Vector2(805, 355);
        }
    }
}
