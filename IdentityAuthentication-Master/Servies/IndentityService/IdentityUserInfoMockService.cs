// Service 层
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Text.Json;
using IdentityAuthentication_Master.Models.DTO;
using Microsoft.Extensions.Configuration;

namespace IdentityAuthentication_Master.Servies.IndentityService
{
    public interface IIdentityUserInfoMockService
    {
        IdentityVerificationResultDTO VerifyIdentity(string name, string idCard);
    }

    public class IdentityUserInfoMockServiceImpl : IIdentityUserInfoMockService
    {
        private readonly HttpClient _httpClient;
        private readonly string _secretId;
        private readonly string _secretKey;
        private const string Service = "faceid";
        private const string Version = "2018-03-01";
        private const string Action = "IdCardVerification";
        private const string Host = "faceid.tencentcloudapi.com";

        public IdentityUserInfoMockServiceImpl(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _secretId = configuration["SecretId"] ?? throw new ArgumentNullException("SecretId");
            _secretKey = configuration["SecretKey"] ?? throw new ArgumentNullException("SecretKey");
            _httpClient = httpClientFactory.CreateClient();
            _httpClient.BaseAddress = new Uri($"https://{Host}");
        }

        private Random _random = new Random();

        public IdentityVerificationResultDTO VerifyIdentity(string name, string idCard)
        {
            // 随机生成一个结果
            int randomValue = _random.Next(0, 2);

            var content = new IdentityVerificationResultDTO
            {
                RequestId = Guid.NewGuid().ToString() 
            };

            if (randomValue == 0)
            {
                content.ResultCode = "0";
                content.Description = "姓名和身份证号一致";
            }
            else
            {
                content.ResultCode = "-1";
                content.Description = "姓名和身份证号不一致";
            }
            return content;
        }


    }
}