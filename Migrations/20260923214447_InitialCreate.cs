using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KROTraining.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EJERCICIO",
                columns: table => new
                {
                    ejercicio_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nombre = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    descripcion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    grupo_muscular = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EJERCICIO", x => x.ejercicio_id);
                });

            migrationBuilder.CreateTable(
                name: "PERMISO",
                columns: table => new
                {
                    permiso_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    descripcion = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PERMISO", x => x.permiso_id);
                });

            migrationBuilder.CreateTable(
                name: "PLAN_MEMBRESIA",
                columns: table => new
                {
                    plan_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    duracion_meses = table.Column<int>(type: "int", nullable: false),
                    precio = table.Column<decimal>(type: "decimal(10,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PLAN_MEMBRESIA", x => x.plan_id);
                });

            migrationBuilder.CreateTable(
                name: "ROL",
                columns: table => new
                {
                    rol_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nombre = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ROL", x => x.rol_id);
                });

            migrationBuilder.CreateTable(
                name: "MULTIMEDIA_EJERCICIO",
                columns: table => new
                {
                    multimedia_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ejercicio_id = table.Column<int>(type: "int", nullable: false),
                    tipo = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MULTIMEDIA_EJERCICIO", x => x.multimedia_id);
                    table.ForeignKey(
                        name: "FK_MULTIMEDIA_EJERCICIO",
                        column: x => x.ejercicio_id,
                        principalTable: "EJERCICIO",
                        principalColumn: "ejercicio_id");
                });

            migrationBuilder.CreateTable(
                name: "ROL_PERMISO",
                columns: table => new
                {
                    rol_id = table.Column<int>(type: "int", nullable: false),
                    permiso_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ROL_PERMISO", x => new { x.rol_id, x.permiso_id });
                    table.ForeignKey(
                        name: "FK_ROL_PERMISO_PERMISO",
                        column: x => x.permiso_id,
                        principalTable: "PERMISO",
                        principalColumn: "permiso_id");
                    table.ForeignKey(
                        name: "FK_ROL_PERMISO_ROL",
                        column: x => x.rol_id,
                        principalTable: "ROL",
                        principalColumn: "rol_id");
                });

            migrationBuilder.CreateTable(
                name: "USUARIO",
                columns: table => new
                {
                    usuario_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nombre = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    correo = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    contrasena_hash = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    telefono = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    foto_perfil = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "ACTIVO"),
                    fecha_registro = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysdatetime())"),
                    rol_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_USUARIO", x => x.usuario_id);
                    table.ForeignKey(
                        name: "FK_USUARIO_ROL",
                        column: x => x.rol_id,
                        principalTable: "ROL",
                        principalColumn: "rol_id");
                });

            migrationBuilder.CreateTable(
                name: "ADMINISTRADOR",
                columns: table => new
                {
                    usuario_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ADMINISTRADOR", x => x.usuario_id);
                    table.ForeignKey(
                        name: "FK_ADMINISTRADOR_USUARIO",
                        column: x => x.usuario_id,
                        principalTable: "USUARIO",
                        principalColumn: "usuario_id");
                });

            migrationBuilder.CreateTable(
                name: "AUDITORIA",
                columns: table => new
                {
                    auditoria_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    usuario_id = table.Column<int>(type: "int", nullable: false),
                    accion = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    fecha = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysdatetime())"),
                    detalle = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AUDITORIA", x => x.auditoria_id);
                    table.ForeignKey(
                        name: "FK_AUDITORIA_USUARIO",
                        column: x => x.usuario_id,
                        principalTable: "USUARIO",
                        principalColumn: "usuario_id");
                });

            migrationBuilder.CreateTable(
                name: "ENTRENADOR",
                columns: table => new
                {
                    usuario_id = table.Column<int>(type: "int", nullable: false),
                    especialidad = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ENTRENADOR", x => x.usuario_id);
                    table.ForeignKey(
                        name: "FK_ENTRENADOR_USUARIO",
                        column: x => x.usuario_id,
                        principalTable: "USUARIO",
                        principalColumn: "usuario_id");
                });

            migrationBuilder.CreateTable(
                name: "HORARIO_TAREA",
                columns: table => new
                {
                    horario_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    usuario_id = table.Column<int>(type: "int", nullable: false),
                    fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    tarea = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    estado = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false, defaultValue: "PENDIENTE")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HORARIO_TAREA", x => x.horario_id);
                    table.ForeignKey(
                        name: "FK_HORARIO_TAREA_USUARIO",
                        column: x => x.usuario_id,
                        principalTable: "USUARIO",
                        principalColumn: "usuario_id");
                });

            migrationBuilder.CreateTable(
                name: "NOTIFICACION",
                columns: table => new
                {
                    notificacion_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    usuario_id = table.Column<int>(type: "int", nullable: false),
                    tipo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    mensaje = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    fecha = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysdatetime())"),
                    estado = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false, defaultValue: "NO_LEIDA")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NOTIFICACION", x => x.notificacion_id);
                    table.ForeignKey(
                        name: "FK_NOTIFICACION_USUARIO",
                        column: x => x.usuario_id,
                        principalTable: "USUARIO",
                        principalColumn: "usuario_id");
                });

            migrationBuilder.CreateTable(
                name: "REPORTE",
                columns: table => new
                {
                    reporte_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    tipo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    periodo_inicio = table.Column<DateOnly>(type: "date", nullable: true),
                    periodo_fin = table.Column<DateOnly>(type: "date", nullable: true),
                    generado_por = table.Column<int>(type: "int", nullable: false),
                    fecha_generacion = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysdatetime())"),
                    formato_exportado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_REPORTE", x => x.reporte_id);
                    table.ForeignKey(
                        name: "FK_REPORTE_USUARIO",
                        column: x => x.generado_por,
                        principalTable: "USUARIO",
                        principalColumn: "usuario_id");
                });

            migrationBuilder.CreateTable(
                name: "EVENTO",
                columns: table => new
                {
                    evento_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nombre = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    descripcion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    cupo_maximo = table.Column<int>(type: "int", nullable: false),
                    estado = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false, defaultValue: "ACTIVO"),
                    administrador_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EVENTO", x => x.evento_id);
                    table.ForeignKey(
                        name: "FK_EVENTO_ADMINISTRADOR",
                        column: x => x.administrador_id,
                        principalTable: "ADMINISTRADOR",
                        principalColumn: "usuario_id");
                });

            migrationBuilder.CreateTable(
                name: "PARAMETRO_RECOMENDACION",
                columns: table => new
                {
                    parametro_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    valor = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    administrador_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PARAMETRO_RECOMENDACION", x => x.parametro_id);
                    table.ForeignKey(
                        name: "FK_PARAMETRO_ADMINISTRADOR",
                        column: x => x.administrador_id,
                        principalTable: "ADMINISTRADOR",
                        principalColumn: "usuario_id");
                });

            migrationBuilder.CreateTable(
                name: "CLIENTE",
                columns: table => new
                {
                    usuario_id = table.Column<int>(type: "int", nullable: false),
                    badge_numero = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    objetivo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    nivel = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    entrenador_id = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CLIENTE", x => x.usuario_id);
                    table.ForeignKey(
                        name: "FK_CLIENTE_ENTRENADOR",
                        column: x => x.entrenador_id,
                        principalTable: "ENTRENADOR",
                        principalColumn: "usuario_id");
                    table.ForeignKey(
                        name: "FK_CLIENTE_USUARIO",
                        column: x => x.usuario_id,
                        principalTable: "USUARIO",
                        principalColumn: "usuario_id");
                });

            migrationBuilder.CreateTable(
                name: "RUTINA",
                columns: table => new
                {
                    rutina_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nombre = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    descripcion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    objetivo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    nivel = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    entrenador_id = table.Column<int>(type: "int", nullable: false),
                    fecha_creacion = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysdatetime())"),
                    estado = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false, defaultValue: "ACTIVA")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RUTINA", x => x.rutina_id);
                    table.ForeignKey(
                        name: "FK_RUTINA_ENTRENADOR",
                        column: x => x.entrenador_id,
                        principalTable: "ENTRENADOR",
                        principalColumn: "usuario_id");
                });

            migrationBuilder.CreateTable(
                name: "ASISTENCIA",
                columns: table => new
                {
                    asistencia_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    cliente_id = table.Column<int>(type: "int", nullable: false),
                    fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    hora = table.Column<TimeOnly>(type: "time", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ASISTENCIA", x => x.asistencia_id);
                    table.ForeignKey(
                        name: "FK_ASISTENCIA_CLIENTE",
                        column: x => x.cliente_id,
                        principalTable: "CLIENTE",
                        principalColumn: "usuario_id");
                });

            migrationBuilder.CreateTable(
                name: "EVALUACION_FISICA",
                columns: table => new
                {
                    evaluacion_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    cliente_id = table.Column<int>(type: "int", nullable: false),
                    entrenador_id = table.Column<int>(type: "int", nullable: false),
                    fecha = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysdatetime())"),
                    peso = table.Column<decimal>(type: "decimal(6,2)", nullable: true),
                    estatura = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    medidas = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    observaciones = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    tipo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EVALUACION_FISICA", x => x.evaluacion_id);
                    table.ForeignKey(
                        name: "FK_EVALUACION_CLIENTE",
                        column: x => x.cliente_id,
                        principalTable: "CLIENTE",
                        principalColumn: "usuario_id");
                    table.ForeignKey(
                        name: "FK_EVALUACION_ENTRENADOR",
                        column: x => x.entrenador_id,
                        principalTable: "ENTRENADOR",
                        principalColumn: "usuario_id");
                });

            migrationBuilder.CreateTable(
                name: "INSCRIPCION_EVENTO",
                columns: table => new
                {
                    evento_id = table.Column<int>(type: "int", nullable: false),
                    cliente_id = table.Column<int>(type: "int", nullable: false),
                    fecha_inscripcion = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysdatetime())"),
                    estado = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false, defaultValue: "INSCRITO")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_INSCRIPCION_EVENTO", x => new { x.evento_id, x.cliente_id });
                    table.ForeignKey(
                        name: "FK_INSCRIPCION_CLIENTE",
                        column: x => x.cliente_id,
                        principalTable: "CLIENTE",
                        principalColumn: "usuario_id");
                    table.ForeignKey(
                        name: "FK_INSCRIPCION_EVENTO",
                        column: x => x.evento_id,
                        principalTable: "EVENTO",
                        principalColumn: "evento_id");
                });

            migrationBuilder.CreateTable(
                name: "MEMBRESIA",
                columns: table => new
                {
                    membresia_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    cliente_id = table.Column<int>(type: "int", nullable: false),
                    plan_id = table.Column<int>(type: "int", nullable: false),
                    fecha_inicio = table.Column<DateOnly>(type: "date", nullable: false),
                    fecha_fin = table.Column<DateOnly>(type: "date", nullable: false),
                    estado = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false, defaultValue: "ACTIVA")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MEMBRESIA", x => x.membresia_id);
                    table.ForeignKey(
                        name: "FK_MEMBRESIA_CLIENTE",
                        column: x => x.cliente_id,
                        principalTable: "CLIENTE",
                        principalColumn: "usuario_id");
                    table.ForeignKey(
                        name: "FK_MEMBRESIA_PLAN",
                        column: x => x.plan_id,
                        principalTable: "PLAN_MEMBRESIA",
                        principalColumn: "plan_id");
                });

            migrationBuilder.CreateTable(
                name: "META",
                columns: table => new
                {
                    meta_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    cliente_id = table.Column<int>(type: "int", nullable: false),
                    tipo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    valor_objetivo = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    fecha_inicio = table.Column<DateOnly>(type: "date", nullable: false),
                    fecha_limite = table.Column<DateOnly>(type: "date", nullable: true),
                    estado = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false, defaultValue: "ACTIVA")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_META", x => x.meta_id);
                    table.ForeignKey(
                        name: "FK_META_CLIENTE",
                        column: x => x.cliente_id,
                        principalTable: "CLIENTE",
                        principalColumn: "usuario_id");
                });

            migrationBuilder.CreateTable(
                name: "OBSERVACION_ENTRENADOR",
                columns: table => new
                {
                    observacion_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    cliente_id = table.Column<int>(type: "int", nullable: false),
                    entrenador_id = table.Column<int>(type: "int", nullable: false),
                    fecha = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysdatetime())"),
                    contenido = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OBSERVACION_ENTRENADOR", x => x.observacion_id);
                    table.ForeignKey(
                        name: "FK_OBSERVACION_CLIENTE",
                        column: x => x.cliente_id,
                        principalTable: "CLIENTE",
                        principalColumn: "usuario_id");
                    table.ForeignKey(
                        name: "FK_OBSERVACION_ENTRENADOR",
                        column: x => x.entrenador_id,
                        principalTable: "ENTRENADOR",
                        principalColumn: "usuario_id");
                });

            migrationBuilder.CreateTable(
                name: "PREDICCION_DESERCION",
                columns: table => new
                {
                    prediccion_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    cliente_id = table.Column<int>(type: "int", nullable: false),
                    nivel_riesgo = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    confiabilidad = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    fecha = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PREDICCION_DESERCION", x => x.prediccion_id);
                    table.ForeignKey(
                        name: "FK_PREDICCION_CLIENTE",
                        column: x => x.cliente_id,
                        principalTable: "CLIENTE",
                        principalColumn: "usuario_id");
                });

            migrationBuilder.CreateTable(
                name: "ASIGNACION_RUTINA",
                columns: table => new
                {
                    asignacion_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    cliente_id = table.Column<int>(type: "int", nullable: false),
                    rutina_id = table.Column<int>(type: "int", nullable: false),
                    entrenador_id = table.Column<int>(type: "int", nullable: false),
                    fecha_asignacion = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysdatetime())"),
                    estado = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false, defaultValue: "ACTIVA")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ASIGNACION_RUTINA", x => x.asignacion_id);
                    table.ForeignKey(
                        name: "FK_ASIGNACION_CLIENTE",
                        column: x => x.cliente_id,
                        principalTable: "CLIENTE",
                        principalColumn: "usuario_id");
                    table.ForeignKey(
                        name: "FK_ASIGNACION_ENTRENADOR",
                        column: x => x.entrenador_id,
                        principalTable: "ENTRENADOR",
                        principalColumn: "usuario_id");
                    table.ForeignKey(
                        name: "FK_ASIGNACION_RUTINA",
                        column: x => x.rutina_id,
                        principalTable: "RUTINA",
                        principalColumn: "rutina_id");
                });

            migrationBuilder.CreateTable(
                name: "RECOMENDACION",
                columns: table => new
                {
                    recomendacion_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    cliente_id = table.Column<int>(type: "int", nullable: false),
                    rutina_sugerida_id = table.Column<int>(type: "int", nullable: false),
                    entrenador_id = table.Column<int>(type: "int", nullable: true),
                    fecha = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysdatetime())"),
                    estado = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false, defaultValue: "PENDIENTE")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RECOMENDACION", x => x.recomendacion_id);
                    table.ForeignKey(
                        name: "FK_RECOMENDACION_CLIENTE",
                        column: x => x.cliente_id,
                        principalTable: "CLIENTE",
                        principalColumn: "usuario_id");
                    table.ForeignKey(
                        name: "FK_RECOMENDACION_ENTRENADOR",
                        column: x => x.entrenador_id,
                        principalTable: "ENTRENADOR",
                        principalColumn: "usuario_id");
                    table.ForeignKey(
                        name: "FK_RECOMENDACION_RUTINA",
                        column: x => x.rutina_sugerida_id,
                        principalTable: "RUTINA",
                        principalColumn: "rutina_id");
                });

            migrationBuilder.CreateTable(
                name: "RUTINA_EJERCICIO",
                columns: table => new
                {
                    rutina_id = table.Column<int>(type: "int", nullable: false),
                    ejercicio_id = table.Column<int>(type: "int", nullable: false),
                    orden = table.Column<int>(type: "int", nullable: false),
                    series = table.Column<int>(type: "int", nullable: false),
                    repeticiones = table.Column<int>(type: "int", nullable: false),
                    descanso_segundos = table.Column<int>(type: "int", nullable: false, defaultValue: 60)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RUTINA_EJERCICIO", x => new { x.rutina_id, x.ejercicio_id });
                    table.ForeignKey(
                        name: "FK_RUTINA_EJERCICIO_EJERCICIO",
                        column: x => x.ejercicio_id,
                        principalTable: "EJERCICIO",
                        principalColumn: "ejercicio_id");
                    table.ForeignKey(
                        name: "FK_RUTINA_EJERCICIO_RUTINA",
                        column: x => x.rutina_id,
                        principalTable: "RUTINA",
                        principalColumn: "rutina_id");
                });

            migrationBuilder.CreateTable(
                name: "PAGO",
                columns: table => new
                {
                    pago_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    membresia_id = table.Column<int>(type: "int", nullable: false),
                    monto = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    fecha_pago = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysdatetime())"),
                    metodo_pago = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    registrado_por = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PAGO", x => x.pago_id);
                    table.ForeignKey(
                        name: "FK_PAGO_MEMBRESIA",
                        column: x => x.membresia_id,
                        principalTable: "MEMBRESIA",
                        principalColumn: "membresia_id");
                    table.ForeignKey(
                        name: "FK_PAGO_USUARIO",
                        column: x => x.registrado_por,
                        principalTable: "USUARIO",
                        principalColumn: "usuario_id");
                });

            migrationBuilder.CreateTable(
                name: "VALIDACION_PREDICCION",
                columns: table => new
                {
                    validacion_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    prediccion_id = table.Column<int>(type: "int", nullable: false),
                    administrador_id = table.Column<int>(type: "int", nullable: false),
                    resultado = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    fecha = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VALIDACION_PREDICCION", x => x.validacion_id);
                    table.ForeignKey(
                        name: "FK_VALIDACION_ADMINISTRADOR",
                        column: x => x.administrador_id,
                        principalTable: "ADMINISTRADOR",
                        principalColumn: "usuario_id");
                    table.ForeignKey(
                        name: "FK_VALIDACION_PREDICCION",
                        column: x => x.prediccion_id,
                        principalTable: "PREDICCION_DESERCION",
                        principalColumn: "prediccion_id");
                });

            migrationBuilder.CreateTable(
                name: "SESION_ENTRENAMIENTO",
                columns: table => new
                {
                    sesion_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    asignacion_id = table.Column<int>(type: "int", nullable: false),
                    fecha = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysdatetime())"),
                    estado = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false, defaultValue: "EN_PROGRESO")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SESION_ENTRENAMIENTO", x => x.sesion_id);
                    table.ForeignKey(
                        name: "FK_SESION_ASIGNACION",
                        column: x => x.asignacion_id,
                        principalTable: "ASIGNACION_RUTINA",
                        principalColumn: "asignacion_id");
                });

            migrationBuilder.CreateTable(
                name: "CALIFICACION_RECOMENDACION",
                columns: table => new
                {
                    calificacion_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    recomendacion_id = table.Column<int>(type: "int", nullable: false),
                    cliente_id = table.Column<int>(type: "int", nullable: false),
                    valor = table.Column<int>(type: "int", nullable: false),
                    comentario = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    fecha = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CALIFICACION_RECOMENDACION", x => x.calificacion_id);
                    table.ForeignKey(
                        name: "FK_CALIFICACION_CLIENTE",
                        column: x => x.cliente_id,
                        principalTable: "CLIENTE",
                        principalColumn: "usuario_id");
                    table.ForeignKey(
                        name: "FK_CALIFICACION_RECOMENDACION",
                        column: x => x.recomendacion_id,
                        principalTable: "RECOMENDACION",
                        principalColumn: "recomendacion_id");
                });

            migrationBuilder.CreateTable(
                name: "SESION_EJERCICIO",
                columns: table => new
                {
                    sesion_id = table.Column<int>(type: "int", nullable: false),
                    ejercicio_id = table.Column<int>(type: "int", nullable: false),
                    series_completadas = table.Column<int>(type: "int", nullable: true),
                    repeticiones_realizadas = table.Column<int>(type: "int", nullable: true),
                    peso_utilizado = table.Column<decimal>(type: "decimal(8,2)", nullable: true),
                    completado = table.Column<bool>(type: "bit", nullable: false),
                    fecha_marcado = table.Column<DateTime>(type: "datetime2", nullable: true),
                    observaciones = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SESION_EJERCICIO", x => new { x.sesion_id, x.ejercicio_id });
                    table.ForeignKey(
                        name: "FK_SESION_EJERCICIO_EJERCICIO",
                        column: x => x.ejercicio_id,
                        principalTable: "EJERCICIO",
                        principalColumn: "ejercicio_id");
                    table.ForeignKey(
                        name: "FK_SESION_EJERCICIO_SESION",
                        column: x => x.sesion_id,
                        principalTable: "SESION_ENTRENAMIENTO",
                        principalColumn: "sesion_id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ASIGNACION_CLIENTE",
                table: "ASIGNACION_RUTINA",
                column: "cliente_id");

            migrationBuilder.CreateIndex(
                name: "IX_ASIGNACION_ENTRENADOR",
                table: "ASIGNACION_RUTINA",
                column: "entrenador_id");

            migrationBuilder.CreateIndex(
                name: "IX_ASIGNACION_RUTINA_rutina_id",
                table: "ASIGNACION_RUTINA",
                column: "rutina_id");

            migrationBuilder.CreateIndex(
                name: "IX_ASISTENCIA_CLIENTE_FECHA",
                table: "ASISTENCIA",
                columns: new[] { "cliente_id", "fecha" });

            migrationBuilder.CreateIndex(
                name: "UQ_ASISTENCIA_CLIENTE_FECHA",
                table: "ASISTENCIA",
                columns: new[] { "cliente_id", "fecha" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AUDITORIA_USUARIO_FECHA",
                table: "AUDITORIA",
                columns: new[] { "usuario_id", "fecha" });

            migrationBuilder.CreateIndex(
                name: "IX_CALIFICACION_RECOMENDACION_cliente_id",
                table: "CALIFICACION_RECOMENDACION",
                column: "cliente_id");

            migrationBuilder.CreateIndex(
                name: "UQ_CALIFICACION_RECOMENDACION",
                table: "CALIFICACION_RECOMENDACION",
                column: "recomendacion_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CLIENTE_BADGE",
                table: "CLIENTE",
                column: "badge_numero");

            migrationBuilder.CreateIndex(
                name: "IX_CLIENTE_ENTRENADOR",
                table: "CLIENTE",
                column: "entrenador_id");

            migrationBuilder.CreateIndex(
                name: "UQ_CLIENTE_BADGE",
                table: "CLIENTE",
                column: "badge_numero",
                unique: true,
                filter: "[badge_numero] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_EVALUACION_CLIENTE",
                table: "EVALUACION_FISICA",
                column: "cliente_id");

            migrationBuilder.CreateIndex(
                name: "IX_EVALUACION_FISICA_entrenador_id",
                table: "EVALUACION_FISICA",
                column: "entrenador_id");

            migrationBuilder.CreateIndex(
                name: "IX_EVENTO_administrador_id",
                table: "EVENTO",
                column: "administrador_id");

            migrationBuilder.CreateIndex(
                name: "IX_HORARIO_TAREA_usuario_id",
                table: "HORARIO_TAREA",
                column: "usuario_id");

            migrationBuilder.CreateIndex(
                name: "IX_INSCRIPCION_EVENTO_cliente_id",
                table: "INSCRIPCION_EVENTO",
                column: "cliente_id");

            migrationBuilder.CreateIndex(
                name: "IX_MEMBRESIA_CLIENTE",
                table: "MEMBRESIA",
                column: "cliente_id");

            migrationBuilder.CreateIndex(
                name: "IX_MEMBRESIA_plan_id",
                table: "MEMBRESIA",
                column: "plan_id");

            migrationBuilder.CreateIndex(
                name: "IX_META_cliente_id",
                table: "META",
                column: "cliente_id");

            migrationBuilder.CreateIndex(
                name: "IX_MULTIMEDIA_EJERCICIO_ejercicio_id",
                table: "MULTIMEDIA_EJERCICIO",
                column: "ejercicio_id");

            migrationBuilder.CreateIndex(
                name: "IX_NOTIFICACION_USUARIO",
                table: "NOTIFICACION",
                column: "usuario_id");

            migrationBuilder.CreateIndex(
                name: "IX_OBSERVACION_ENTRENADOR_cliente_id",
                table: "OBSERVACION_ENTRENADOR",
                column: "cliente_id");

            migrationBuilder.CreateIndex(
                name: "IX_OBSERVACION_ENTRENADOR_entrenador_id",
                table: "OBSERVACION_ENTRENADOR",
                column: "entrenador_id");

            migrationBuilder.CreateIndex(
                name: "IX_PAGO_MEMBRESIA",
                table: "PAGO",
                column: "membresia_id");

            migrationBuilder.CreateIndex(
                name: "IX_PAGO_registrado_por",
                table: "PAGO",
                column: "registrado_por");

            migrationBuilder.CreateIndex(
                name: "IX_PARAMETRO_RECOMENDACION_administrador_id",
                table: "PARAMETRO_RECOMENDACION",
                column: "administrador_id");

            migrationBuilder.CreateIndex(
                name: "UQ_PERMISO_NOMBRE",
                table: "PERMISO",
                column: "nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PREDICCION_CLIENTE",
                table: "PREDICCION_DESERCION",
                column: "cliente_id");

            migrationBuilder.CreateIndex(
                name: "IX_RECOMENDACION_CLIENTE",
                table: "RECOMENDACION",
                column: "cliente_id");

            migrationBuilder.CreateIndex(
                name: "IX_RECOMENDACION_entrenador_id",
                table: "RECOMENDACION",
                column: "entrenador_id");

            migrationBuilder.CreateIndex(
                name: "IX_RECOMENDACION_rutina_sugerida_id",
                table: "RECOMENDACION",
                column: "rutina_sugerida_id");

            migrationBuilder.CreateIndex(
                name: "IX_REPORTE_generado_por",
                table: "REPORTE",
                column: "generado_por");

            migrationBuilder.CreateIndex(
                name: "UQ_ROL_NOMBRE",
                table: "ROL",
                column: "nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ROL_PERMISO_permiso_id",
                table: "ROL_PERMISO",
                column: "permiso_id");

            migrationBuilder.CreateIndex(
                name: "IX_RUTINA_ENTRENADOR",
                table: "RUTINA",
                column: "entrenador_id");

            migrationBuilder.CreateIndex(
                name: "IX_RUTINA_EJERCICIO_ejercicio_id",
                table: "RUTINA_EJERCICIO",
                column: "ejercicio_id");

            migrationBuilder.CreateIndex(
                name: "UQ_RUTINA_EJERCICIO_ORDEN",
                table: "RUTINA_EJERCICIO",
                columns: new[] { "rutina_id", "orden" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SESION_EJERCICIO_ejercicio_id",
                table: "SESION_EJERCICIO",
                column: "ejercicio_id");

            migrationBuilder.CreateIndex(
                name: "IX_SESION_ASIGNACION",
                table: "SESION_ENTRENAMIENTO",
                column: "asignacion_id");

            migrationBuilder.CreateIndex(
                name: "IX_USUARIO_ROL",
                table: "USUARIO",
                column: "rol_id");

            migrationBuilder.CreateIndex(
                name: "UQ_USUARIO_CORREO",
                table: "USUARIO",
                column: "correo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VALIDACION_PREDICCION_administrador_id",
                table: "VALIDACION_PREDICCION",
                column: "administrador_id");

            migrationBuilder.CreateIndex(
                name: "UQ_VALIDACION_PREDICCION",
                table: "VALIDACION_PREDICCION",
                column: "prediccion_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ASISTENCIA");

            migrationBuilder.DropTable(
                name: "AUDITORIA");

            migrationBuilder.DropTable(
                name: "CALIFICACION_RECOMENDACION");

            migrationBuilder.DropTable(
                name: "EVALUACION_FISICA");

            migrationBuilder.DropTable(
                name: "HORARIO_TAREA");

            migrationBuilder.DropTable(
                name: "INSCRIPCION_EVENTO");

            migrationBuilder.DropTable(
                name: "META");

            migrationBuilder.DropTable(
                name: "MULTIMEDIA_EJERCICIO");

            migrationBuilder.DropTable(
                name: "NOTIFICACION");

            migrationBuilder.DropTable(
                name: "OBSERVACION_ENTRENADOR");

            migrationBuilder.DropTable(
                name: "PAGO");

            migrationBuilder.DropTable(
                name: "PARAMETRO_RECOMENDACION");

            migrationBuilder.DropTable(
                name: "REPORTE");

            migrationBuilder.DropTable(
                name: "ROL_PERMISO");

            migrationBuilder.DropTable(
                name: "RUTINA_EJERCICIO");

            migrationBuilder.DropTable(
                name: "SESION_EJERCICIO");

            migrationBuilder.DropTable(
                name: "VALIDACION_PREDICCION");

            migrationBuilder.DropTable(
                name: "RECOMENDACION");

            migrationBuilder.DropTable(
                name: "EVENTO");

            migrationBuilder.DropTable(
                name: "MEMBRESIA");

            migrationBuilder.DropTable(
                name: "PERMISO");

            migrationBuilder.DropTable(
                name: "EJERCICIO");

            migrationBuilder.DropTable(
                name: "SESION_ENTRENAMIENTO");

            migrationBuilder.DropTable(
                name: "PREDICCION_DESERCION");

            migrationBuilder.DropTable(
                name: "ADMINISTRADOR");

            migrationBuilder.DropTable(
                name: "PLAN_MEMBRESIA");

            migrationBuilder.DropTable(
                name: "ASIGNACION_RUTINA");

            migrationBuilder.DropTable(
                name: "CLIENTE");

            migrationBuilder.DropTable(
                name: "RUTINA");

            migrationBuilder.DropTable(
                name: "ENTRENADOR");

            migrationBuilder.DropTable(
                name: "USUARIO");

            migrationBuilder.DropTable(
                name: "ROL");
        }
    }
}
