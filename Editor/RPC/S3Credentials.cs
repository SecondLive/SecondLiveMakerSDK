
using System;

namespace SecondLive.Maker.Editor
{
    
    public class S3CredentialsRequest
    {
        public UInt64 space_guid;
    }
    
    public class S3CredentialsResponse : BaseResponse
    {
        public S3Credentials data;
    }

    public class S3Credentials
    {
        public string accessKey;
        public long expire;
        public int remainingTimes;
        public string secret;
        public string token;
        public string bucket;
        public string dir;
        public UInt64 spaceId;
    }
}