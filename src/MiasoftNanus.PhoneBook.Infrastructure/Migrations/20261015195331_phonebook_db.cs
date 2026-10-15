using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiasoftNanus.PhoneBook.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class phonebook_db : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "locations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_locations", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "work_areas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_work_areas", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "workstations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    work_area_id = table.Column<Guid>(type: "uuid", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_workstations", x => x.id);
                    table.ForeignKey(
                        name: "fk_workstations_work_areas_work_area_id",
                        column: x => x.work_area_id,
                        principalTable: "work_areas",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "agendas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    full_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    email = table.Column<string>(type: "text", nullable: false),
                    enabled = table.Column<bool>(type: "boolean", nullable: false),
                    image_path = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    direct_line = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    @internal = table.Column<string>(name: "internal", type: "character varying(50)", maxLength: 50, nullable: true),
                    corporate = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    direct_access = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    workstation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    location_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id_created = table.Column<Guid>(type: "uuid", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_agendas", x => x.id);
                    table.ForeignKey(
                        name: "fk_agendas_location_location_id",
                        column: x => x.location_id,
                        principalTable: "locations",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_agendas_user_user_id_created",
                        column: x => x.user_id_created,
                        principalTable: "phonebook_users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_agendas_workstation_workstation_id",
                        column: x => x.workstation_id,
                        principalTable: "workstations",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "ix_agendas_location_id",
                table: "agendas",
                column: "location_id");

            migrationBuilder.CreateIndex(
                name: "ix_agendas_user_id_created",
                table: "agendas",
                column: "user_id_created");

            migrationBuilder.CreateIndex(
                name: "ix_agendas_workstation_id",
                table: "agendas",
                column: "workstation_id");

            migrationBuilder.CreateIndex(
                name: "ix_workstations_work_area_id",
                table: "workstations",
                column: "work_area_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "agendas");

            migrationBuilder.DropTable(
                name: "locations");

            migrationBuilder.DropTable(
                name: "workstations");

            migrationBuilder.DropTable(
                name: "work_areas");
        }
    }
}
