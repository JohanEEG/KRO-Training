/* ============================================================
   BASE DE DATOS: KRO TRAINING
   MOTOR: Microsoft SQL Server
   ============================================================ */


/* ============================================================
   1. CREAR BASE DE DATOS
   ============================================================ */

IF DB_ID('KRO_Training') IS NULL
BEGIN
    CREATE DATABASE KRO_Training;
END
GO

USE KRO_Training;
GO


/* ============================================================
   2. ELIMINAR TABLAS SI YA EXISTEN
      Se eliminan primero las tablas con dependencias.
   ============================================================ */

IF OBJECT_ID('dbo.VALIDACION_PREDICCION', 'U') IS NOT NULL DROP TABLE dbo.VALIDACION_PREDICCION;
IF OBJECT_ID('dbo.CALIFICACION_RECOMENDACION', 'U') IS NOT NULL DROP TABLE dbo.CALIFICACION_RECOMENDACION;
IF OBJECT_ID('dbo.PREDICCION_DESERCION', 'U') IS NOT NULL DROP TABLE dbo.PREDICCION_DESERCION;
IF OBJECT_ID('dbo.RECOMENDACION', 'U') IS NOT NULL DROP TABLE dbo.RECOMENDACION;
IF OBJECT_ID('dbo.INSCRIPCION_EVENTO', 'U') IS NOT NULL DROP TABLE dbo.INSCRIPCION_EVENTO;
IF OBJECT_ID('dbo.EVENTO', 'U') IS NOT NULL DROP TABLE dbo.EVENTO;
IF OBJECT_ID('dbo.PAGO', 'U') IS NOT NULL DROP TABLE dbo.PAGO;
IF OBJECT_ID('dbo.MEMBRESIA', 'U') IS NOT NULL DROP TABLE dbo.MEMBRESIA;
IF OBJECT_ID('dbo.PLAN_MEMBRESIA', 'U') IS NOT NULL DROP TABLE dbo.PLAN_MEMBRESIA;
IF OBJECT_ID('dbo.META', 'U') IS NOT NULL DROP TABLE dbo.META;
IF OBJECT_ID('dbo.EVALUACION_FISICA', 'U') IS NOT NULL DROP TABLE dbo.EVALUACION_FISICA;
IF OBJECT_ID('dbo.SESION_EJERCICIO', 'U') IS NOT NULL DROP TABLE dbo.SESION_EJERCICIO;
IF OBJECT_ID('dbo.SESION_ENTRENAMIENTO', 'U') IS NOT NULL DROP TABLE dbo.SESION_ENTRENAMIENTO;
IF OBJECT_ID('dbo.ASIGNACION_RUTINA', 'U') IS NOT NULL DROP TABLE dbo.ASIGNACION_RUTINA;
IF OBJECT_ID('dbo.RUTINA_EJERCICIO', 'U') IS NOT NULL DROP TABLE dbo.RUTINA_EJERCICIO;
IF OBJECT_ID('dbo.MULTIMEDIA_EJERCICIO', 'U') IS NOT NULL DROP TABLE dbo.MULTIMEDIA_EJERCICIO;
IF OBJECT_ID('dbo.EJERCICIO', 'U') IS NOT NULL DROP TABLE dbo.EJERCICIO;
IF OBJECT_ID('dbo.RUTINA', 'U') IS NOT NULL DROP TABLE dbo.RUTINA;
IF OBJECT_ID('dbo.OBSERVACION_ENTRENADOR', 'U') IS NOT NULL DROP TABLE dbo.OBSERVACION_ENTRENADOR;
IF OBJECT_ID('dbo.ASISTENCIA', 'U') IS NOT NULL DROP TABLE dbo.ASISTENCIA;
IF OBJECT_ID('dbo.HORARIO_TAREA', 'U') IS NOT NULL DROP TABLE dbo.HORARIO_TAREA;
IF OBJECT_ID('dbo.NOTIFICACION', 'U') IS NOT NULL DROP TABLE dbo.NOTIFICACION;
IF OBJECT_ID('dbo.AUDITORIA', 'U') IS NOT NULL DROP TABLE dbo.AUDITORIA;
IF OBJECT_ID('dbo.REPORTE', 'U') IS NOT NULL DROP TABLE dbo.REPORTE;
IF OBJECT_ID('dbo.PARAMETRO_RECOMENDACION', 'U') IS NOT NULL DROP TABLE dbo.PARAMETRO_RECOMENDACION;
IF OBJECT_ID('dbo.ADMINISTRADOR', 'U') IS NOT NULL DROP TABLE dbo.ADMINISTRADOR;
IF OBJECT_ID('dbo.ENTRENADOR', 'U') IS NOT NULL DROP TABLE dbo.ENTRENADOR;
IF OBJECT_ID('dbo.CLIENTE', 'U') IS NOT NULL DROP TABLE dbo.CLIENTE;
IF OBJECT_ID('dbo.ROL_PERMISO', 'U') IS NOT NULL DROP TABLE dbo.ROL_PERMISO;
IF OBJECT_ID('dbo.PERMISO', 'U') IS NOT NULL DROP TABLE dbo.PERMISO;
IF OBJECT_ID('dbo.ROL', 'U') IS NOT NULL DROP TABLE dbo.ROL;
IF OBJECT_ID('dbo.USUARIO', 'U') IS NOT NULL DROP TABLE dbo.USUARIO;
GO


