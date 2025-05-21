using Microsoft.EntityFrameworkCore;
using condominio_API.Models;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;


namespace condominio_API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<AcessoEntradaMorador> AcessoEntradaMoradores { get; set; }
        public DbSet<AcessoEntradaVisitante> AcessoEntradaVisitantes { get; set; }
        public DbSet<Apartamento> Apartamentos { get; set; }
        public DbSet<Notificacao> Notificacoes { get; set; }
        public DbSet<QRCodeTemp> QRCodesTemp { get; set; }
        public DbSet<Visitante> Visitantes { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AcessoEntradaMorador>()
                .HasOne(a => a.Usuario)
                .WithMany()
                .HasForeignKey(a => a.UsuarioId);

            modelBuilder.Entity<AcessoEntradaVisitante>()
                .HasOne(a => a.Visitante)
                .WithMany()
                .HasForeignKey(a => a.VisitanteId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<AcessoEntradaVisitante>()
                .HasOne(a => a.Usuario)
                .WithMany()
                .HasForeignKey(a => a.UsuarioId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Notificacao>()
                .HasOne(n => n.MoradorOrigem)
                .WithMany()
                .HasForeignKey(n => n.MoradorOrigemId);

            modelBuilder.Entity<Notificacao>()
                .HasOne(n => n.ApartamentoDestino)
                .WithMany()
                .HasForeignKey(n => n.ApartamentoDestinoId);

            modelBuilder.Entity<QRCodeTemp>()
                .HasOne(q => q.Morador)
                .WithMany()
                .HasForeignKey(q => q.MoradorId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<QRCodeTemp>()
                .HasOne(q => q.Visitante)
                .WithMany()
                .HasForeignKey(q => q.VisitanteId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Usuario>()
                .HasOne(u => u.Apartamento)
                .WithMany()
                .HasForeignKey(u => u.ApartamentoId);

            base.OnModelCreating(modelBuilder);
        }
    }
}