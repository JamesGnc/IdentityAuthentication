using IdentityAuthentication_Master.Models.Tables;
using IdentityAuthentication_Master.Servies.IndentityService;
using SqlSugar;
using System.Security.Cryptography;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient();
builder.Services.AddScoped<IIdentityUserInfoService, IdentityUserInfoServiceImpl>();
builder.Services.AddScoped<IIdentityUserInfoMockService, IdentityUserInfoMockServiceImpl>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IdentityUserInfoServiceImpl>();
builder.Services.AddScoped<IdentityUserInfoMockServiceImpl>();

// 添加服务到容器  
builder.Services.AddSingleton<SqlSugarClient>(provider =>
{
    var configuration = provider.GetRequiredService<IConfiguration>();
    var connectionString = configuration.GetConnectionString("MySql");
    var sqlSugarClient = new SqlSugarClient(new ConnectionConfig()
    {
        ConnectionString = connectionString,
        DbType = DbType.MySql, // 或者 DbType.SqlServer，根据您的数据库类型
        IsAutoCloseConnection = true
    },
    db =>
    {
        db.Aop.OnLogExecuting = (sql, pars) =>
        {
            Console.WriteLine(sql);
        };
    });

    // 自动创建表
    sqlSugarClient.CodeFirst.SetStringDefaultLength(200)
                            .InitTables(typeof(UserIdentityInfos)); // 这里可以添加您的所有实体类

    return sqlSugarClient;
});

// 生成Kye Iv
//using (Aes aes = Aes.Create())
//{
//    aes.KeySize = 256; // 256-bit 密钥
//    aes.BlockSize = 128; // 128-bit 块大小
//    aes.GenerateKey();
//    aes.GenerateIV();

//    string keyBase64 = Convert.ToBase64String(aes.Key);
//    string ivBase64 = Convert.ToBase64String(aes.IV);

//    Console.WriteLine("🔑 AES Key: " + keyBase64);
//    Console.WriteLine("🛠️ AES IV: " + ivBase64);
//}

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
