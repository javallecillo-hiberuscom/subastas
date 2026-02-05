using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Subastas.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDniToUsuario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Empresa",
                columns: table => new
                {
                    IdEmpresa = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    Cif = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    Direccion = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: true),
                    Telefono = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Empresa__75D2CED44086D3C7", x => x.IdEmpresa);
                });

            migrationBuilder.CreateTable(
                name: "Vehiculo",
                columns: table => new
                {
                    IdVehiculo = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Marca = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    Modelo = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    Matricula = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    Color = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: true),
                    Anio = table.Column<int>(type: "int", nullable: true),
                    NumeroPuertas = table.Column<int>(type: "int", nullable: true),
                    Kilometraje = table.Column<int>(type: "int", nullable: true),
                    Potencia = table.Column<int>(type: "int", nullable: true),
                    TipoMotor = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    TipoCarroceria = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FechaMatriculacion = table.Column<DateOnly>(type: "date", nullable: true),
                    FechaCreacion = table.Column<DateOnly>(type: "date", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false, defaultValue: "registrado")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Vehiculo__4868297046AD87EF", x => x.IdVehiculo);
                });

            migrationBuilder.CreateTable(
                name: "Usuario",
                columns: table => new
                {
                    IdUsuario = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    Apellidos = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    Email = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    Password = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false),
                    Rol = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false, defaultValue: "registrado"),
                    Activo = table.Column<byte>(type: "tinyint", nullable: false, defaultValue: (byte)1),
                    Validado = table.Column<byte>(type: "tinyint", nullable: false),
                    IdEmpresa = table.Column<int>(type: "int", nullable: true),
                    Telefono = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    Direccion = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true),
                    FotoPerfilBase64 = table.Column<string>(type: "varchar(max)", nullable: true),
                    DocumentoIAE = table.Column<string>(type: "varchar(500)", unicode: false, maxLength: 500, nullable: true),
                    Dni = table.Column<string>(type: "varchar(9)", unicode: false, maxLength: 9, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Usuario__645723A6CA1F7D0A", x => x.IdUsuario);
                    table.ForeignKey(
                        name: "FK_Usuario_Empresa",
                        column: x => x.IdEmpresa,
                        principalTable: "Empresa",
                        principalColumn: "IdEmpresa",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ImagenVehiculo",
                columns: table => new
                {
                    IdImagen = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdVehiculo = table.Column<int>(type: "int", nullable: false),
                    Nombre = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    Ruta = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: true),
                    Activo = table.Column<byte>(type: "tinyint", nullable: false, defaultValue: (byte)1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__ImagenVe__EA9A7136BBBF0FF8", x => x.IdImagen);
                    table.ForeignKey(
                        name: "FK_ImagenVehiculo_Vehiculo",
                        column: x => x.IdVehiculo,
                        principalTable: "Vehiculo",
                        principalColumn: "IdVehiculo");
                });

            migrationBuilder.CreateTable(
                name: "Subasta",
                columns: table => new
                {
                    IdSubasta = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdVehiculo = table.Column<int>(type: "int", nullable: false),
                    FechaInicio = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false),
                    FechaFin = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false),
                    PrecioInicial = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    IncrementoMinimo = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    PrecioActual = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false, defaultValue: "activa")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Subasta__710C9B2556003E5C", x => x.IdSubasta);
                    table.ForeignKey(
                        name: "FK_Subasta_Vehiculo",
                        column: x => x.IdVehiculo,
                        principalTable: "Vehiculo",
                        principalColumn: "IdVehiculo");
                });

            migrationBuilder.CreateTable(
                name: "NotificacionAdmin",
                columns: table => new
                {
                    IdNotificacion = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Titulo = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: false),
                    Mensaje = table.Column<string>(type: "varchar(500)", unicode: false, maxLength: 500, nullable: false),
                    Tipo = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    IdUsuario = table.Column<int>(type: "int", nullable: true),
                    Leida = table.Column<byte>(type: "tinyint", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime", nullable: false),
                    DatosAdicionales = table.Column<string>(type: "varchar(500)", unicode: false, maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificacionAdmin", x => x.IdNotificacion);
                    table.ForeignKey(
                        name: "FK_NotificacionAdmin_Usuario_IdUsuario",
                        column: x => x.IdUsuario,
                        principalTable: "Usuario",
                        principalColumn: "IdUsuario",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Notificacion",
                columns: table => new
                {
                    IdNotificacion = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdUsuario = table.Column<int>(type: "int", nullable: false),
                    IdSubasta = table.Column<int>(type: "int", nullable: false),
                    Mensaje = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaEnvio = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false),
                    Leida = table.Column<byte>(type: "tinyint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Notifica__AFE1D7E48069B189", x => x.IdNotificacion);
                    table.ForeignKey(
                        name: "FK_Notificacion_Subasta",
                        column: x => x.IdSubasta,
                        principalTable: "Subasta",
                        principalColumn: "IdSubasta");
                    table.ForeignKey(
                        name: "FK_Notificacion_Usuario",
                        column: x => x.IdUsuario,
                        principalTable: "Usuario",
                        principalColumn: "IdUsuario");
                });

            migrationBuilder.CreateTable(
                name: "Puja",
                columns: table => new
                {
                    IdPuja = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdSubasta = table.Column<int>(type: "int", nullable: false),
                    IdUsuario = table.Column<int>(type: "int", nullable: false),
                    Cantidad = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    FechaPuja = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Puja__B4598EC6C86DB51A", x => x.IdPuja);
                    table.ForeignKey(
                        name: "FK_Puja_Subasta",
                        column: x => x.IdSubasta,
                        principalTable: "Subasta",
                        principalColumn: "IdSubasta");
                    table.ForeignKey(
                        name: "FK_Puja_Usuario",
                        column: x => x.IdUsuario,
                        principalTable: "Usuario",
                        principalColumn: "IdUsuario");
                });

            migrationBuilder.CreateTable(
                name: "Pago",
                columns: table => new
                {
                    IdPago = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdPuja = table.Column<int>(type: "int", nullable: false),
                    FechaPago = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false),
                    Cantidad = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    MetodoPago = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Pago__BD2295AD68E6A9EE", x => x.IdPago);
                    table.ForeignKey(
                        name: "FK_Pago_Puja",
                        column: x => x.IdPuja,
                        principalTable: "Puja",
                        principalColumn: "IdPuja");
                });

            migrationBuilder.CreateIndex(
                name: "UQ__Empresa__D837D05C040BA45A",
                table: "Empresa",
                column: "Cif",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ImagenVehiculo_IdVehiculo",
                table: "ImagenVehiculo",
                column: "IdVehiculo");

            migrationBuilder.CreateIndex(
                name: "IX_Notificacion_IdSubasta",
                table: "Notificacion",
                column: "IdSubasta");

            migrationBuilder.CreateIndex(
                name: "IX_Notificacion_IdUsuario",
                table: "Notificacion",
                column: "IdUsuario");

            migrationBuilder.CreateIndex(
                name: "IX_NotificacionAdmin_IdUsuario",
                table: "NotificacionAdmin",
                column: "IdUsuario");

            migrationBuilder.CreateIndex(
                name: "IX_Pago_IdPuja",
                table: "Pago",
                column: "IdPuja");

            migrationBuilder.CreateIndex(
                name: "IX_Puja_IdSubasta",
                table: "Puja",
                column: "IdSubasta");

            migrationBuilder.CreateIndex(
                name: "IX_Puja_IdUsuario",
                table: "Puja",
                column: "IdUsuario");

            migrationBuilder.CreateIndex(
                name: "IX_Subasta_IdVehiculo",
                table: "Subasta",
                column: "IdVehiculo");

            migrationBuilder.CreateIndex(
                name: "IX_Usuario_IdEmpresa",
                table: "Usuario",
                column: "IdEmpresa");

            migrationBuilder.CreateIndex(
                name: "UQ__Usuario__AB6E616411E2D183",
                table: "Usuario",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ__Vehiculo__30962D1527B6AAEF",
                table: "Vehiculo",
                column: "Matricula",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ImagenVehiculo");

            migrationBuilder.DropTable(
                name: "Notificacion");

            migrationBuilder.DropTable(
                name: "NotificacionAdmin");

            migrationBuilder.DropTable(
                name: "Pago");

            migrationBuilder.DropTable(
                name: "Puja");

            migrationBuilder.DropTable(
                name: "Subasta");

            migrationBuilder.DropTable(
                name: "Usuario");

            migrationBuilder.DropTable(
                name: "Vehiculo");

            migrationBuilder.DropTable(
                name: "Empresa");
        }
    }
}
