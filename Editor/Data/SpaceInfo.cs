using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecondLive.Maker.Editor
{
    [Serializable]
    public class SpaceInfo
    {
        public UInt64 guid;
        public string name;
        public string introduce;
        public string image_url;
        public long create_time;
    }
}
