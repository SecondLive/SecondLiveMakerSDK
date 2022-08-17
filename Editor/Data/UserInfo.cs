using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SecondLive.Maker.Editor
{
    [Serializable]
    public class UserInfo
    {

        [SerializeField] string m_Token;
        [SerializeField] string m_Username;
        [SerializeField] bool m_Logined;

        private SpaceInfo[] m_SpaceInfos;

        public string Token
        {
            get => m_Token;
            set => m_Token = value;
        }

        public string Username
        {
            get => m_Username;
            set => m_Username = value;
        }

        public bool Logined
        {
            get => m_Logined;
            set => m_Logined = value;
        }

        public SpaceInfo[] SpaceInfos
        {
            get => m_SpaceInfos;
            set => m_SpaceInfos = value;
        }
    }
}
