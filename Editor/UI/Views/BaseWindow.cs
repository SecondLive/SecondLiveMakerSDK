using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace SecondLive.Maker.Editor
{
    public abstract class BaseWindow : EditorWindow
    {
        public event Action Destroyed;

        private SerializedObject m_SerializedObject;

        public SerializedObject BindObject => m_SerializedObject;



        void OnDestroy()
        {
            m_SerializedObject?.Dispose();
            Destroyed?.Invoke();
        }

        public virtual void Rebuild()
        {
            m_SerializedObject.Update();
        }

        public void Bind(UnityEngine.Object obj)
        {
            m_SerializedObject = new SerializedObject(obj);
            rootVisualElement.Bind(m_SerializedObject);
        }
    }
}
