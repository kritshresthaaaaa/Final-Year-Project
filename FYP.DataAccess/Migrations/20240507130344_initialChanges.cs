﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fyp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class initialChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductRecommendations_Product_ProductId",
                table: "ProductRecommendations");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductRecommendations_Product_RecommendedProductId",
                table: "ProductRecommendations");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductRecommendations_Product_ProductId",
                table: "ProductRecommendations",
                column: "ProductId",
                principalTable: "Product",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductRecommendations_Product_RecommendedProductId",
                table: "ProductRecommendations",
                column: "RecommendedProductId",
                principalTable: "Product",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductRecommendations_Product_ProductId",
                table: "ProductRecommendations");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductRecommendations_Product_RecommendedProductId",
                table: "ProductRecommendations");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductRecommendations_Product_ProductId",
                table: "ProductRecommendations",
                column: "ProductId",
                principalTable: "Product",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductRecommendations_Product_RecommendedProductId",
                table: "ProductRecommendations",
                column: "RecommendedProductId",
                principalTable: "Product",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