/* ============================================================
   3. SEGURIDAD Y USUARIOS
   ============================================================ */

CREATE TABLE ROL
(
    rol_id INT IDENTITY(1,1) NOT NULL,
    nombre NVARCHAR(50) NOT NULL,

    CONSTRAINT PK_ROL
        PRIMARY KEY (rol_id),

    CONSTRAINT UQ_ROL_NOMBRE
        UNIQUE (nombre)
);
GO


CREATE TABLE PERMISO
(
    permiso_id INT IDENTITY(1,1) NOT NULL,
    nombre NVARCHAR(100) NOT NULL,
    descripcion NVARCHAR(255) NULL,

    CONSTRAINT PK_PERMISO
        PRIMARY KEY (permiso_id),

    CONSTRAINT UQ_PERMISO_NOMBRE
        UNIQUE (nombre)
);
GO


CREATE TABLE ROL_PERMISO
(
    rol_id INT NOT NULL,
    permiso_id INT NOT NULL,

    CONSTRAINT PK_ROL_PERMISO
        PRIMARY KEY (rol_id, permiso_id),

    CONSTRAINT FK_ROL_PERMISO_ROL
        FOREIGN KEY (rol_id)
        REFERENCES ROL(rol_id),

    CONSTRAINT FK_ROL_PERMISO_PERMISO
        FOREIGN KEY (permiso_id)
        REFERENCES PERMISO(permiso_id)
);
GO


CREATE TABLE USUARIO
(
    usuario_id INT IDENTITY(1,1) NOT NULL,
    nombre NVARCHAR(150) NOT NULL,
    correo NVARCHAR(150) NOT NULL,
    contrasena_hash NVARCHAR(255) NOT NULL,
    telefono NVARCHAR(30) NULL,
    foto_perfil NVARCHAR(500) NULL,
    estado NVARCHAR(20) NOT NULL DEFAULT 'ACTIVO',
    fecha_registro DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    rol_id INT NOT NULL,

    CONSTRAINT PK_USUARIO
        PRIMARY KEY (usuario_id),

    CONSTRAINT UQ_USUARIO_CORREO
        UNIQUE (correo),

    CONSTRAINT FK_USUARIO_ROL
        FOREIGN KEY (rol_id)
        REFERENCES ROL(rol_id),

    CONSTRAINT CK_USUARIO_ESTADO
        CHECK (estado IN ('ACTIVO', 'INACTIVO', 'BLOQUEADO'))
);
GO


CREATE TABLE CLIENTE
(
    usuario_id INT NOT NULL,
    badge_numero NVARCHAR(50) NULL, -- Número de carné o badge para control de asistencia
    objetivo NVARCHAR(100) NULL,
    nivel NVARCHAR(30) NULL,
    entrenador_id INT NULL,

    CONSTRAINT PK_CLIENTE
        PRIMARY KEY (usuario_id),

    CONSTRAINT UQ_CLIENTE_BADGE
        UNIQUE (badge_numero),

    CONSTRAINT FK_CLIENTE_USUARIO
        FOREIGN KEY (usuario_id)
        REFERENCES USUARIO(usuario_id),

    CONSTRAINT CK_CLIENTE_NIVEL
        CHECK (nivel IS NULL OR nivel IN
        ('PRINCIPIANTE', 'INTERMEDIO', 'AVANZADO'))
);
GO


