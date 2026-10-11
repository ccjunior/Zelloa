using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Zelloa.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class OrderCutoffOpeningTime : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<TimeOnly>(
                name: "OpeningTime",
                table: "OrderCutoffs",
                type: "time without time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OpeningTime",
                table: "OrderCutoffs");
        }
    }
}
