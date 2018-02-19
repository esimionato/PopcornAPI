using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace PopcornApi.Migrations
{
    public partial class LanguagesIds : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Iso639",
                table: "LanguageSet",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SubLanguageId",
                table: "LanguageSet",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Iso639",
                table: "LanguageSet");

            migrationBuilder.DropColumn(
                name: "SubLanguageId",
                table: "LanguageSet");
        }
    }
}
