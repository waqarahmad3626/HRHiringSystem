using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRHiringSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAIEvaluationFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_JobApplications_CandidateId",
                table: "JobApplications");

            migrationBuilder.AddColumn<string>(
                name: "Education",
                table: "Jobs",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ExperienceYears",
                table: "Jobs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "RequiredSkills",
                table: "Jobs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                comment: "JSON array of required skills");

            migrationBuilder.AddColumn<DateTime>(
                name: "EvaluatedAt",
                table: "JobApplications",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HRNotes",
                table: "JobApplications",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "InterviewScheduledAt",
                table: "JobApplications",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MongoReportId",
                table: "JobApplications",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Score",
                table: "JobApplications",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "JobApplications",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Pending");

            migrationBuilder.CreateIndex(
                name: "IX_JobApplications_CandidateId_JobId",
                table: "JobApplications",
                columns: new[] { "CandidateId", "JobId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_JobApplications_Status",
                table: "JobApplications",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_JobApplications_CandidateId_JobId",
                table: "JobApplications");

            migrationBuilder.DropIndex(
                name: "IX_JobApplications_Status",
                table: "JobApplications");

            migrationBuilder.DropColumn(
                name: "Education",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "ExperienceYears",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "RequiredSkills",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "EvaluatedAt",
                table: "JobApplications");

            migrationBuilder.DropColumn(
                name: "HRNotes",
                table: "JobApplications");

            migrationBuilder.DropColumn(
                name: "InterviewScheduledAt",
                table: "JobApplications");

            migrationBuilder.DropColumn(
                name: "MongoReportId",
                table: "JobApplications");

            migrationBuilder.DropColumn(
                name: "Score",
                table: "JobApplications");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "JobApplications");

            migrationBuilder.CreateIndex(
                name: "IX_JobApplications_CandidateId",
                table: "JobApplications",
                column: "CandidateId");
        }
    }
}
