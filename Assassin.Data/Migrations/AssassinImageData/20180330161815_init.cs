using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace Assassin.Data.Migrations.AssassinImageData
{
    public partial class init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AssassinImage",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CreatedDT = table.Column<DateTime>(nullable: false),
                    Image = table.Column<byte[]>(nullable: true),
                    ModifiedDT = table.Column<DateTime>(nullable: false),
                    RelatedTypeDescription = table.Column<string>(nullable: true),
                    RelationId = table.Column<Guid>(nullable: false),
                    Synced = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssassinImage", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AssassinImage_CreatedDT",
                table: "AssassinImage",
                column: "CreatedDT");

            migrationBuilder.CreateIndex(
                name: "IX_AssassinImage_Id",
                table: "AssassinImage",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AssassinImage_ModifiedDT",
                table: "AssassinImage",
                column: "ModifiedDT");

            migrationBuilder.CreateIndex(
                name: "IX_AssassinImage_RelatedTypeDescription",
                table: "AssassinImage",
                column: "RelatedTypeDescription");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AssassinImage");
        }
    }
}
