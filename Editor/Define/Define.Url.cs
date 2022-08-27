using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecondLive.Maker.Editor
{
    public static partial class Define
    {
        public static class Url
        {
            public const string URL = "https://secondlive.world";
            public const string SECRETKEY_URL = "https://secondlive.world/secretkey";
            public const string API_URL = "https://api.secondlive.world/backend";
            public const string LOGIN_URL = API_URL + "/v1/account/info";
            public const string GET_SPACES_URL = API_URL + "/v1/editor/space/list";
            public const string CREATE_SPACE_URL = API_URL + "/v1/editor/space/save";
            public const string UPLOAD_COVER_URL = API_URL + "/v1/editor/space/upload-cover";
            public const string UPLOAD_RESOURCE_URL = API_URL + "/v1/editor/space/file/upload-resource";
            public const string UPLOAD_CONFIG_URL = API_URL + "/v1/editor/space/file/upload-config";
            public const string REMOVE_SPACE_URL = API_URL + "/v1/editor/space/delete";
            public const string DELETE_SPACE_FILE_URL = API_URL + "/v1/editor/space/file/delete";
            public const string GET_S3_CREDENTIALS = API_URL + "/v1/editor/getS3TempCredentials";
            public static string GET_SPACE_CONFIG_URL => API_URL + "/v1/editor/space/config-file";

        }
    }
}