CREATE TABLE ENTRENADOR
(
    usuario_id INT NOT NULL,
    especialidad NVARCHAR(150) NULL,

    CONSTRAINT PK_ENTRENADOR
        PRIMARY KEY (usuario_id),

    CONSTRAINT FK_ENTRENADOR_USUARIO
        FOREIGN KEY (usuario_id)
        REFERENCES USUARIO(usuario_id)
);
GO


/* FK CLIENTE -> ENTRENADOR */
ALTER TABLE CLIENTE
ADD CONSTRAINT FK_CLIENTE_ENTRENADOR
    FOREIGN KEY (entrenador_id)
    REFERENCES ENTRENADOR(usuario_id);
GO


CREATE TABLE ADMINISTRADOR
(
    usuario_id INT NOT NULL,

    CONSTRAINT PK_ADMINISTRADOR
        PRIMARY KEY (usuario_id),

    CONSTRAINT FK_ADMINISTRADOR_USUARIO
        FOREIGN KEY (usuario_id)
        REFERENCES USUARIO(usuario_id)
);
GO


/* ============================================================
   4. TAREAS, NOTIFICACIONES Y AUDITORÍA
   ============================================================ */

CREATE TABLE HORARIO_TAREA
(
    horario_id INT IDENTITY(1,1) NOT NULL,
    usuario_id INT NOT NULL,
    fecha DATE NOT NULL,
    tarea NVARCHAR(255) NOT NULL,
    estado NVARCHAR(30) NOT NULL DEFAULT 'PENDIENTE',

    CONSTRAINT PK_HORARIO_TAREA
        PRIMARY KEY (horario_id),

    CONSTRAINT FK_HORARIO_TAREA_USUARIO
        FOREIGN KEY (usuario_id)
        REFERENCES USUARIO(usuario_id)
);
GO


CREATE TABLE NOTIFICACION
(
    notificacion_id INT IDENTITY(1,1) NOT NULL,
    usuario_id INT NOT NULL,
    tipo NVARCHAR(50) NOT NULL,
    mensaje NVARCHAR(500) NOT NULL,
    fecha DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    estado NVARCHAR(30) NOT NULL DEFAULT 'NO_LEIDA',

    CONSTRAINT PK_NOTIFICACION
        PRIMARY KEY (notificacion_id),

    CONSTRAINT FK_NOTIFICACION_USUARIO
        FOREIGN KEY (usuario_id)
        REFERENCES USUARIO(usuario_id)
);
GO


CREATE TABLE AUDITORIA
(
    auditoria_id INT IDENTITY(1,1) NOT NULL,
    usuario_id INT NOT NULL,
    accion NVARCHAR(100) NOT NULL,
    fecha DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    detalle NVARCHAR(MAX) NULL,

    CONSTRAINT PK_AUDITORIA
        PRIMARY KEY (auditoria_id),

    CONSTRAINT FK_AUDITORIA_USUARIO
        FOREIGN KEY (usuario_id)
        REFERENCES USUARIO(usuario_id)
);
GO


CREATE TABLE REPORTE
(
    reporte_id INT IDENTITY(1,1) NOT NULL,
    tipo NVARCHAR(100) NOT NULL,
    periodo_inicio DATE NULL,
    periodo_fin DATE NULL,
    generado_por INT NOT NULL,
    fecha_generacion DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    formato_exportado NVARCHAR(20) NULL,

    CONSTRAINT PK_REPORTE
        PRIMARY KEY (reporte_id),

    CONSTRAINT FK_REPORTE_USUARIO
        FOREIGN KEY (generado_por)
        REFERENCES USUARIO(usuario_id)
);
GO


/* ============================================================
   5. EJERCICIOS
   ============================================================ */

CREATE TABLE EJERCICIO
(
    ejercicio_id INT IDENTITY(1,1) NOT NULL,
    nombre NVARCHAR(150) NOT NULL,
    descripcion NVARCHAR(MAX) NULL,
    grupo_muscular NVARCHAR(100) NULL,

    CONSTRAINT PK_EJERCICIO
        PRIMARY KEY (ejercicio_id)
);
GO


