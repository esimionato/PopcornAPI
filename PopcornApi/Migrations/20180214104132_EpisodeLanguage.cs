using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace PopcornApi.Migrations
{
    public partial class EpisodeLanguage : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Subtitle_ShowSet_ShowId",
                table: "Subtitle");

            migrationBuilder.RenameColumn(
                name: "ShowId",
                table: "Subtitle",
                newName: "EpisodeShowId");

            migrationBuilder.RenameIndex(
                name: "IX_Subtitle_ShowId",
                table: "Subtitle",
                newName: "IX_Subtitle_EpisodeShowId");

            migrationBuilder.AddForeignKey(
                name: "FK_Subtitle_EpisodeShowSet_EpisodeShowId",
                table: "Subtitle",
                column: "EpisodeShowId",
                principalTable: "EpisodeShowSet",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Subtitle_EpisodeShowSet_EpisodeShowId",
                table: "Subtitle");

            migrationBuilder.RenameColumn(
                name: "EpisodeShowId",
                table: "Subtitle",
                newName: "ShowId");

            migrationBuilder.RenameIndex(
                name: "IX_Subtitle_EpisodeShowId",
                table: "Subtitle",
                newName: "IX_Subtitle_ShowId");

            migrationBuilder.AddForeignKey(
                name: "FK_Subtitle_ShowSet_ShowId",
                table: "Subtitle",
                column: "ShowId",
                principalTable: "ShowSet",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
