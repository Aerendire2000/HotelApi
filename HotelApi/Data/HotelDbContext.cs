using Microsoft.EntityFrameworkCore;
using HotelApi.Models;

namespace HotelApi.Data
{
    public class HotelDbContext : DbContext
    {
        public HotelDbContext(DbContextOptions<HotelDbContext> options) : base(options) { }

        public DbSet<Hotel> Hoteles { get; set; }
        public DbSet<Habitacion> Habitaciones { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Reserva> Reservas { get; set; }
        public DbSet<ReservaDetalle> ReservaDetalles { get; set; }
        public DbSet<ServicioAdicional> ServiciosAdicionales { get; set; }
        public DbSet<ReservaServicio> ReservaServicios { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Hotel>(e =>
            {
                e.ToTable(t => t.HasCheckConstraint("CK_Hotel_Estrellas",
                    "Estrellas BETWEEN 1 AND 5"));
            });

            modelBuilder.Entity<Habitacion>(e =>
            {
                e.HasOne(h => h.Hotel)
                 .WithMany(ho => ho.Habitaciones)
                 .HasForeignKey(h => h.HotelId)
                 .OnDelete(DeleteBehavior.Restrict);

                e.Property(h => h.PrecioPorNoche).HasPrecision(10, 2);
                e.HasIndex(h => new { h.HotelId, h.Numero }).IsUnique();
                e.ToTable(t =>
                {
                    t.HasCheckConstraint("CK_Habitacion_Precio", "PrecioPorNoche > 0");
                    t.HasCheckConstraint("CK_Habitacion_Tipo",
                        "Tipo IN ('Simple','Doble','Suite')");
                });
            });

            modelBuilder.Entity<Usuario>(e =>
            {
                e.HasIndex(u => u.Email).IsUnique();
                e.Property(u => u.FechaRegistro).HasDefaultValueSql("GETDATE()");
            });

            modelBuilder.Entity<Reserva>(e =>
            {
                e.HasOne(r => r.Usuario)
                 .WithMany(u => u.Reservas)
                 .HasForeignKey(r => r.UsuarioId)
                 .OnDelete(DeleteBehavior.Restrict);

                e.Property(r => r.Total).HasPrecision(10, 2);
                e.Property(r => r.Estado).HasDefaultValue("Pendiente");
                e.Property(r => r.FechaCreacion).HasDefaultValueSql("GETDATE()");
                e.ToTable(t =>
                {
                    t.HasCheckConstraint("CK_Reserva_Fechas", "FechaSalida > FechaEntrada");
                    t.HasCheckConstraint("CK_Reserva_Estado",
                        "Estado IN ('Pendiente','Confirmada','Cancelada')");
                });
            });

            modelBuilder.Entity<ReservaDetalle>(e =>
            {
                e.HasOne(d => d.Reserva)
                 .WithMany(r => r.Detalles)
                 .HasForeignKey(d => d.ReservaId)
                 .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(d => d.Habitacion)
                 .WithMany()
                 .HasForeignKey(d => d.HabitacionId)
                 .OnDelete(DeleteBehavior.Restrict);

                e.Property(d => d.PrecioPorNoche).HasPrecision(10, 2);
                e.Property(d => d.Subtotal).HasPrecision(10, 2);
                e.HasIndex(d => new { d.ReservaId, d.HabitacionId }).IsUnique();
            });

            modelBuilder.Entity<ServicioAdicional>(e =>
            {
                e.Property(s => s.Precio).HasPrecision(10, 2);
            });

            modelBuilder.Entity<ReservaServicio>(e =>
            {
                e.HasOne(rs => rs.Reserva)
                 .WithMany(r => r.Servicios)
                 .HasForeignKey(rs => rs.ReservaId)
                 .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(rs => rs.ServicioAdicional)
                 .WithMany()
                 .HasForeignKey(rs => rs.ServicioAdicionalId)
                 .OnDelete(DeleteBehavior.Restrict);

                e.Property(rs => rs.PrecioUnitario).HasPrecision(10, 2);
                e.Property(rs => rs.Subtotal).HasPrecision(10, 2);
                e.HasIndex(rs => new { rs.ReservaId, rs.ServicioAdicionalId }).IsUnique();
            });

            modelBuilder.Entity<Hotel>().HasData(
                new Hotel { Id = 1, Nombre = "Hotel Misti Plaza", Ciudad = "Arequipa",
                    Direccion = "Calle Mercaderes 210", Estrellas = 4 },
                new Hotel { Id = 2, Nombre = "Hotel Costa Azul", Ciudad = "Lima",
                    Direccion = "Av. Malecón 455", Estrellas = 5 },
                new Hotel { Id = 3, Nombre = "Hostal Valle Inca", Ciudad = "Cusco",
                    Direccion = "Jr. Saphi 88", Estrellas = 3 });

            modelBuilder.Entity<Habitacion>().HasData(
                Hab(1, 1, "101", "Simple", 120, 1),
                Hab(2, 1, "102", "Doble", 180, 2),
                Hab(3, 1, "301", "Suite", 350, 4),
                Hab(4, 2, "201", "Doble", 260, 2),
                Hab(5, 2, "202", "Suite", 520, 4),
                Hab(6, 3, "11", "Simple", 90, 1),
                Hab(7, 3, "12", "Doble", 140, 2));

            modelBuilder.Entity<Usuario>().HasData(
                new Usuario { Id = 1, NombreCompleto = "Ana Pérez Gómez",
                    Email = "ana.perez@correo.com", Telefono = "987654321",
                    FechaRegistro = new DateTime(2026, 10, 1) },
                new Usuario { Id = 2, NombreCompleto = "Carlos Ruiz Torres",
                    Email = "carlos.ruiz@correo.com", Telefono = "912345678",
                    FechaRegistro = new DateTime(2026, 10, 5) },
                new Usuario { Id = 3, NombreCompleto = "Lucía Fernández Silva",
                    Email = "lucia.fs@correo.com",
                    FechaRegistro = new DateTime(2026, 10, 9) });

            modelBuilder.Entity<ServicioAdicional>().HasData(
                new ServicioAdicional { Id = 1, Nombre = "Desayuno buffet",
                    Descripcion = "Por persona y día", Precio = 35 },
                new ServicioAdicional { Id = 2, Nombre = "Traslado aeropuerto",
                    Descripcion = "Ida o vuelta", Precio = 60 },
                new ServicioAdicional { Id = 3, Nombre = "Spa",
                    Descripcion = "Sesión de 1 hora", Precio = 120 },
                new ServicioAdicional { Id = 4, Nombre = "Estacionamiento",
                    Descripcion = "Por día", Precio = 20 });
        }

        private static Habitacion Hab(int id, int hotelId, string numero, string tipo,
            decimal precio, int capacidad) => new Habitacion
        {
            Id = id, HotelId = hotelId, Numero = numero, Tipo = tipo,
            PrecioPorNoche = precio, Capacidad = capacidad
        };
    }
}
