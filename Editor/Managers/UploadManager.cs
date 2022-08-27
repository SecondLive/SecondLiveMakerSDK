
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using UnityEditor;
using UnityEngine;

namespace SecondLive.Maker.Editor
{
    public class UploadManager : ScriptableSingleton<UploadManager>
    {
        public delegate void UploadProgressCallBack(long total, long current);
        public delegate void UploadErrorCallBack(string message);
        
        [SerializeField]
        private S3Credentials m_S3Credentials;

        public async Task<S3Credentials> CheckS3Credentials(UInt64 spaceid)
        {
            if (m_S3Credentials == null || m_S3Credentials.expire < SystemHelper.NowTimeStamp())
            {
                var s3 = await RPCService.instance.GetS3Credentials(spaceid);
                if (s3.code != 0)
                {
                    EditorUtility.DisplayDialog("Upload Error", s3.message, "OK");
                    return null;
                }

                if (s3.data.remainingTimes == 0)
                {
                    EditorUtility.DisplayDialog("Upload Error", Define.Text.MAX_UPLOAD_S3_COUNT, "OK");
                    return null;
                }

                m_S3Credentials = s3.data;
            }
            return m_S3Credentials;
        }
        
        public async Task<string> UploadFileToS3Async(string localFilePath,
            S3Credentials s3Credentials,UploadProgressCallBack progressCallback,
            UploadErrorCallBack errorCallback)
        {
            using(var inputStream = new FileStream(localFilePath,FileMode.Open))
            {
                var fileLength = inputStream.Length;
                progressCallback?.Invoke(inputStream.Length, 0);
                var targetFileName = Path.GetFileName(localFilePath);
                var targetFilePath = s3Credentials.dir + $"/{targetFileName}";
                Debug.Log($"Upload {localFilePath} to s3://{targetFilePath}");
                
                var partMinSize = 5L * 1024 * 1024;
                var filePosition = 0L;
                var partTags = new List<PartETag>();
                var awsCredentials = new SessionAWSCredentials(s3Credentials.accessKey,s3Credentials.secret,s3Credentials.token); 
                var client = new AmazonS3Client(awsCredentials,Amazon.RegionEndpoint.USEast1);

                var initRequest = new InitiateMultipartUploadRequest
                {
                    BucketName = s3Credentials.bucket,
                    Key = targetFilePath
                };
                
                var initResponse = await client.InitiateMultipartUploadAsync(initRequest);
                var number = 1;
                while (filePosition < fileLength)
                {
                    var partSize = Math.Min(partMinSize, fileLength - filePosition);
                    var uploadRequest = new UploadPartRequest
                    {
                        BucketName = s3Credentials.bucket,
                        Key = targetFilePath,
                        UploadId = initResponse.UploadId,
                        PartNumber = number,
                        PartSize = partSize,
                        InputStream = inputStream,
                        
                    };
                    var uploadResponse = await client.UploadPartAsync(uploadRequest);
                    if (uploadResponse.HttpStatusCode != HttpStatusCode.OK)
                    {
                        errorCallback?.Invoke($"Upload {localFilePath} error!");
                        return null;
                    }

                    partTags.Add(new PartETag(uploadResponse));
                    filePosition += partSize;
                    number++;
                    progressCallback?.Invoke(fileLength, filePosition);
                }
                
                var compRequest = new CompleteMultipartUploadRequest
                {
                    BucketName = s3Credentials.bucket,
                    Key = targetFilePath,
                    UploadId = initResponse.UploadId,
                    PartETags = partTags
                };
                var compResponse = await  client.CompleteMultipartUploadAsync(compRequest);
                client.Dispose();

                return compResponse.Location.Replace("metaverses.s3.amazonaws.com", "d31usbtacmd2ho.cloudfront.net");
            }
        }
    }
}