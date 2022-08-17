using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace SecondLive.Maker.Editor
{
    public class GetSpaceConfigRequest
    {
        public UInt64 guid;
    }

    public class GetSpaceConfig: BaseResponse
    {
        public string data;
    }
}