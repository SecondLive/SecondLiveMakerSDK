using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SecondLive.Maker.Editor
{
    public class AccountWindowModel : ScriptableObject
    {

        [SerializeField] string m_Token;
        [SerializeField] string m_Message;
        [SerializeField] string m_Username;

        public string Token
        {
            get => m_Token;
            set => m_Token = value;
        }

        public string Message
        {
            get => m_Message;
            set => m_Message = value;
        }

        public string Username
        {
            get => m_Username;
            set => m_Username = value;
        }
    }
}
