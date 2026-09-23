using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace KROTraining.Models;

public partial class KroTrainingContext : DbContext
{
    public KroTrainingContext()
    {
    }

    public KroTrainingContext(DbContextOptions<KroTrainingContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Administrador> Administradors { get; set; }

    public virtual DbSet<AsignacionRutina> AsignacionRutinas { get; set; }

    public virtual DbSet<Asistencium> Asistencia { get; set; }

    public virtual DbSet<Auditorium> Auditoria { get; set; }

    public virtual DbSet<CalificacionRecomendacion> CalificacionRecomendacions { get; set; }

    public virtual DbSet<Cliente> Clientes { get; set; }

    public virtual DbSet<Ejercicio> Ejercicios { get; set; }

    public virtual DbSet<Entrenador> Entrenadors { get; set; }

    public virtual DbSet<EvaluacionFisica> EvaluacionFisicas { get; set; }

    public virtual DbSet<Evento> Eventos { get; set; }

    public virtual DbSet<HorarioTarea> HorarioTareas { get; set; }

    public virtual DbSet<InscripcionEvento> InscripcionEventos { get; set; }

    public virtual DbSet<Membresium> Membresia { get; set; }

    public virtual DbSet<Metum> Meta { get; set; }

    public virtual DbSet<MultimediaEjercicio> MultimediaEjercicios { get; set; }

    public virtual DbSet<Notificacion> Notificacions { get; set; }

    public virtual DbSet<ObservacionEntrenador> ObservacionEntrenadors { get; set; }

    public virtual DbSet<Pago> Pagos { get; set; }

    public virtual DbSet<ParametroRecomendacion> ParametroRecomendacions { get; set; }

    public virtual DbSet<Permiso> Permisos { get; set; }

    public virtual DbSet<PlanMembresium> PlanMembresia { get; set; }

    public virtual DbSet<PrediccionDesercion> PrediccionDesercions { get; set; }

    public virtual DbSet<Recomendacion> Recomendacions { get; set; }

    public virtual DbSet<Reporte> Reportes { get; set; }

    public virtual DbSet<Rol> Rols { get; set; }

    public virtual DbSet<Rutina> Rutinas { get; set; }

    public virtual DbSet<RutinaEjercicio> RutinaEjercicios { get; set; }

    public virtual DbSet<SesionEjercicio> SesionEjercicios { get; set; }

    public virtual DbSet<SesionEntrenamiento> SesionEntrenamientos { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    public virtual DbSet<ValidacionPrediccion> ValidacionPrediccions { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=DESKTOP-T9U1F0L\\MSSQLSERVER01;Database=KRO_Training;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Administrador>(entity =>
        {
            entity.HasKey(e => e.UsuarioId);

            entity.ToTable("ADMINISTRADOR");

            entity.Property(e => e.UsuarioId)
                .ValueGeneratedNever()
                .HasColumnName("usuario_id");

            entity.HasOne(d => d.Usuario).WithOne(p => p.Administrador)
                .HasForeignKey<Administrador>(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ADMINISTRADOR_USUARIO");
        });

        modelBuilder.Entity<AsignacionRutina>(entity =>
        {
            entity.HasKey(e => e.AsignacionId);

            entity.ToTable("ASIGNACION_RUTINA");

            entity.HasIndex(e => e.ClienteId, "IX_ASIGNACION_CLIENTE");

            entity.HasIndex(e => e.EntrenadorId, "IX_ASIGNACION_ENTRENADOR");

            entity.Property(e => e.AsignacionId).HasColumnName("asignacion_id");
            entity.Property(e => e.ClienteId).HasColumnName("cliente_id");
            entity.Property(e => e.EntrenadorId).HasColumnName("entrenador_id");
            entity.Property(e => e.Estado)
                .HasMaxLength(30)
                .HasDefaultValue("ACTIVA")
                .HasColumnName("estado");
            entity.Property(e => e.FechaAsignacion)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("fecha_asignacion");
            entity.Property(e => e.RutinaId).HasColumnName("rutina_id");

            entity.HasOne(d => d.Cliente).WithMany(p => p.AsignacionRutinas)
                .HasForeignKey(d => d.ClienteId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ASIGNACION_CLIENTE");

            entity.HasOne(d => d.Entrenador).WithMany(p => p.AsignacionRutinas)
                .HasForeignKey(d => d.EntrenadorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ASIGNACION_ENTRENADOR");

            entity.HasOne(d => d.Rutina).WithMany(p => p.AsignacionRutinas)
                .HasForeignKey(d => d.RutinaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ASIGNACION_RUTINA");
        });

        modelBuilder.Entity<Asistencium>(entity =>
        {
            entity.HasKey(e => e.AsistenciaId);

            entity.ToTable("ASISTENCIA");

            entity.HasIndex(e => new { e.ClienteId, e.Fecha }, "IX_ASISTENCIA_CLIENTE_FECHA");

            entity.HasIndex(e => new { e.ClienteId, e.Fecha }, "UQ_ASISTENCIA_CLIENTE_FECHA").IsUnique();

            entity.Property(e => e.AsistenciaId).HasColumnName("asistencia_id");
            entity.Property(e => e.ClienteId).HasColumnName("cliente_id");
            entity.Property(e => e.Fecha).HasColumnName("fecha");
            entity.Property(e => e.Hora).HasColumnName("hora");

            entity.HasOne(d => d.Cliente).WithMany(p => p.Asistencia)
                .HasForeignKey(d => d.ClienteId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ASISTENCIA_CLIENTE");
        });

        modelBuilder.Entity<Auditorium>(entity =>
        {
            entity.HasKey(e => e.AuditoriaId);

            entity.ToTable("AUDITORIA");

            entity.HasIndex(e => new { e.UsuarioId, e.Fecha }, "IX_AUDITORIA_USUARIO_FECHA");

            entity.Property(e => e.AuditoriaId).HasColumnName("auditoria_id");
            entity.Property(e => e.Accion)
                .HasMaxLength(100)
                .HasColumnName("accion");
            entity.Property(e => e.Detalle).HasColumnName("detalle");
            entity.Property(e => e.Fecha)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("fecha");
            entity.Property(e => e.UsuarioId).HasColumnName("usuario_id");

            entity.HasOne(d => d.Usuario).WithMany(p => p.Auditoria)
                .HasForeignKey(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AUDITORIA_USUARIO");
        });

        modelBuilder.Entity<CalificacionRecomendacion>(entity =>
        {
            entity.HasKey(e => e.CalificacionId);

            entity.ToTable("CALIFICACION_RECOMENDACION");

            entity.HasIndex(e => e.RecomendacionId, "UQ_CALIFICACION_RECOMENDACION").IsUnique();

            entity.Property(e => e.CalificacionId).HasColumnName("calificacion_id");
            entity.Property(e => e.ClienteId).HasColumnName("cliente_id");
            entity.Property(e => e.Comentario)
                .HasMaxLength(500)
                .HasColumnName("comentario");
            entity.Property(e => e.Fecha)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("fecha");
            entity.Property(e => e.RecomendacionId).HasColumnName("recomendacion_id");
            entity.Property(e => e.Valor).HasColumnName("valor");

            entity.HasOne(d => d.Cliente).WithMany(p => p.CalificacionRecomendacions)
                .HasForeignKey(d => d.ClienteId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CALIFICACION_CLIENTE");

            entity.HasOne(d => d.Recomendacion).WithOne(p => p.CalificacionRecomendacion)
                .HasForeignKey<CalificacionRecomendacion>(d => d.RecomendacionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CALIFICACION_RECOMENDACION");
        });

        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.HasKey(e => e.UsuarioId);

            entity.ToTable("CLIENTE");

            entity.HasIndex(e => e.BadgeNumero, "IX_CLIENTE_BADGE");

            entity.HasIndex(e => e.EntrenadorId, "IX_CLIENTE_ENTRENADOR");

            entity.HasIndex(e => e.BadgeNumero, "UQ_CLIENTE_BADGE").IsUnique();

            entity.Property(e => e.UsuarioId)
                .ValueGeneratedNever()
                .HasColumnName("usuario_id");
            entity.Property(e => e.BadgeNumero)
                .HasMaxLength(50)
                .HasColumnName("badge_numero");
            entity.Property(e => e.EntrenadorId).HasColumnName("entrenador_id");
            entity.Property(e => e.Nivel)
                .HasMaxLength(30)
                .HasColumnName("nivel");
            entity.Property(e => e.Objetivo)
                .HasMaxLength(100)
                .HasColumnName("objetivo");

            entity.HasOne(d => d.Entrenador).WithMany(p => p.Clientes)
                .HasForeignKey(d => d.EntrenadorId)
                .HasConstraintName("FK_CLIENTE_ENTRENADOR");

            entity.HasOne(d => d.Usuario).WithOne(p => p.Cliente)
                .HasForeignKey<Cliente>(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CLIENTE_USUARIO");
        });

        modelBuilder.Entity<Ejercicio>(entity =>
        {
            entity.ToTable("EJERCICIO");

            entity.Property(e => e.EjercicioId).HasColumnName("ejercicio_id");
            entity.Property(e => e.Descripcion).HasColumnName("descripcion");
            entity.Property(e => e.GrupoMuscular)
                .HasMaxLength(100)
                .HasColumnName("grupo_muscular");
            entity.Property(e => e.Nombre)
                .HasMaxLength(150)
                .HasColumnName("nombre");
        });

        modelBuilder.Entity<Entrenador>(entity =>
        {
            entity.HasKey(e => e.UsuarioId);

            entity.ToTable("ENTRENADOR");

            entity.Property(e => e.UsuarioId)
                .ValueGeneratedNever()
                .HasColumnName("usuario_id");
            entity.Property(e => e.Especialidad)
                .HasMaxLength(150)
                .HasColumnName("especialidad");

            entity.HasOne(d => d.Usuario).WithOne(p => p.Entrenador)
                .HasForeignKey<Entrenador>(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ENTRENADOR_USUARIO");
        });

        modelBuilder.Entity<EvaluacionFisica>(entity =>
        {
            entity.HasKey(e => e.EvaluacionId);

            entity.ToTable("EVALUACION_FISICA");

            entity.HasIndex(e => e.ClienteId, "IX_EVALUACION_CLIENTE");

            entity.Property(e => e.EvaluacionId).HasColumnName("evaluacion_id");
            entity.Property(e => e.ClienteId).HasColumnName("cliente_id");
            entity.Property(e => e.EntrenadorId).HasColumnName("entrenador_id");
            entity.Property(e => e.Estatura)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("estatura");
            entity.Property(e => e.Fecha)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("fecha");
            entity.Property(e => e.Medidas)
                .HasMaxLength(1000)
                .HasColumnName("medidas");
            entity.Property(e => e.Observaciones).HasColumnName("observaciones");
            entity.Property(e => e.Peso)
                .HasColumnType("decimal(6, 2)")
                .HasColumnName("peso");
            entity.Property(e => e.Tipo)
                .HasMaxLength(50)
                .HasColumnName("tipo");

            entity.HasOne(d => d.Cliente).WithMany(p => p.EvaluacionFisicas)
                .HasForeignKey(d => d.ClienteId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_EVALUACION_CLIENTE");

            entity.HasOne(d => d.Entrenador).WithMany(p => p.EvaluacionFisicas)
                .HasForeignKey(d => d.EntrenadorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_EVALUACION_ENTRENADOR");
        });

        modelBuilder.Entity<Evento>(entity =>
        {
            entity.ToTable("EVENTO");

            entity.Property(e => e.EventoId).HasColumnName("evento_id");
            entity.Property(e => e.AdministradorId).HasColumnName("administrador_id");
            entity.Property(e => e.CupoMaximo).HasColumnName("cupo_maximo");
            entity.Property(e => e.Descripcion).HasColumnName("descripcion");
            entity.Property(e => e.Estado)
                .HasMaxLength(30)
                .HasDefaultValue("ACTIVO")
                .HasColumnName("estado");
            entity.Property(e => e.Fecha).HasColumnName("fecha");
            entity.Property(e => e.Nombre)
                .HasMaxLength(150)
                .HasColumnName("nombre");

            entity.HasOne(d => d.Administrador).WithMany(p => p.Eventos)
                .HasForeignKey(d => d.AdministradorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_EVENTO_ADMINISTRADOR");
        });

        modelBuilder.Entity<HorarioTarea>(entity =>
        {
            entity.HasKey(e => e.HorarioId);

            entity.ToTable("HORARIO_TAREA");

            entity.Property(e => e.HorarioId).HasColumnName("horario_id");
            entity.Property(e => e.Estado)
                .HasMaxLength(30)
                .HasDefaultValue("PENDIENTE")
                .HasColumnName("estado");
            entity.Property(e => e.Fecha).HasColumnName("fecha");
            entity.Property(e => e.Tarea)
                .HasMaxLength(255)
                .HasColumnName("tarea");
            entity.Property(e => e.UsuarioId).HasColumnName("usuario_id");

            entity.HasOne(d => d.Usuario).WithMany(p => p.HorarioTareas)
                .HasForeignKey(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_HORARIO_TAREA_USUARIO");
        });

        modelBuilder.Entity<InscripcionEvento>(entity =>
        {
            entity.HasKey(e => new { e.EventoId, e.ClienteId });

            entity.ToTable("INSCRIPCION_EVENTO");

            entity.Property(e => e.EventoId).HasColumnName("evento_id");
            entity.Property(e => e.ClienteId).HasColumnName("cliente_id");
            entity.Property(e => e.Estado)
                .HasMaxLength(30)
                .HasDefaultValue("INSCRITO")
                .HasColumnName("estado");
            entity.Property(e => e.FechaInscripcion)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("fecha_inscripcion");

            entity.HasOne(d => d.Cliente).WithMany(p => p.InscripcionEventos)
                .HasForeignKey(d => d.ClienteId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_INSCRIPCION_CLIENTE");

            entity.HasOne(d => d.Evento).WithMany(p => p.InscripcionEventos)
                .HasForeignKey(d => d.EventoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_INSCRIPCION_EVENTO");
        });

        modelBuilder.Entity<Membresium>(entity =>
        {
            entity.HasKey(e => e.MembresiaId);

            entity.ToTable("MEMBRESIA");

            entity.HasIndex(e => e.ClienteId, "IX_MEMBRESIA_CLIENTE");

            entity.Property(e => e.MembresiaId).HasColumnName("membresia_id");
            entity.Property(e => e.ClienteId).HasColumnName("cliente_id");
            entity.Property(e => e.Estado)
                .HasMaxLength(30)
                .HasDefaultValue("ACTIVA")
                .HasColumnName("estado");
            entity.Property(e => e.FechaFin).HasColumnName("fecha_fin");
            entity.Property(e => e.FechaInicio).HasColumnName("fecha_inicio");
            entity.Property(e => e.PlanId).HasColumnName("plan_id");

            entity.HasOne(d => d.Cliente).WithMany(p => p.Membresia)
                .HasForeignKey(d => d.ClienteId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MEMBRESIA_CLIENTE");

            entity.HasOne(d => d.Plan).WithMany(p => p.Membresia)
                .HasForeignKey(d => d.PlanId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MEMBRESIA_PLAN");
        });

        modelBuilder.Entity<Metum>(entity =>
        {
            entity.HasKey(e => e.MetaId);

            entity.ToTable("META");

            entity.Property(e => e.MetaId).HasColumnName("meta_id");
            entity.Property(e => e.ClienteId).HasColumnName("cliente_id");
            entity.Property(e => e.Estado)
                .HasMaxLength(30)
                .HasDefaultValue("ACTIVA")
                .HasColumnName("estado");
            entity.Property(e => e.FechaInicio).HasColumnName("fecha_inicio");
            entity.Property(e => e.FechaLimite).HasColumnName("fecha_limite");
            entity.Property(e => e.Tipo)
                .HasMaxLength(100)
                .HasColumnName("tipo");
            entity.Property(e => e.ValorObjetivo)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("valor_objetivo");

            entity.HasOne(d => d.Cliente).WithMany(p => p.Meta)
                .HasForeignKey(d => d.ClienteId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_META_CLIENTE");
        });

        modelBuilder.Entity<MultimediaEjercicio>(entity =>
        {
            entity.HasKey(e => e.MultimediaId);

            entity.ToTable("MULTIMEDIA_EJERCICIO");

            entity.Property(e => e.MultimediaId).HasColumnName("multimedia_id");
            entity.Property(e => e.EjercicioId).HasColumnName("ejercicio_id");
            entity.Property(e => e.Tipo)
                .HasMaxLength(30)
                .HasColumnName("tipo");
            entity.Property(e => e.Url)
                .HasMaxLength(500)
                .HasColumnName("url");

            entity.HasOne(d => d.Ejercicio).WithMany(p => p.MultimediaEjercicios)
                .HasForeignKey(d => d.EjercicioId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MULTIMEDIA_EJERCICIO");
        });

        modelBuilder.Entity<Notificacion>(entity =>
        {
            entity.ToTable("NOTIFICACION");

            entity.HasIndex(e => e.UsuarioId, "IX_NOTIFICACION_USUARIO");

            entity.Property(e => e.NotificacionId).HasColumnName("notificacion_id");
            entity.Property(e => e.Estado)
                .HasMaxLength(30)
                .HasDefaultValue("NO_LEIDA")
                .HasColumnName("estado");
            entity.Property(e => e.Fecha)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("fecha");
            entity.Property(e => e.Mensaje)
                .HasMaxLength(500)
                .HasColumnName("mensaje");
            entity.Property(e => e.Tipo)
                .HasMaxLength(50)
                .HasColumnName("tipo");
            entity.Property(e => e.UsuarioId).HasColumnName("usuario_id");

            entity.HasOne(d => d.Usuario).WithMany(p => p.Notificacions)
                .HasForeignKey(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_NOTIFICACION_USUARIO");
        });

        modelBuilder.Entity<ObservacionEntrenador>(entity =>
        {
            entity.HasKey(e => e.ObservacionId);

            entity.ToTable("OBSERVACION_ENTRENADOR");

            entity.Property(e => e.ObservacionId).HasColumnName("observacion_id");
            entity.Property(e => e.ClienteId).HasColumnName("cliente_id");
            entity.Property(e => e.Contenido).HasColumnName("contenido");
            entity.Property(e => e.EntrenadorId).HasColumnName("entrenador_id");
            entity.Property(e => e.Fecha)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("fecha");

            entity.HasOne(d => d.Cliente).WithMany(p => p.ObservacionEntrenadors)
                .HasForeignKey(d => d.ClienteId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_OBSERVACION_CLIENTE");

            entity.HasOne(d => d.Entrenador).WithMany(p => p.ObservacionEntrenadors)
                .HasForeignKey(d => d.EntrenadorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_OBSERVACION_ENTRENADOR");
        });

        modelBuilder.Entity<Pago>(entity =>
        {
            entity.ToTable("PAGO");

            entity.HasIndex(e => e.MembresiaId, "IX_PAGO_MEMBRESIA");

            entity.Property(e => e.PagoId).HasColumnName("pago_id");
            entity.Property(e => e.FechaPago)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("fecha_pago");
            entity.Property(e => e.MembresiaId).HasColumnName("membresia_id");
            entity.Property(e => e.MetodoPago)
                .HasMaxLength(30)
                .HasColumnName("metodo_pago");
            entity.Property(e => e.Monto)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("monto");
            entity.Property(e => e.RegistradoPor).HasColumnName("registrado_por");

            entity.HasOne(d => d.Membresia).WithMany(p => p.Pagos)
                .HasForeignKey(d => d.MembresiaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PAGO_MEMBRESIA");

            entity.HasOne(d => d.RegistradoPorNavigation).WithMany(p => p.Pagos)
                .HasForeignKey(d => d.RegistradoPor)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PAGO_USUARIO");
        });

        modelBuilder.Entity<ParametroRecomendacion>(entity =>
        {
            entity.HasKey(e => e.ParametroId);

            entity.ToTable("PARAMETRO_RECOMENDACION");

            entity.Property(e => e.ParametroId).HasColumnName("parametro_id");
            entity.Property(e => e.AdministradorId).HasColumnName("administrador_id");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .HasColumnName("nombre");
            entity.Property(e => e.Valor)
                .HasMaxLength(500)
                .HasColumnName("valor");

            entity.HasOne(d => d.Administrador).WithMany(p => p.ParametroRecomendacions)
                .HasForeignKey(d => d.AdministradorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PARAMETRO_ADMINISTRADOR");
        });

        modelBuilder.Entity<Permiso>(entity =>
        {
            entity.ToTable("PERMISO");

            entity.HasIndex(e => e.Nombre, "UQ_PERMISO_NOMBRE").IsUnique();

            entity.Property(e => e.PermisoId).HasColumnName("permiso_id");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(255)
                .HasColumnName("descripcion");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .HasColumnName("nombre");
        });

        modelBuilder.Entity<PlanMembresium>(entity =>
        {
            entity.HasKey(e => e.PlanId);

            entity.ToTable("PLAN_MEMBRESIA");

            entity.Property(e => e.PlanId).HasColumnName("plan_id");
            entity.Property(e => e.DuracionMeses).HasColumnName("duracion_meses");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .HasColumnName("nombre");
            entity.Property(e => e.Precio)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("precio");
        });

        modelBuilder.Entity<PrediccionDesercion>(entity =>
        {
            entity.HasKey(e => e.PrediccionId);

            entity.ToTable("PREDICCION_DESERCION");

            entity.HasIndex(e => e.ClienteId, "IX_PREDICCION_CLIENTE");

            entity.Property(e => e.PrediccionId).HasColumnName("prediccion_id");
            entity.Property(e => e.ClienteId).HasColumnName("cliente_id");
            entity.Property(e => e.Confiabilidad)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("confiabilidad");
            entity.Property(e => e.Fecha)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("fecha");
            entity.Property(e => e.NivelRiesgo)
                .HasMaxLength(30)
                .HasColumnName("nivel_riesgo");

            entity.HasOne(d => d.Cliente).WithMany(p => p.PrediccionDesercions)
                .HasForeignKey(d => d.ClienteId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PREDICCION_CLIENTE");
        });

        modelBuilder.Entity<Recomendacion>(entity =>
        {
            entity.ToTable("RECOMENDACION");

            entity.HasIndex(e => e.ClienteId, "IX_RECOMENDACION_CLIENTE");

            entity.Property(e => e.RecomendacionId).HasColumnName("recomendacion_id");
            entity.Property(e => e.ClienteId).HasColumnName("cliente_id");
            entity.Property(e => e.EntrenadorId).HasColumnName("entrenador_id");
            entity.Property(e => e.Estado)
                .HasMaxLength(30)
                .HasDefaultValue("PENDIENTE")
                .HasColumnName("estado");
            entity.Property(e => e.Fecha)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("fecha");
            entity.Property(e => e.RutinaSugeridaId).HasColumnName("rutina_sugerida_id");

            entity.HasOne(d => d.Cliente).WithMany(p => p.Recomendacions)
                .HasForeignKey(d => d.ClienteId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RECOMENDACION_CLIENTE");

            entity.HasOne(d => d.Entrenador).WithMany(p => p.Recomendacions)
                .HasForeignKey(d => d.EntrenadorId)
                .HasConstraintName("FK_RECOMENDACION_ENTRENADOR");

            entity.HasOne(d => d.RutinaSugerida).WithMany(p => p.Recomendacions)
                .HasForeignKey(d => d.RutinaSugeridaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RECOMENDACION_RUTINA");
        });

        modelBuilder.Entity<Reporte>(entity =>
        {
            entity.ToTable("REPORTE");

            entity.Property(e => e.ReporteId).HasColumnName("reporte_id");
            entity.Property(e => e.FechaGeneracion)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("fecha_generacion");
            entity.Property(e => e.FormatoExportado)
                .HasMaxLength(20)
                .HasColumnName("formato_exportado");
            entity.Property(e => e.GeneradoPor).HasColumnName("generado_por");
            entity.Property(e => e.PeriodoFin).HasColumnName("periodo_fin");
            entity.Property(e => e.PeriodoInicio).HasColumnName("periodo_inicio");
            entity.Property(e => e.Tipo)
                .HasMaxLength(100)
                .HasColumnName("tipo");

            entity.HasOne(d => d.GeneradoPorNavigation).WithMany(p => p.Reportes)
                .HasForeignKey(d => d.GeneradoPor)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_REPORTE_USUARIO");
        });

        modelBuilder.Entity<Rol>(entity =>
        {
            entity.ToTable("ROL");

            entity.HasIndex(e => e.Nombre, "UQ_ROL_NOMBRE").IsUnique();

            entity.Property(e => e.RolId).HasColumnName("rol_id");
            entity.Property(e => e.Nombre)
                .HasMaxLength(50)
                .HasColumnName("nombre");

            entity.HasMany(d => d.Permisos).WithMany(p => p.Rols)
                .UsingEntity<Dictionary<string, object>>(
                    "RolPermiso",
                    r => r.HasOne<Permiso>().WithMany()
                        .HasForeignKey("PermisoId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_ROL_PERMISO_PERMISO"),
                    l => l.HasOne<Rol>().WithMany()
                        .HasForeignKey("RolId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_ROL_PERMISO_ROL"),
                    j =>
                    {
                        j.HasKey("RolId", "PermisoId");
                        j.ToTable("ROL_PERMISO");
                        j.IndexerProperty<int>("RolId").HasColumnName("rol_id");
                        j.IndexerProperty<int>("PermisoId").HasColumnName("permiso_id");
                    });
        });

        modelBuilder.Entity<Rutina>(entity =>
        {
            entity.ToTable("RUTINA");

            entity.HasIndex(e => e.EntrenadorId, "IX_RUTINA_ENTRENADOR");

            entity.Property(e => e.RutinaId).HasColumnName("rutina_id");
            entity.Property(e => e.Descripcion).HasColumnName("descripcion");
            entity.Property(e => e.EntrenadorId).HasColumnName("entrenador_id");
            entity.Property(e => e.Estado)
                .HasMaxLength(30)
                .HasDefaultValue("ACTIVA")
                .HasColumnName("estado");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.Nivel)
                .HasMaxLength(30)
                .HasColumnName("nivel");
            entity.Property(e => e.Nombre)
                .HasMaxLength(150)
                .HasColumnName("nombre");
            entity.Property(e => e.Objetivo)
                .HasMaxLength(100)
                .HasColumnName("objetivo");

            entity.HasOne(d => d.Entrenador).WithMany(p => p.Rutinas)
                .HasForeignKey(d => d.EntrenadorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RUTINA_ENTRENADOR");
        });

        modelBuilder.Entity<RutinaEjercicio>(entity =>
        {
            entity.HasKey(e => new { e.RutinaId, e.EjercicioId });

            entity.ToTable("RUTINA_EJERCICIO");

            entity.HasIndex(e => new { e.RutinaId, e.Orden }, "UQ_RUTINA_EJERCICIO_ORDEN").IsUnique();

            entity.Property(e => e.RutinaId).HasColumnName("rutina_id");
            entity.Property(e => e.EjercicioId).HasColumnName("ejercicio_id");
            entity.Property(e => e.DescansoSegundos)
                .HasDefaultValue(60)
                .HasColumnName("descanso_segundos");
            entity.Property(e => e.Orden).HasColumnName("orden");
            entity.Property(e => e.Repeticiones).HasColumnName("repeticiones");
            entity.Property(e => e.Series).HasColumnName("series");

            entity.HasOne(d => d.Ejercicio).WithMany(p => p.RutinaEjercicios)
                .HasForeignKey(d => d.EjercicioId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RUTINA_EJERCICIO_EJERCICIO");

            entity.HasOne(d => d.Rutina).WithMany(p => p.RutinaEjercicios)
                .HasForeignKey(d => d.RutinaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RUTINA_EJERCICIO_RUTINA");
        });

        modelBuilder.Entity<SesionEjercicio>(entity =>
        {
            entity.HasKey(e => new { e.SesionId, e.EjercicioId });

            entity.ToTable("SESION_EJERCICIO");

            entity.Property(e => e.SesionId).HasColumnName("sesion_id");
            entity.Property(e => e.EjercicioId).HasColumnName("ejercicio_id");
            entity.Property(e => e.Completado).HasColumnName("completado");
            entity.Property(e => e.FechaMarcado).HasColumnName("fecha_marcado");
            entity.Property(e => e.Observaciones)
                .HasMaxLength(500)
                .HasColumnName("observaciones");
            entity.Property(e => e.PesoUtilizado)
                .HasColumnType("decimal(8, 2)")
                .HasColumnName("peso_utilizado");
            entity.Property(e => e.RepeticionesRealizadas).HasColumnName("repeticiones_realizadas");
            entity.Property(e => e.SeriesCompletadas).HasColumnName("series_completadas");

            entity.HasOne(d => d.Ejercicio).WithMany(p => p.SesionEjercicios)
                .HasForeignKey(d => d.EjercicioId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SESION_EJERCICIO_EJERCICIO");

            entity.HasOne(d => d.Sesion).WithMany(p => p.SesionEjercicios)
                .HasForeignKey(d => d.SesionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SESION_EJERCICIO_SESION");
        });

        modelBuilder.Entity<SesionEntrenamiento>(entity =>
        {
            entity.HasKey(e => e.SesionId);

            entity.ToTable("SESION_ENTRENAMIENTO");

            entity.HasIndex(e => e.AsignacionId, "IX_SESION_ASIGNACION");

            entity.Property(e => e.SesionId).HasColumnName("sesion_id");
            entity.Property(e => e.AsignacionId).HasColumnName("asignacion_id");
            entity.Property(e => e.Estado)
                .HasMaxLength(30)
                .HasDefaultValue("EN_PROGRESO")
                .HasColumnName("estado");
            entity.Property(e => e.Fecha)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("fecha");

            entity.HasOne(d => d.Asignacion).WithMany(p => p.SesionEntrenamientos)
                .HasForeignKey(d => d.AsignacionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SESION_ASIGNACION");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.ToTable("USUARIO");

            entity.HasIndex(e => e.RolId, "IX_USUARIO_ROL");

            entity.HasIndex(e => e.Correo, "UQ_USUARIO_CORREO").IsUnique();

            entity.Property(e => e.UsuarioId).HasColumnName("usuario_id");
            entity.Property(e => e.ContrasenaHash)
                .HasMaxLength(255)
                .HasColumnName("contrasena_hash");
            entity.Property(e => e.Correo)
                .HasMaxLength(150)
                .HasColumnName("correo");
            entity.Property(e => e.Estado)
                .HasMaxLength(20)
                .HasDefaultValue("ACTIVO")
                .HasColumnName("estado");
            entity.Property(e => e.FechaRegistro)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("fecha_registro");
            entity.Property(e => e.FotoPerfil)
                .HasMaxLength(500)
                .HasColumnName("foto_perfil");
            entity.Property(e => e.Nombre)
                .HasMaxLength(150)
                .HasColumnName("nombre");
            entity.Property(e => e.RolId).HasColumnName("rol_id");
            entity.Property(e => e.Telefono)
                .HasMaxLength(30)
                .HasColumnName("telefono");

            entity.HasOne(d => d.Rol).WithMany(p => p.Usuarios)
                .HasForeignKey(d => d.RolId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_USUARIO_ROL");
        });

        modelBuilder.Entity<ValidacionPrediccion>(entity =>
        {
            entity.HasKey(e => e.ValidacionId);

            entity.ToTable("VALIDACION_PREDICCION");

            entity.HasIndex(e => e.PrediccionId, "UQ_VALIDACION_PREDICCION").IsUnique();

            entity.Property(e => e.ValidacionId).HasColumnName("validacion_id");
            entity.Property(e => e.AdministradorId).HasColumnName("administrador_id");
            entity.Property(e => e.Fecha)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("fecha");
            entity.Property(e => e.PrediccionId).HasColumnName("prediccion_id");
            entity.Property(e => e.Resultado)
                .HasMaxLength(50)
                .HasColumnName("resultado");

            entity.HasOne(d => d.Administrador).WithMany(p => p.ValidacionPrediccions)
                .HasForeignKey(d => d.AdministradorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_VALIDACION_ADMINISTRADOR");

            entity.HasOne(d => d.Prediccion).WithOne(p => p.ValidacionPrediccion)
                .HasForeignKey<ValidacionPrediccion>(d => d.PrediccionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_VALIDACION_PREDICCION");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
