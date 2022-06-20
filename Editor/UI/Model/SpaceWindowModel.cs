using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SecondLive.Maker.Editor
{
    public class SpaceWindowModel : ScriptableObject
    {
        [SerializeField] List<SpaceInfo> m_SpaceList = new List<SpaceInfo>();
        [SerializeField] string m_NameInput;
        [SerializeField] string m_IntroduceInput;
        [SerializeField] string m_Guid;

        public SpaceInfo[] SpaceInfos
        {
            get => m_SpaceList.ToArray();
            set
            {
                m_SpaceList.Clear();
                m_SpaceList.AddRange(value);
            }
        }

        public string Guid
        {
            get => m_Guid;
            set => m_Guid = value;
        }

        public string NameInput
        {
            get => m_NameInput;
            set => m_NameInput = value;
        }

        public string IntroduceInput
        {
            get => m_IntroduceInput;
            set => m_IntroduceInput = value;
        }

        public SpaceInfo CurrentSpaceInfo { get; set; }

        public void AddSpace(SpaceInfo space)
        {
            m_SpaceList.Add(space);
            m_SpaceList.Sort(new SpaceListComparer());
        }

        public void RemoveSpace(SpaceInfo space)
        {
            m_SpaceList.Remove(space);
        }

        private class SpaceListComparer : IComparer<SpaceInfo>
        {
            public int Compare(SpaceInfo x, SpaceInfo y)
            {
                if (x.create_time < y.create_time)
                    return -1;
                else if (x.create_time > y.create_time)
                    return 1;
                return 0;
            }
        }
    }
}

