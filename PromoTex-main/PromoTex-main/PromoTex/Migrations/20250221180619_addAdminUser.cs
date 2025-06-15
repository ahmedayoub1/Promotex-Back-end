using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PromoTex.Migrations
{
    /// <inheritdoc />
    public partial class addAdminUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
     INSERT INTO [dbo].[AspNetUsers] 
         ([Id], [FullName], [PhoneNumber], [FullAddress],[UserName], [NormalizedUserName], [Email], [NormalizedEmail], [EmailConfirmed], [PasswordHash], [SecurityStamp], [ConcurrencyStamp], [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEnd], [LockoutEnabled], [AccessFailedCount]) 
         VALUES 
             (N'1fb443da-b011-43b9-88b9-dbadfe952db5', N'Sally Adel Kamel', N'01223650694', N'Asyut / Baheg', N'SallyAdel22', N'SALLYADEL22', N'SallyAdel22@gmail.com', N'SALLYADEL22@GMAIL.COM', 1, 
             N'AQAAAAIAAYagAAAAECMWrxpYjPfwFEqZG1/PbxjQumaRnEmNmgSIlMzWRZO4+ky8KyV9cLZ5TVvRSB/bUA==', 
             N'GXIEGS44DXUKNOYW5KVUXBBAZSJ375SM', 
             N'0cbcc911-9d01-48f0-a87b-cfbdaf4d0e4e', 0, 0, NULL, 1, 0)
     ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
         DELETE FROM [dbo].[AspNetUsers] WHERE [Id] = '1fb443da-b011-43b9-88b9-dbadfe952db5';
     ");
        }
    }
}
