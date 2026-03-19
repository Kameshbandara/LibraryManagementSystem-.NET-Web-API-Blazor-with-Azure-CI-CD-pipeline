using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace LibraryAPI.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Books",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Author = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ISBN = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CoverImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Publisher = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Language = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "English"),
                    Pages = table.Column<int>(type: "int", nullable: false),
                    PublishedYear = table.Column<int>(type: "int", nullable: false),
                    TotalCopies = table.Column<int>(type: "int", nullable: false),
                    AvailableCopies = table.Column<int>(type: "int", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Rating = table.Column<double>(type: "float", nullable: false),
                    IsFeatured = table.Column<bool>(type: "bit", nullable: false),
                    AddedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    LastUpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TimesBorrowed = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Books", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Members",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    MembershipNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    JoinedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    MembershipExpiry = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    MaxBooksAllowed = table.Column<int>(type: "int", nullable: false, defaultValue: 3),
                    TotalBooksBorrowed = table.Column<int>(type: "int", nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MembershipType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Members", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BorrowRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MemberId = table.Column<int>(type: "int", nullable: false),
                    BookId = table.Column<int>(type: "int", nullable: false),
                    BorrowDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReturnDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Borrowed"),
                    FineAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BorrowRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BorrowRecords_Books_BookId",
                        column: x => x.BookId,
                        principalTable: "Books",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BorrowRecords_Members_MemberId",
                        column: x => x.MemberId,
                        principalTable: "Members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Books",
                columns: new[] { "Id", "AddedDate", "Author", "AvailableCopies", "Category", "CoverImageUrl", "Description", "ISBN", "IsFeatured", "Language", "LastUpdatedDate", "Pages", "Price", "PublishedYear", "Publisher", "Rating", "TimesBorrowed", "Title", "TotalCopies" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 10, 26, 15, 40, 8, 370, DateTimeKind.Local).AddTicks(291), "Robert C. Martin", 5, "Programming", "https://example.com/cleancode.jpg", "A handbook of agile software craftsmanship", "978-0132350884", false, "English", null, 464, 45.99m, 2008, "Prentice Hall", 4.5, 0, "Clean Code", 5 },
                    { 2, new DateTime(2025, 10, 26, 15, 40, 8, 371, DateTimeKind.Local).AddTicks(3691), "Andrew Hunt", 3, "Programming", "https://example.com/cleancode.jpg", "Your journey to mastery", "978-0201616224", false, "English", null, 352, 49.99m, 1999, "Addison-Wesley", 4.7000000000000002, 0, "The Pragmatic Programmer", 3 }
                });

            migrationBuilder.InsertData(
                table: "Members",
                columns: new[] { "Id", "Address", "DateOfBirth", "Email", "FirstName", "IsActive", "JoinedDate", "LastName", "MaxBooksAllowed", "MembershipExpiry", "MembershipNumber", "MembershipType", "PhoneNumber", "TotalBooksBorrowed" },
                values: new object[,]
                {
                    { 1, "123 Main St, City", null, "john.doe@email.com", "John", true, new DateTime(2024, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Doe", 5, new DateTime(2025, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "MEM2024001", "Premium", "+1234567890", 0 },
                    { 2, "456 Oak Ave, City", null, "jane.smith@email.com", "Jane", true, new DateTime(2024, 7, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Smith", 3, new DateTime(2025, 7, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "MEM2024002", "Regular", "+1234567891", 0 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Books_ISBN",
                table: "Books",
                column: "ISBN",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BorrowRecords_BookId",
                table: "BorrowRecords",
                column: "BookId");

            migrationBuilder.CreateIndex(
                name: "IX_BorrowRecords_MemberId",
                table: "BorrowRecords",
                column: "MemberId");

            migrationBuilder.CreateIndex(
                name: "IX_Members_Email",
                table: "Members",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Members_MembershipNumber",
                table: "Members",
                column: "MembershipNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BorrowRecords");

            migrationBuilder.DropTable(
                name: "Books");

            migrationBuilder.DropTable(
                name: "Members");
        }
    }
}