CREATE TABLE MULTIMEDIA_EJERCICIO
(
    multimedia_id INT IDENTITY(1,1) NOT NULL,
    ejercicio_id INT NOT NULL,
    tipo NVARCHAR(30) NOT NULL,
    url NVARCHAR(500) NOT NULL,

    CONSTRAINT PK_MULTIMEDIA_EJERCICIO
        PRIMARY KEY (multimedia_id),

    CONSTRAINT FK_MULTIMEDIA_EJERCICIO
        FOREIGN KEY (ejercicio_id)
        REFERENCES EJERCICIO(ejercicio_id),

    CONSTRAINT CK_MULTIMEDIA_TIPO
        CHECK (tipo IN ('IMAGEN', 'VIDEO', 'GIF', 'OTRO'))
);
GO


/* ============================================================
   6. RUTINAS
   ============================================================ */

CREATE TABLE RUTINA
(
    rutina_id INT IDENTITY(1,1) NOT NULL,
    nombre NVARCHAR(150) NOT NULL,
    descripcion NVARCHAR(MAX) NULL,
    objetivo NVARCHAR(100) NULL,
    nivel NVARCHAR(30) NULL,
    entrenador_id INT NOT NULL,
    fecha_creacion DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    estado NVARCHAR(30) NOT NULL DEFAULT 'ACTIVA',

    CONSTRAINT PK_RUTINA
        PRIMARY KEY (rutina_id),

    CONSTRAINT FK_RUTINA_ENTRENADOR
        FOREIGN KEY (entrenador_id)
        REFERENCES ENTRENADOR(usuario_id),

    CONSTRAINT CK_RUTINA_NIVEL
        CHECK (nivel IS NULL OR nivel IN
        ('PRINCIPIANTE', 'INTERMEDIO', 'AVANZADO'))
);
GO


CREATE TABLE RUTINA_EJERCICIO
(
    rutina_id INT NOT NULL,
    ejercicio_id INT NOT NULL,
    orden INT NOT NULL,
    series INT NOT NULL,
    repeticiones INT NOT NULL,
    descanso_segundos INT NOT NULL DEFAULT 60,

    CONSTRAINT PK_RUTINA_EJERCICIO
        PRIMARY KEY (rutina_id, ejercicio_id),

    CONSTRAINT FK_RUTINA_EJERCICIO_RUTINA
        FOREIGN KEY (rutina_id)
        REFERENCES RUTINA(rutina_id),

    CONSTRAINT FK_RUTINA_EJERCICIO_EJERCICIO
        FOREIGN KEY (ejercicio_id)
        REFERENCES EJERCICIO(ejercicio_id),

    CONSTRAINT UQ_RUTINA_EJERCICIO_ORDEN
        UNIQUE (rutina_id, orden),

    CONSTRAINT CK_RUTINA_EJERCICIO_SERIES
        CHECK (series > 0),

    CONSTRAINT CK_RUTINA_EJERCICIO_REPETICIONES
        CHECK (repeticiones > 0),

    CONSTRAINT CK_RUTINA_EJERCICIO_DESCANSO
        CHECK (descanso_segundos >= 0),

    CONSTRAINT CK_RUTINA_EJERCICIO_ORDEN_POSITIVO
        CHECK (orden > 0)
);
GO


/* ============================================================
   7. ASIGNACIÓN Y SESIONES
   ============================================================ */

CREATE TABLE ASIGNACION_RUTINA
(
    asignacion_id INT IDENTITY(1,1) NOT NULL,
    cliente_id INT NOT NULL,
    rutina_id INT NOT NULL,
    entrenador_id INT NOT NULL,
    fecha_asignacion DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    estado NVARCHAR(30) NOT NULL DEFAULT 'ACTIVA',

    CONSTRAINT PK_ASIGNACION_RUTINA
        PRIMARY KEY (asignacion_id),

    CONSTRAINT FK_ASIGNACION_CLIENTE
        FOREIGN KEY (cliente_id)
        REFERENCES CLIENTE(usuario_id),

    CONSTRAINT FK_ASIGNACION_RUTINA
        FOREIGN KEY (rutina_id)
        REFERENCES RUTINA(rutina_id),

    CONSTRAINT FK_ASIGNACION_ENTRENADOR
        FOREIGN KEY (entrenador_id)
        REFERENCES ENTRENADOR(usuario_id)
);
GO


