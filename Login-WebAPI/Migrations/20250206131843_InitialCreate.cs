using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Login_WebAPI.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Employees",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Department = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ManagerId = table.Column<int>(type: "int", nullable: true),
                    JoiningDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Employees", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TokenRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    RefreshToken = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EmployeeId = table.Column<int>(type: "int", nullable: true),
                    RefreshToken = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id");
                });

            migrationBuilder.InsertData(
                table: "Employees",
                columns: new[] { "Id", "Department", "Email", "JoiningDate", "ManagerId", "Name" },
                values: new object[] { 1, "HR", "alice.johnson@example.com", new DateTime(2023, 2, 6, 18, 48, 43, 269, DateTimeKind.Local).AddTicks(6426), null, "Alice Johnson" });

            migrationBuilder.InsertData(
                table: "Employees",
                columns: new[] { "Id", "Department", "Email", "JoiningDate", "ManagerId", "Name" },
                values: new object[] { 2, "IT", "bob.smith@example.com", new DateTime(2024, 2, 6, 18, 48, 43, 269, DateTimeKind.Local).AddTicks(6438), 1, "Bob Smith" });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Email", "EmployeeId", "Name", "PasswordHash", "RefreshToken", "Role" },
                values: new object[] { 1, "alice.johnson@example.com", 1, "Alice Johnson", "$2a$11$N10J9vUUC28dA/7f6LyObOlK1gkVUmn2LqtkA9E4IOzc9CXCKCt.K", "sample-refresh-token-1", "Admin" });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Email", "EmployeeId", "Name", "PasswordHash", "RefreshToken", "Role" },
                values: new object[] { 2, "bob.smith@example.com", 2, "Bob Smith", "$2a$11$5MP6WDXWZ95KDm9xdickYe//DiDiTpxiogljzHPX/gh3gRiayKwLG", "sample-refresh-token-2", "Employee" });

            migrationBuilder.CreateIndex(
                name: "IX_Users_EmployeeId",
                table: "Users",
                column: "EmployeeId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TokenRequests");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Employees");
        }
    }
}
