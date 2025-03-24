using IdentityAuthentication_Master.Models.DTO;

namespace IdentityAuthentication_Master.Servies.IndentityService
{
    public interface IIdentityUserInfoService
    {
        String IndentityUserInfo(IndentityUserInfoParamDTO param);
    }
    public class IdentityUserInfoService:IIdentityUserInfoService
    {
        private readonly IConfiguration _configuration;

        public IdentityUserInfoService(IConfiguration configuration)
        {
            _configuration = configuration;
            string _appSecret = _configuration["AppSecret"]!;
            string _appKey = _configuration["AppKey"]!;
        }

        public string IndentityUserInfo(IndentityUserInfoParamDTO param)
        {
            var results = "This is result";
            return results;
        }
    }
}