CREATE TABLE SESION_ENTRENAMIENTO
(
    sesion_id INT IDENTITY(1,1) NOT NULL,
    asignacion_id INT NOT NULL,
    fecha DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    estado NVARCHAR(30) NOT NULL DEFAULT 'EN_PROGRESO',

    CONSTRAINT PK_SESION_ENTRENAMIENTO
        PRIMARY KEY (sesion_id),

    CONSTRAINT FK_SESION_ASIGNACION
        FOREIGN KEY (asignacion_id)
        REFERENCES ASIGNACION_RUTINA(asignacion_id)
);
GO


CREATE TABLE SESION_EJERCICIO
(
    sesion_id INT NOT NULL,
    ejercicio_id INT NOT NULL,
    series_completadas INT NULL,
    repeticiones_realizadas INT NULL,
    peso_utilizado DECIMAL(8,2) NULL,
    completado BIT NOT NULL DEFAULT 0,
    fecha_marcado DATETIME2 NULL,
    observaciones NVARCHAR(500) NULL,

    CONSTRAINT PK_SESION_EJERCICIO
        PRIMARY KEY (sesion_id, ejercicio_id),

    CONSTRAINT FK_SESION_EJERCICIO_SESION
        FOREIGN KEY (sesion_id)
        REFERENCES SESION_ENTRENAMIENTO(sesion_id),

    CONSTRAINT FK_SESION_EJERCICIO_EJERCICIO
        FOREIGN KEY (ejercicio_id)
        REFERENCES EJERCICIO(ejercicio_id),

    CONSTRAINT CK_SESION_SERIES
        CHECK (series_completadas IS NULL OR series_completadas >= 0),

    CONSTRAINT CK_SESION_REPETICIONES
        CHECK (repeticiones_realizadas IS NULL OR repeticiones_realizadas >= 0),

    CONSTRAINT CK_SESION_PESO
        CHECK (peso_utilizado IS NULL OR peso_utilizado >= 0)
);
GO


/* ============================================================
   8. EVALUACIONES FÍSICAS
   ============================================================ */

CREATE TABLE EVALUACION_FISICA
(
    evaluacion_id INT IDENTITY(1,1) NOT NULL,
    cliente_id INT NOT NULL,
    entrenador_id INT NOT NULL,
    fecha DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    peso DECIMAL(6,2) NULL,
    estatura DECIMAL(5,2) NULL,
    medidas NVARCHAR(1000) NULL,
    observaciones NVARCHAR(MAX) NULL,
    tipo NVARCHAR(50) NULL,

    CONSTRAINT PK_EVALUACION_FISICA
        PRIMARY KEY (evaluacion_id),

    CONSTRAINT FK_EVALUACION_CLIENTE
        FOREIGN KEY (cliente_id)
        REFERENCES CLIENTE(usuario_id),

    CONSTRAINT FK_EVALUACION_ENTRENADOR
        FOREIGN KEY (entrenador_id)
        REFERENCES ENTRENADOR(usuario_id),

    CONSTRAINT CK_EVALUACION_PESO
        CHECK (peso IS NULL OR peso > 0),

    CONSTRAINT CK_EVALUACION_ESTATURA
        CHECK (estatura IS NULL OR estatura > 0)
);
GO


/* ============================================================
   9. METAS
   ============================================================ */

CREATE TABLE META
(
    meta_id INT IDENTITY(1,1) NOT NULL,
    cliente_id INT NOT NULL,
    tipo NVARCHAR(100) NOT NULL,
    valor_objetivo DECIMAL(10,2) NULL,
    fecha_inicio DATE NOT NULL,
    fecha_limite DATE NULL,
    estado NVARCHAR(30) NOT NULL DEFAULT 'ACTIVA',

    CONSTRAINT PK_META
        PRIMARY KEY (meta_id),

    CONSTRAINT FK_META_CLIENTE
        FOREIGN KEY (cliente_id)
        REFERENCES CLIENTE(usuario_id),

    CONSTRAINT CK_META_FECHAS
        CHECK (fecha_limite IS NULL OR fecha_limite >= fecha_inicio)
);
GO


/* ============================================================
   10. MEMBRESÍAS Y PAGOS
   ============================================================ */

