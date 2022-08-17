using System;
using System.Collections.Generic;

namespace SecondLive.Maker.Editor
{
    [Serializable]
    public class SpaceInfo
    {
        public ulong guid;
        public string name;
        public string introduce;
        public SpaceStatus status;
        public string image_url;
        public long create_time;

        public enum SpaceStatus
        {
            Upload = 0,
            Pending = 4,
            Reviewing = 1,
            Approved = 2,
            ReviewFailed = 3
        }
    }
}