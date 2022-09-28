using SecondLive.Sdk.Sapce;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SecondLive.Maker.Editor
{
    [CustomEditor(typeof(SL_VideoScreen))]
    public class SL_VideoScreenEditor : UnityEditor.Editor
    {
        
        SerializedProperty _mTargetProperty;
        SerializedProperty _mDefaultCoverImageProperty;
        SerializedProperty _mRawImageProperty;
        SerializedProperty _mRendererProperty;
        SerializedProperty _mMaterialIndexProperty;
        SerializedProperty _mTextureNameProperty;

        private void OnEnable()
        {
            _mTargetProperty = serializedObject.FindProperty("target");
            _mDefaultCoverImageProperty = serializedObject.FindProperty("defaultCoverImage");
            _mRawImageProperty = serializedObject.FindProperty("rawImage");
            _mRendererProperty = serializedObject.FindProperty("Renderer");
            _mMaterialIndexProperty = serializedObject.FindProperty("materialIndex");
            _mTextureNameProperty = serializedObject.FindProperty("texturePropertyName");
        }

        public override VisualElement CreateInspectorGUI()
        {
            var rootElement = new VisualElement();

            var template = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/com.secondlive.maker/Editor/UI/Uxml/VideoScreenEditor.uxml");
            template.CloneTree(rootElement);

            var stylesheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Packages/com.secondlive.maker/Editor/UI/Uss/VideoScreenEditor.uss");
            rootElement.styleSheets.Add(stylesheet);

            var imgui = rootElement.Q<IMGUIContainer>("imgui");
            imgui.onGUIHandler = OnDrawIMGUI;

            return rootElement;
        }

        private void OnDrawIMGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(_mTargetProperty);
            EditorGUILayout.PropertyField(_mDefaultCoverImageProperty);

            var target = Enum.GetValues(typeof(VideoScreenTarget)).GetValue(_mTargetProperty.enumValueIndex);
            switch (target)
            {
                case VideoScreenTarget.UGUI:
                    {
                        EditorGUILayout.PropertyField(_mRawImageProperty);
                    }
                    break;
                case VideoScreenTarget.Renderer:
                    {
                        EditorGUILayout.PropertyField(_mRendererProperty);
                        EditorGUILayout.PropertyField(_mMaterialIndexProperty);
                        EditorGUILayout.PropertyField(_mTextureNameProperty);
                    }
                    break;
            }

            if (serializedObject.ApplyModifiedProperties())
            {
            }
        }
    }
}