CREATE TABLE PLAN_MEMBRESIA
(
    plan_id INT IDENTITY(1,1) NOT NULL,
    nombre NVARCHAR(100) NOT NULL,
    duracion_meses INT NOT NULL,
    precio DECIMAL(10,2) NOT NULL,

    CONSTRAINT PK_PLAN_MEMBRESIA
        PRIMARY KEY (plan_id),

    CONSTRAINT CK_PLAN_DURACION
        CHECK (duracion_meses > 0),

    CONSTRAINT CK_PLAN_PRECIO
        CHECK (precio >= 0)
);
GO


CREATE TABLE MEMBRESIA
(
    membresia_id INT IDENTITY(1,1) NOT NULL,
    cliente_id INT NOT NULL,
    plan_id INT NOT NULL,
    fecha_inicio DATE NOT NULL,
    fecha_fin DATE NOT NULL,
    estado NVARCHAR(30) NOT NULL DEFAULT 'ACTIVA',

    CONSTRAINT PK_MEMBRESIA
        PRIMARY KEY (membresia_id),

    CONSTRAINT FK_MEMBRESIA_CLIENTE
        FOREIGN KEY (cliente_id)
        REFERENCES CLIENTE(usuario_id),

    CONSTRAINT FK_MEMBRESIA_PLAN
        FOREIGN KEY (plan_id)
        REFERENCES PLAN_MEMBRESIA(plan_id),

    CONSTRAINT CK_MEMBRESIA_FECHAS
        CHECK (fecha_fin >= fecha_inicio)
);
GO


CREATE TABLE PAGO
(
    pago_id INT IDENTITY(1,1) NOT NULL,
    membresia_id INT NOT NULL,
    monto DECIMAL(10,2) NOT NULL,
    fecha_pago DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    metodo_pago NVARCHAR(30) NOT NULL,
    registrado_por INT NOT NULL,

    CONSTRAINT PK_PAGO
        PRIMARY KEY (pago_id),

    CONSTRAINT FK_PAGO_MEMBRESIA
        FOREIGN KEY (membresia_id)
        REFERENCES MEMBRESIA(membresia_id),

    CONSTRAINT FK_PAGO_USUARIO
        FOREIGN KEY (registrado_por)
        REFERENCES USUARIO(usuario_id),

    CONSTRAINT CK_PAGO_MONTO
        CHECK (monto > 0),

    CONSTRAINT CK_PAGO_METODO
        CHECK (metodo_pago IN
        ('EFECTIVO', 'TARJETA', 'SINPE', 'TRANSFERENCIA', 'OTRO'))
);
GO


/* ============================================================
   11. ASISTENCIA
   ============================================================ */

CREATE TABLE ASISTENCIA
(
    asistencia_id INT IDENTITY(1,1) NOT NULL,
    cliente_id INT NOT NULL,
    fecha DATE NOT NULL,
    hora TIME NOT NULL,

    CONSTRAINT PK_ASISTENCIA
        PRIMARY KEY (asistencia_id),

    CONSTRAINT FK_ASISTENCIA_CLIENTE
        FOREIGN KEY (cliente_id)
        REFERENCES CLIENTE(usuario_id),

    CONSTRAINT UQ_ASISTENCIA_CLIENTE_FECHA
        UNIQUE (cliente_id, fecha)
);
GO


/* ============================================================
   12. EVENTOS
   ============================================================ */

CREATE TABLE EVENTO
(
    evento_id INT IDENTITY(1,1) NOT NULL,
    nombre NVARCHAR(150) NOT NULL,
    descripcion NVARCHAR(MAX) NULL,
    fecha DATETIME2 NOT NULL,
    cupo_maximo INT NOT NULL,
    estado NVARCHAR(30) NOT NULL DEFAULT 'ACTIVO',
    administrador_id INT NOT NULL,

    CONSTRAINT PK_EVENTO
        PRIMARY KEY (evento_id),

    CONSTRAINT FK_EVENTO_ADMINISTRADOR
        FOREIGN KEY (administrador_id)
        REFERENCES ADMINISTRADOR(usuario_id),

    CONSTRAINT CK_EVENTO_CUPO
        CHECK (cupo_maximo > 0)
);
GO


