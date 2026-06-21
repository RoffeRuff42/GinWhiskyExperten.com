using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GinWhiskeyExperten.Migrations
{
    /// <inheritdoc />
    public partial class AddSpiritVotes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SpiritVotes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SpiritId = table.Column<int>(type: "int", nullable: false),
                    Stars = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpiritVotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SpiritVotes_Spirits_SpiritId",
                        column: x => x.SpiritId,
                        principalTable: "Spirits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SpiritVotes_SpiritId",
                table: "SpiritVotes",
                column: "SpiritId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SpiritVotes");
        }
    }
}
