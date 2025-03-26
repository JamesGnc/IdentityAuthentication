using SqlSugar;

namespace IdentityAuthentication_Master.Models.Tables
{
    public class UserIdentityInfos
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public long Id { get; set; }

        public string Name { get; set; } = null!;

        public string IdCard { get; set; } = null!;

        public DateTime CreateTime { get; set; }

    }
}
