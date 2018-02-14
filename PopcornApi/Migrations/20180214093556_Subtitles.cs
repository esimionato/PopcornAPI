using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace PopcornApi.Migrations
{
    public partial class Subtitles : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Subtitle",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Bad = table.Column<double>(nullable: false),
                    ImdbId = table.Column<string>(nullable: true),
                    Iso639 = table.Column<string>(nullable: true),
                    LanguageId = table.Column<string>(nullable: true),
                    LanguageName = table.Column<string>(nullable: true),
                    MovieId = table.Column<int>(nullable: true),
                    OsdbSubtitleId = table.Column<string>(nullable: true),
                    Rating = table.Column<double>(nullable: false),
                    ShowId = table.Column<int>(nullable: true),
                    SubtitleDownloadLink = table.Column<string>(nullable: true),
                    SubtitleFileName = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subtitle", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Subtitle_MovieSet_MovieId",
                        column: x => x.MovieId,
                        principalTable: "MovieSet",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Subtitle_ShowSet_ShowId",
                        column: x => x.ShowId,
                        principalTable: "ShowSet",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Subtitle_MovieId",
                table: "Subtitle",
                column: "MovieId");

            migrationBuilder.CreateIndex(
                name: "IX_Subtitle_ShowId",
                table: "Subtitle",
                column: "ShowId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Subtitle");
        }
    }
}
