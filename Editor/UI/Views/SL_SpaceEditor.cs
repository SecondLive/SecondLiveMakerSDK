using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditorInternal;
using SecondLive.Sdk.Sapce;
using System.Collections.Generic;

namespace SecondLive.Maker.Editor
{
    [CustomEditor(typeof(SL_Space))]
    public class SL_SpaceEditor : UnityEditor.Editor
    {

        SerializedProperty m_RespawnHeightY;

        readonly List<SL_SpawnPoint> m_SpawnPoints = new List<SL_SpawnPoint>();

        ReorderableList m_SpawnPointList;

        private void OnEnable()
        {
            m_RespawnHeightY = serializedObject.FindProperty("respawnHeightY");

            m_SpawnPoints.Clear();
            m_SpawnPoints.AddRange(((SL_Space)target).GetComponentsInChildren<SL_SpawnPoint>());

            m_SpawnPointList = new ReorderableList(m_SpawnPoints, typeof(SL_SpawnPoint));
            m_SpawnPointList.drawHeaderCallback = OnHeaderCallback;
            m_SpawnPointList.onSelectCallback += OnSelectionCallback;
            m_SpawnPointList.drawElementCallback = OnElementCallback;
            m_SpawnPointList.onAddCallback = OnAddSpawnPoint;
            m_SpawnPointList.onRemoveCallback = OnRemoveSpawnPoint;
            m_SpawnPointList.elementHeightCallback = ElementHeightCallbackDelegate;
        }

        public override VisualElement CreateInspectorGUI()
        {
            var rootElement = new VisualElement();

            var template = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/com.secondlive.maker/Editor/UI/Uxml/SpaceEditor.uxml");
            template.CloneTree(rootElement);

            var stylesheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Packages/com.secondlive.maker/Editor/UI/Uss/SpaceEditor.uss");
            rootElement.styleSheets.Add(stylesheet);

            var imgui = rootElement.Q<IMGUIContainer>("imgui");
            imgui.onGUIHandler = OnDrawIMGUI;

            return rootElement;
        }

        private void OnDrawIMGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(m_RespawnHeightY);
            m_SpawnPointList.DoLayoutList();
            serializedObject.ApplyModifiedProperties();
            if (GUI.changed)
            {
                SceneView.RepaintAll();
                EditorUtility.SetDirty(target);
            }
        }

        private void OnSceneGUI()
        {
            for (int i=0; i < m_SpawnPoints.Count; i++)
            {
                Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual;

                float alpha = (i == m_SpawnPointList.index)?0.8f:0.4f;
                Handles.color = new Color(0, 1, 0, alpha);
                Handles.DrawSolidDisc(m_SpawnPoints[i].transform.position, Vector3.up, m_SpawnPoints[i].maxRadius);

                Handles.color = new Color(1, 0, 0, alpha);
                Handles.DrawSolidDisc(m_SpawnPoints[i].transform.position, Vector3.up, m_SpawnPoints[i].minRadius);

                if(i == m_SpawnPointList.index)
                {
                    var position = Handles.PositionHandle(m_SpawnPoints[i].transform.position, m_SpawnPoints[i].transform.rotation);
                    m_SpawnPoints[i].transform.position = position;
                }
            }
        }

        private void OnHeaderCallback(Rect rect)
        {
            EditorGUI.LabelField(rect, "Spawn Point:");
        }

        private void OnSelectionCallback(ReorderableList list)
        {
            if(list.index > -1 && list.index < m_SpawnPoints.Count)
            {
                var point = m_SpawnPoints[list.index];
                SceneView.lastActiveSceneView.Frame(new Bounds(point.transform.position,Vector3.one * Mathf.Max(1.0f, point.maxRadius) * 2.5f ),false);
            }
            HandleUtility.Repaint();
            SceneView.RepaintAll();
        }

        private float ElementHeightCallbackDelegate(int index)
        {
            return EditorGUIUtility.singleLineHeight * 2 + 8;
        }

        private void OnElementCallback(Rect rect, int index, bool isActive, bool isFocused)
        {
            GUI.changed = false;
            var sp = m_SpawnPoints[index];

            Vector3 position = sp.transform.position;
            Vector3 eulerAngles = sp.transform.rotation.eulerAngles;
            Vector3 other = new Vector3(sp.minRadius, sp.maxRadius, eulerAngles.y);
            position = EditorGUI.Vector3Field(new Rect(rect.x ,rect.y + 2f, rect.width, (rect.height/2) -2f),"",position);
            other = EditorGUI.Vector3Field(new Rect(rect.x, rect.y + 2f + EditorGUIUtility.singleLineHeight, rect.width, (rect.height / 2) - 2f), "", other);
            sp.minRadius = Mathf.Min(sp.maxRadius - 0.1f, other.x);
            sp.maxRadius = Mathf.Max(sp.minRadius + 0.1f, other.y);
            sp.transform.position = position;
            sp.transform.rotation = Quaternion.Euler(new Vector3(eulerAngles.x,other.z,eulerAngles.z));

            if (GUI.changed)
            {
                EditorUtility.SetDirty(sp);
            }
        }

        private void OnAddSpawnPoint(ReorderableList list)
        {
            var go = new GameObject($"Spawn Point");
            go.transform.SetParent(((SL_Space)target).transform);
            var sp = go.AddComponent<SL_SpawnPoint>();
            sp.gameObject.hideFlags = HideFlags.HideInHierarchy;
            sp.minRadius = 0.5f;
            sp.maxRadius = 1.2f;
            list.list.Add(sp);
        }

        private void OnRemoveSpawnPoint(ReorderableList list)
        {
            var spawnPoint = (SL_SpawnPoint)list.list[list.index];
            list.list.RemoveAt(list.index);
            Object.DestroyImmediate(spawnPoint.gameObject);
        }
    }
}