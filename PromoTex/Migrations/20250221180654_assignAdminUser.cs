using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PromoTex.Migrations
{
    /// <inheritdoc />
    public partial class assignAdminUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Assign Admin Role to the User (Ensure the role exists)
            migrationBuilder.Sql(@"
                INSERT INTO [dbo].[AspNetUserRoles] ([UserId], [RoleId]) 
                SELECT 
                    (SELECT [Id] FROM [dbo].[AspNetUsers] WHERE [Email] = 'SallyAdel22@gmail.com'),
                    (SELECT [Id] FROM [dbo].[AspNetRoles] WHERE [Name] = 'Admin')
                WHERE NOT EXISTS (
                    SELECT 1 FROM [dbo].[AspNetUserRoles] 
                    WHERE [UserId] = (SELECT [Id] FROM [dbo].[AspNetUsers] WHERE [Email] = 'SallyAdel22@gmail.com')
                    AND [RoleId] = (SELECT [Id] FROM [dbo].[AspNetRoles] WHERE [Name] = 'Admin')
                );
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove Admin Role Assignment
            migrationBuilder.Sql(@"
                DELETE FROM [dbo].[AspNetUserRoles] 
                WHERE [UserId] = (SELECT [Id] FROM [dbo].[AspNetUsers] WHERE [Email] = 'SallyAdel22@gmail.com')
                AND [RoleId] = (SELECT [Id] FROM [dbo].[AspNetRoles] WHERE [Name] = 'Admin');
            ");
        }
    }
}

