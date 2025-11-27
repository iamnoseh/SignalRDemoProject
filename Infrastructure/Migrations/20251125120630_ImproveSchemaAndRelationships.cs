using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ImproveSchemaAndRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ChatGroupMembers_GroupId",
                table: "ChatGroupMembers");

            migrationBuilder.AddColumn<DateTime>(
                name: "EditedAt",
                table: "ChatMessages",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "ChatMessages",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsEdited",
                table: "ChatMessages",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Role",
                table: "ChatGroupMembers",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_GroupName",
                table: "ChatMessages",
                column: "GroupName");

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_ReceiverUserId",
                table: "ChatMessages",
                column: "ReceiverUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_UserId_ReceiverUserId",
                table: "ChatMessages",
                columns: new[] { "UserId", "ReceiverUserId" });

            migrationBuilder.CreateIndex(
                name: "IX_ChatGroups_OwnerUserId",
                table: "ChatGroups",
                column: "OwnerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatGroupMembers_GroupId_UserId",
                table: "ChatGroupMembers",
                columns: new[] { "GroupId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChatGroupMembers_UserId",
                table: "ChatGroupMembers",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_ChatGroupMembers_AspNetUsers_UserId",
                table: "ChatGroupMembers",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ChatGroups_AspNetUsers_OwnerUserId",
                table: "ChatGroups",
                column: "OwnerUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ChatMessages_AspNetUsers_ReceiverUserId",
                table: "ChatMessages",
                column: "ReceiverUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ChatMessages_AspNetUsers_UserId",
                table: "ChatMessages",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChatGroupMembers_AspNetUsers_UserId",
                table: "ChatGroupMembers");

            migrationBuilder.DropForeignKey(
                name: "FK_ChatGroups_AspNetUsers_OwnerUserId",
                table: "ChatGroups");

            migrationBuilder.DropForeignKey(
                name: "FK_ChatMessages_AspNetUsers_ReceiverUserId",
                table: "ChatMessages");

            migrationBuilder.DropForeignKey(
                name: "FK_ChatMessages_AspNetUsers_UserId",
                table: "ChatMessages");

            migrationBuilder.DropIndex(
                name: "IX_ChatMessages_GroupName",
                table: "ChatMessages");

            migrationBuilder.DropIndex(
                name: "IX_ChatMessages_ReceiverUserId",
                table: "ChatMessages");

            migrationBuilder.DropIndex(
                name: "IX_ChatMessages_UserId_ReceiverUserId",
                table: "ChatMessages");

            migrationBuilder.DropIndex(
                name: "IX_ChatGroups_OwnerUserId",
                table: "ChatGroups");

            migrationBuilder.DropIndex(
                name: "IX_ChatGroupMembers_GroupId_UserId",
                table: "ChatGroupMembers");

            migrationBuilder.DropIndex(
                name: "IX_ChatGroupMembers_UserId",
                table: "ChatGroupMembers");

            migrationBuilder.DropColumn(
                name: "EditedAt",
                table: "ChatMessages");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "ChatMessages");

            migrationBuilder.DropColumn(
                name: "IsEdited",
                table: "ChatMessages");

            migrationBuilder.DropColumn(
                name: "Role",
                table: "ChatGroupMembers");

            migrationBuilder.CreateIndex(
                name: "IX_ChatGroupMembers_GroupId",
                table: "ChatGroupMembers",
                column: "GroupId");
        }
    }
}
