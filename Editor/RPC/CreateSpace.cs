using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecondLive.Maker.Editor
{
    
    [Serializable]
    public class CreateSpaceRequest
    {
        public UInt64 guid;
        public string name;
        public string introduce;
    }
    
    [Serializable]
    public class CreateSpaceResponse : BaseResponse
    {
        public SpaceInfo data;
    }
}
