using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecondLive.Maker.Editor
{
    [Serializable]
    public class GetSpaceInfos : BaseResponse
    {
        public SpaceInfo[] data;
    }
}
