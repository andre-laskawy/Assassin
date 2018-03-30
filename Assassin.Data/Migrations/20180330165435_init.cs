using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace Assassin.Data.Migrations
{
    public partial class init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BaseModel",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Archived = table.Column<bool>(nullable: false),
                    CreatedDT = table.Column<DateTime>(nullable: false),
                    JsonAttributes = table.Column<string>(nullable: true),
                    ModifiedDT = table.Column<DateTime>(nullable: false),
                    Synced = table.Column<bool>(nullable: false),
                    TypeDiscriptor = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BaseModel", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Relation",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    EntityId = table.Column<Guid>(nullable: false),
                    RelatedEntityId = table.Column<Guid>(nullable: true),
                    RelationName = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Relation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Relation_BaseModel_EntityId",
                        column: x => x.EntityId,
                        principalTable: "BaseModel",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Relation_BaseModel_RelatedEntityId",
                        column: x => x.RelatedEntityId,
                        principalTable: "BaseModel",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BaseModel_CreatedDT",
                table: "BaseModel",
                column: "CreatedDT");

            migrationBuilder.CreateIndex(
                name: "IX_BaseModel_Id",
                table: "BaseModel",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BaseModel_ModifiedDT",
                table: "BaseModel",
                column: "ModifiedDT");

            migrationBuilder.CreateIndex(
                name: "IX_BaseModel_TypeDiscriptor",
                table: "BaseModel",
                column: "TypeDiscriptor");

            migrationBuilder.CreateIndex(
                name: "IX_Relation_EntityId",
                table: "Relation",
                column: "EntityId");

            migrationBuilder.CreateIndex(
                name: "IX_Relation_RelatedEntityId",
                table: "Relation",
                column: "RelatedEntityId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Relation");

            migrationBuilder.DropTable(
                name: "BaseModel");
        }
    }
}
