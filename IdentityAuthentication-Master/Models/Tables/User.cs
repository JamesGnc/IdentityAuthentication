namespace IdentityAuthentication_Master.Models.Tables
{
    public class User
    {
        public int Id { get; set; } // 用户id
        public string? AccountNumber { get; set; } // 用户账号
        public string? UserName { get; set; } // 用户昵称
        public string? Password { get; set; } // 用户密码
        public string? UserSex { get; set; } // 用户性别
        public string? Telephone { get; set; } // 用户手机号
        public DateTime CreatTime { get; set; } // 注册时间
        public DateTime? LoginTime { get; set; } // 登录时间
        public bool UserState { get; set; } // 用户状态
        public string? Summary { get; set; } // 个人简介
        public string? UserAddress { get; set; } // 用户地址
        public string? AvatarUrl { get; set; } // 用户头像
        public string? BackgroundUrl { get; set; } // 背景图片
        public string? Status { get; set; } // 用户身份
    }
}
