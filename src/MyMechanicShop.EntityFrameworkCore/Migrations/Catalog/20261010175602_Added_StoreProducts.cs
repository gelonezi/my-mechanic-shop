using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyMechanicShop.Migrations.Catalog
{
    /// <inheritdoc />
    public partial class Added_StoreProducts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Price",
                table: "CatalogProducts");

            migrationBuilder.RenameColumn(
                name: "StockCount",
                table: "CatalogProducts",
                newName: "Unit");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "CatalogProducts",
                type: "character varying(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<string>(
                name: "Brand",
                table: "CatalogProducts",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeleterId",
                table: "CatalogProducts",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletionTime",
                table: "CatalogProducts",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "CatalogProducts",
                type: "character varying(1024)",
                maxLength: 1024,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Ean",
                table: "CatalogProducts",
                type: "character varying(13)",
                maxLength: 13,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "CatalogProducts",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "CatalogStoreProducts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    PriceAmount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    PriceCurrency = table.Column<int>(type: "integer", nullable: false),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CatalogStoreProducts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CatalogStoreProducts_CatalogProducts_ProductId",
                        column: x => x.ProductId,
                        principalTable: "CatalogProducts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CatalogStoreProducts_ProductId",
                table: "CatalogStoreProducts",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_CatalogStoreProducts_TenantId_ProductId",
                table: "CatalogStoreProducts",
                columns: new[] { "TenantId", "ProductId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CatalogStoreProducts");

            migrationBuilder.DropColumn(
                name: "Brand",
                table: "CatalogProducts");

            migrationBuilder.DropColumn(
                name: "DeleterId",
                table: "CatalogProducts");

            migrationBuilder.DropColumn(
                name: "DeletionTime",
                table: "CatalogProducts");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "CatalogProducts");

            migrationBuilder.DropColumn(
                name: "Ean",
                table: "CatalogProducts");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "CatalogProducts");

            migrationBuilder.RenameColumn(
                name: "Unit",
                table: "CatalogProducts",
                newName: "StockCount");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "CatalogProducts",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(128)",
                oldMaxLength: 128);

            migrationBuilder.AddColumn<float>(
                name: "Price",
                table: "CatalogProducts",
                type: "real",
                nullable: false,
                defaultValue: 0f);
        }
    }
}
