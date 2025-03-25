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
    public interface IIdentityUserInfoService
    {
        Task<IdentityVerificationResultDTO> VerifyIdentityAsync(string name, string idCard);
    }

    public class IdentityUserInfoService : IIdentityUserInfoService
    {
        private readonly HttpClient _httpClient;
        private readonly string _secretId;
        private readonly string _secretKey;
        private const string Service = "faceid";
        private const string Version = "2018-03-01";
        private const string Action = "IdCardVerification";
        private const string Host = "faceid.tencentcloudapi.com";

        public IdentityUserInfoService(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _secretId = configuration["SecretId"] ?? throw new ArgumentNullException("SecretId");
            _secretKey = configuration["SecretKey"] ?? throw new ArgumentNullException("SecretKey");
            _httpClient = httpClientFactory.CreateClient();
            _httpClient.BaseAddress = new Uri($"https://{Host}");
        }

        public async Task<IdentityVerificationResultDTO> VerifyIdentityAsync(string name, string idCard)
        {
            var requestBody = BuildRequestBody(name, idCard);
            var request = BuildRequestMessage(requestBody);

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            return ParseResponse(content);
        }

        private static StringContent BuildRequestBody(string name, string idCard)
        {
            var body = new
            {
                Name = name,
                IdCard = idCard
            };
            return new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
        }

        private HttpRequestMessage BuildRequestMessage(StringContent content)
        {
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
            var auth = GenerateAuthorizationHeader(timestamp, content);

            var request = new HttpRequestMessage(HttpMethod.Post, "/")
            {
                Content = content
            };

            request.Headers.Authorization = new AuthenticationHeaderValue("TC3-HMAC-SHA256", auth);

            request.Headers.Add("Host", Host);
            request.Headers.Add("X-TC-Timestamp", timestamp);
            request.Headers.Add("X-TC-Version", Version);
            request.Headers.Add("X-TC-Action", Action);
            //request.Headers.Add("Authorization", auth);

            return request;
        }

        //private string GenerateAuthorizationHeader(string timestamp, HttpContent content)
        //{
        //    var canonicalRequest = BuildCanonicalRequest(content);
        //    var stringToSign = BuildStringToSign(timestamp, canonicalRequest);
        //    var signature = GenerateSignature(stringToSign);

        //    return $"TC3-HMAC-SHA256 Credential={_secretId}/{GetCredentialScope(timestamp)}, SignedHeaders=content-type;host, Signature={signature}";
        //}

        private string GenerateAuthorizationHeader(string timestamp, HttpContent content)
        {
            var canonicalRequest = BuildCanonicalRequest(content);
            var stringToSign = BuildStringToSign(timestamp, canonicalRequest);
            var signature = GenerateSignature(stringToSign, timestamp); // 传入时间戳

            // ✅ 移除算法前缀，仅保留凭证部分
            return $"Credential={_secretId}/{GetCredentialScope(timestamp)}, SignedHeaders=content-type;host, Signature={signature}";
        }



        private string BuildCanonicalRequest(HttpContent content)
        {
            var contentType = content.Headers.ContentType?.ToString() ?? "application/json";
            var hashedPayload = ComputeSha256Hash(content.ReadAsStringAsync().Result);

            return $"POST\n/\n\ncontent-type:{contentType}\nhost:{Host}\n\ncontent-type;host\n{hashedPayload}";
        }

        //private string BuildStringToSign(string timestamp, string canonicalRequest)
        //{
        //    var date = DateTimeOffset.FromUnixTimeSeconds(long.Parse(timestamp)).UtcDate.ToString("yyyy-MM-dd");
        //    var credentialScope = $"{date}/{Service}/tc3_request";
        //    var hashedCanonicalRequest = ComputeSha256Hash(canonicalRequest);

        //    return $"TC3-HMAC-SHA256\n{timestamp}\n{credentialScope}\n{hashedCanonicalRequest}";
        //}

        private string BuildStringToSign(string timestamp, string canonicalRequest)
        {
            var date = DateTimeOffset.FromUnixTimeSeconds(long.Parse(timestamp)).ToString("yyyy-MM-dd");
            var credentialScope = $"{date}/faceid/tc3_request"; // ✅ 硬编码服务名
            var hashedCanonicalRequest = ComputeSha256Hash(canonicalRequest);

            return $"TC3-HMAC-SHA256\n{timestamp}\n{credentialScope}\n{hashedCanonicalRequest}";
        }

        //private string GenerateSignature(string stringToSign)
        //{
        //    var date = DateTime.UtcNow.ToString("yyyy-MM-dd");
        //    var secretKey = Encoding.UTF8.GetBytes("TC3" + _secretKey);
        //    var secretDate = ComputeHmacSha256Hash(secretKey, Encoding.UTF8.GetBytes(date));
        //    var secretService = ComputeHmacSha256Hash(secretDate, Encoding.UTF8.GetBytes(Service));
        //    var secretSigning = ComputeHmacSha256Hash(secretService, Encoding.UTF8.GetBytes("tc3_request"));
        //    var signatureBytes = ComputeHmacSha256Hash(secretSigning, Encoding.UTF8.GetBytes(stringToSign));

        //    return BitConverter.ToString(signatureBytes).Replace("-", "").ToLower();
        //}

        private string GenerateSignature(string stringToSign, string timestamp)
        {
            // ✅ 基于时间戳生成日期
            var date = DateTimeOffset.FromUnixTimeSeconds(long.Parse(timestamp)).ToString("yyyy-MM-dd");

            var secretKey = Encoding.UTF8.GetBytes("TC3" + _secretKey);
            var secretDate = ComputeHmacSha256Hash(secretKey, Encoding.UTF8.GetBytes(date));
            var secretService = ComputeHmacSha256Hash(secretDate, Encoding.UTF8.GetBytes("faceid"));
            var secretSigning = ComputeHmacSha256Hash(secretService, Encoding.UTF8.GetBytes("tc3_request"));
            var signatureBytes = ComputeHmacSha256Hash(secretSigning, Encoding.UTF8.GetBytes(stringToSign));

            return BitConverter.ToString(signatureBytes).Replace("-", "").ToLower();
        }

        private static string GetCredentialScope(string timestamp)
        {
            var date = DateTimeOffset.FromUnixTimeSeconds(long.Parse(timestamp)).ToString("yyyy-MM-dd");

            //var date = DateTimeOffset.FromUnixTimeSeconds(long.Parse(timestamp)).UtcDate.ToString("yyyy-MM-dd");
            return $"{date}/{Service}/tc3_request";
        }

        private static string ComputeSha256Hash(string input)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
            return BitConverter.ToString(bytes).Replace("-", "").ToLower();
        }

        private static byte[] ComputeHmacSha256Hash(byte[] key, byte[] data)
        {
            using var hmac = new HMACSHA256(key);
            return hmac.ComputeHash(data);
        }

        /// <summary>
        /// 解析API返回数据
        /// </summary>
        /// <param name="jsonResponse"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private static IdentityVerificationResultDTO ParseResponse(string jsonResponse)
        {
            using var doc = JsonDocument.Parse(jsonResponse);

            if (doc.RootElement.TryGetProperty("Response", out var responseElement))
            {
                if (responseElement.TryGetProperty("Error", out var errorElement))
                {
                    return new IdentityVerificationResultDTO
                    {
                        ResultCode = errorElement.GetProperty("Code").GetString(),
                        Description = errorElement.GetProperty("Message").GetString(),
                        RequestId = responseElement.GetProperty("RequestId").GetString()
                    };
                }

                return new IdentityVerificationResultDTO
                {
                    ResultCode = responseElement.TryGetProperty("ResultCode", out var resultCode) ? resultCode.GetString() : "Unknown",
                    Description = responseElement.TryGetProperty("Description", out var description) ? description.GetString() : "No description",
                    RequestId = responseElement.GetProperty("RequestId").GetString()
                };
            }

            throw new Exception("Unexpected JSON format: " + jsonResponse);
        }

    }
}