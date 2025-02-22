﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VAII.Data.Migrations
{
    /// <inheritdoc />
    public partial class ReviewPlusUserIDInGame : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UserID",
                table: "Games",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserID",
                table: "Games");
        }
    }
}
