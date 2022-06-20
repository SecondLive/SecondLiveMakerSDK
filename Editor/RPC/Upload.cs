using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecondLive.Maker.Editor
{
    public class UploadResponse : BaseResponse
    {
        public string data;
    }
    
    public class UploadConfigeRequest
    {
        public UInt64 space_guid;
        public string config;
    }
}