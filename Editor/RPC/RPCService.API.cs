using LitJson;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace SecondLive.Maker.Editor
{
    public partial class RPCService : ScriptableSingleton<RPCService>
    {

        public Task<LoginResponse> Login(string token)
        {
            return Call<LoginResponse>(Define.Url.LOGIN_URL,"GET",token,null);
        }

        public Task<GetSpaceInfos> GetSpaceInfos()
        {
            return Call<GetSpaceInfos>(Define.Url.GET_SPACES_URL, "POST", UserManager.instance.UserInfo.Token, null);
        }

        public Task<CreateSpaceResponse> CreateSpace()
        {
            var json = JsonMapper.ToJson(new CreateSpaceRequest
            {
                guid = 0,
                name = "Unnamed",
                introduce = string.Empty
            });
            return Call<CreateSpaceResponse>(Define.Url.CREATE_SPACE_URL, "POST", UserManager.instance.UserInfo.Token, json);
        }

        public Task<CreateSpaceResponse> ApplySpaceInfo(UInt64 guid,string name,string introduce)
        {
            var json = JsonMapper.ToJson(new CreateSpaceRequest
            {
                guid = guid,
                name = name,
                introduce = introduce
            });
            return Call<CreateSpaceResponse>(Define.Url.CREATE_SPACE_URL, "POST", UserManager.instance.UserInfo.Token, json);
        }

        public Task<UploadResponse> UploadCoverImage(UInt64 guid,string filename, Action<float> progressCallback = null)
        {
            var form = new WWWForm();
            form.AddBinaryData("file", File.ReadAllBytes(filename),Path.GetFileName(filename));
            form.AddField("space_guid",guid.ToString());
            return Call<UploadResponse>(Define.Url.UPLOAD_COVER_URL, UserManager.instance.UserInfo.Token, form,progressCallback);
        }
        
        public Task<S3CredentialsResponse> GetS3Credentials(UInt64 guid)
        {
            var json = JsonMapper.ToJson(new S3CredentialsRequest
            {
                space_guid = guid
            });
            return Call<S3CredentialsResponse>(Define.Url.GET_S3_CREDENTIALS, "POST", UserManager.instance.UserInfo.Token, json);
        }

        public Task<BaseResponse> UploadSceneConfigBundle(UInt64 guid, string config)
        {
            var json = JsonMapper.ToJson(new UploadConfigeRequest
            {
                space_guid = guid,
                config = config
            });
            return Call<BaseResponse>(Define.Url.UPLOAD_CONFIG_URL,  "POST",UserManager.instance.UserInfo.Token, json);
        }

        public Task<BaseResponse> DeleteSpace(UInt64 guid, Action<float> progressCallback = null)
        {
            var json = JsonMapper.ToJson(new DeleteSpaceRequest
            {
                space_guid = guid
            });
            return Call<BaseResponse>(Define.Url.REMOVE_SPACE_URL,"POST",UserManager.instance.UserInfo.Token,json);
        }

        public Task<GetSpaceConfig> GetSpaceConfig(UInt64 guid)
        {
            var json = JsonMapper.ToJson(new GetSpaceConfigRequest()
            {
                guid = guid,
            });
            return Call<GetSpaceConfig>(Define.Url.GET_SPACE_CONFIG_URL, "POST", UserManager.instance.UserInfo.Token, json);
        }

        public Task<BaseResponse> DeleteSpaceFile(UInt64 spaceId,int fileId,Action<float> progressCallback = null)
        {
            var json = JsonMapper.ToJson(new
            {
                space_guid = spaceId,
                file_id = fileId,
            });
            return Call<BaseResponse>(Define.Url.DELETE_SPACE_FILE_URL, "POST", UserManager.instance.UserInfo.Token, json);
        }
    }
}
