using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FlightDay.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class ChangeToDateOnly : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FlightDays_Date_InstructorId",
                table: "FlightDays");

            migrationBuilder.AlterColumn<DateOnly>(
                name: "Date",
                table: "FlightDays",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.CreateIndex(
                name: "IX_FlightDays_Date_InstructorId",
                table: "FlightDays",
                columns: new[] { "Date", "InstructorId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FlightDays_Date_InstructorId",
                table: "FlightDays");

            migrationBuilder.AlterColumn<DateTime>(
                name: "Date",
                table: "FlightDays",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateOnly),
                oldType: "date");

            migrationBuilder.CreateIndex(
                name: "IX_FlightDays_Date_InstructorId",
                table: "FlightDays",
                columns: new[] { "Date", "InstructorId" });
        }
    }
}
