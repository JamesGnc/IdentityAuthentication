using IdentityAuthentication_Master.Servies.IndentityService;
using SqlSugar;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient(); 
builder.Services.AddScoped<IIdentityUserInfoService, IdentityUserInfoService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IdentityUserInfoService>();

// 添加服务到容器  
builder.Services.AddSingleton<SqlSugarClient>(provider =>
{
    var configuration = provider.GetRequiredService<IConfiguration>();
    var connectionString = configuration.GetConnectionString("SqlConnection");
    return new SqlSugarClient(new ConnectionConfig()
    {
        ConnectionString = connectionString,
        DbType = DbType.MySql,
        IsAutoCloseConnection = true
    },
    db =>
    {
        db.Aop.OnLogExecuting = (sql, pars) =>
        {
            Console.WriteLine(sql);
        };
    });
});

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
