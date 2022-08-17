using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecondLive.Maker.Editor
{
    [Serializable]
    public class LoginResponse
    {
        public int code;
        public string message;
        public UserInfo data;

        [Serializable]
        public class UserInfo
        {
            public string name;
            public int update_times;
        }
    }
}