CREATE TABLE INSCRIPCION_EVENTO
(
    evento_id INT NOT NULL,
    cliente_id INT NOT NULL,
    fecha_inscripcion DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    estado NVARCHAR(30) NOT NULL DEFAULT 'INSCRITO',

    CONSTRAINT PK_INSCRIPCION_EVENTO
        PRIMARY KEY (evento_id, cliente_id),

    CONSTRAINT FK_INSCRIPCION_EVENTO
        FOREIGN KEY (evento_id)
        REFERENCES EVENTO(evento_id),

    CONSTRAINT FK_INSCRIPCION_CLIENTE
        FOREIGN KEY (cliente_id)
        REFERENCES CLIENTE(usuario_id)
);
GO


/* ============================================================
   13. RECOMENDACIONES
   ============================================================ */

CREATE TABLE RECOMENDACION
(
    recomendacion_id INT IDENTITY(1,1) NOT NULL,
    cliente_id INT NOT NULL,
    rutina_sugerida_id INT NOT NULL,
    entrenador_id INT NULL,
    fecha DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    estado NVARCHAR(30) NOT NULL DEFAULT 'PENDIENTE',

    CONSTRAINT PK_RECOMENDACION
        PRIMARY KEY (recomendacion_id),

    CONSTRAINT FK_RECOMENDACION_CLIENTE
        FOREIGN KEY (cliente_id)
        REFERENCES CLIENTE(usuario_id),

    CONSTRAINT FK_RECOMENDACION_RUTINA
        FOREIGN KEY (rutina_sugerida_id)
        REFERENCES RUTINA(rutina_id),

    CONSTRAINT FK_RECOMENDACION_ENTRENADOR
        FOREIGN KEY (entrenador_id)
        REFERENCES ENTRENADOR(usuario_id)
);
GO


CREATE TABLE CALIFICACION_RECOMENDACION
(
    calificacion_id INT IDENTITY(1,1) NOT NULL,
    recomendacion_id INT NOT NULL,
    cliente_id INT NOT NULL,
    valor INT NOT NULL,
    comentario NVARCHAR(500) NULL,
    fecha DATETIME2 NOT NULL DEFAULT SYSDATETIME(),

    CONSTRAINT PK_CALIFICACION_RECOMENDACION
        PRIMARY KEY (calificacion_id),

    CONSTRAINT FK_CALIFICACION_RECOMENDACION
        FOREIGN KEY (recomendacion_id)
        REFERENCES RECOMENDACION(recomendacion_id),

    CONSTRAINT FK_CALIFICACION_CLIENTE
        FOREIGN KEY (cliente_id)
        REFERENCES CLIENTE(usuario_id),

    CONSTRAINT UQ_CALIFICACION_RECOMENDACION
        UNIQUE (recomendacion_id),

    CONSTRAINT CK_CALIFICACION_VALOR
        CHECK (valor BETWEEN 1 AND 5)
);
GO


CREATE TABLE PARAMETRO_RECOMENDACION
(
    parametro_id INT IDENTITY(1,1) NOT NULL,
    nombre NVARCHAR(100) NOT NULL,
    valor NVARCHAR(500) NOT NULL,
    administrador_id INT NOT NULL,

    CONSTRAINT PK_PARAMETRO_RECOMENDACION
        PRIMARY KEY (parametro_id),

    CONSTRAINT FK_PARAMETRO_ADMINISTRADOR
        FOREIGN KEY (administrador_id)
        REFERENCES ADMINISTRADOR(usuario_id)
);
GO


/* ============================================================
   14. PREDICCIÓN DE DESERCIÓN
   ============================================================ */

CREATE TABLE PREDICCION_DESERCION
(
    prediccion_id INT IDENTITY(1,1) NOT NULL,
    cliente_id INT NOT NULL,
    nivel_riesgo NVARCHAR(30) NOT NULL,
    confiabilidad DECIMAL(5,2) NOT NULL,
    fecha DATETIME2 NOT NULL DEFAULT SYSDATETIME(),

    CONSTRAINT PK_PREDICCION_DESERCION
        PRIMARY KEY (prediccion_id),

    CONSTRAINT FK_PREDICCION_CLIENTE
        FOREIGN KEY (cliente_id)
        REFERENCES CLIENTE(usuario_id),

    CONSTRAINT CK_PREDICCION_RIESGO
        CHECK (nivel_riesgo IN ('BAJO', 'MEDIO', 'ALTO')),

    CONSTRAINT CK_PREDICCION_CONFIABILIDAD
        CHECK (confiabilidad BETWEEN 0 AND 100)
);
GO


CREATE TABLE VALIDACION_PREDICCION
(
    validacion_id INT IDENTITY(1,1) NOT NULL,
    prediccion_id INT NOT NULL,
    administrador_id INT NOT NULL,
    resultado NVARCHAR(50) NOT NULL,
    fecha DATETIME2 NOT NULL DEFAULT SYSDATETIME(),

    CONSTRAINT PK_VALIDACION_PREDICCION
        PRIMARY KEY (validacion_id),

    CONSTRAINT FK_VALIDACION_PREDICCION
        FOREIGN KEY (prediccion_id)
        REFERENCES PREDICCION_DESERCION(prediccion_id),

    CONSTRAINT FK_VALIDACION_ADMINISTRADOR
        FOREIGN KEY (administrador_id)
        REFERENCES ADMINISTRADOR(usuario_id),

    CONSTRAINT UQ_VALIDACION_PREDICCION
        UNIQUE (prediccion_id)
);
GO


/* ============================================================
   15. OBSERVACIONES DEL ENTRENADOR
   ============================================================ */

CREATE TABLE OBSERVACION_ENTRENADOR
(
    observacion_id INT IDENTITY(1,1) NOT NULL,
    cliente_id INT NOT NULL,
    entrenador_id INT NOT NULL,
    fecha DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    contenido NVARCHAR(MAX) NOT NULL,

    CONSTRAINT PK_OBSERVACION_ENTRENADOR
        PRIMARY KEY (observacion_id),

    CONSTRAINT FK_OBSERVACION_CLIENTE
        FOREIGN KEY (cliente_id)
        REFERENCES CLIENTE(usuario_id),

    CONSTRAINT FK_OBSERVACION_ENTRENADOR
        FOREIGN KEY (entrenador_id)
        REFERENCES ENTRENADOR(usuario_id)
);
GO


/* ============================================================
   16. ÍNDICES
   ============================================================ */

CREATE INDEX IX_USUARIO_ROL
ON USUARIO(rol_id);
GO

CREATE INDEX IX_CLIENTE_ENTRENADOR
ON CLIENTE(entrenador_id);
GO

CREATE INDEX IX_CLIENTE_BADGE
ON CLIENTE(badge_numero);
GO

CREATE INDEX IX_RUTINA_ENTRENADOR
ON RUTINA(entrenador_id);
GO

CREATE INDEX IX_ASIGNACION_CLIENTE
ON ASIGNACION_RUTINA(cliente_id);
GO

CREATE INDEX IX_ASIGNACION_ENTRENADOR
ON ASIGNACION_RUTINA(entrenador_id);
GO

CREATE INDEX IX_SESION_ASIGNACION
ON SESION_ENTRENAMIENTO(asignacion_id);
GO

CREATE INDEX IX_EVALUACION_CLIENTE
ON EVALUACION_FISICA(cliente_id);
GO

CREATE INDEX IX_MEMBRESIA_CLIENTE
ON MEMBRESIA(cliente_id);
GO

CREATE INDEX IX_PAGO_MEMBRESIA
ON PAGO(membresia_id);
GO

CREATE INDEX IX_ASISTENCIA_CLIENTE_FECHA
ON ASISTENCIA(cliente_id, fecha);
GO

CREATE INDEX IX_RECOMENDACION_CLIENTE
ON RECOMENDACION(cliente_id);
GO

CREATE INDEX IX_PREDICCION_CLIENTE
ON PREDICCION_DESERCION(cliente_id);
GO

CREATE INDEX IX_NOTIFICACION_USUARIO
ON NOTIFICACION(usuario_id);
GO

CREATE INDEX IX_AUDITORIA_USUARIO_FECHA
ON AUDITORIA(usuario_id, fecha);
GO


/* ============================================================
   17. DATOS INICIALES DE ROLES
   ============================================================ */

INSERT INTO ROL (nombre)
VALUES
    ('ADMINISTRADOR'),
    ('ENTRENADOR'),
    ('CLIENTE');
GO


/* ============================================================
   FIN DEL SCRIPT
   ============================================================ */