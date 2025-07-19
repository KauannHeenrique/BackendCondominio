using Microsoft.EntityFrameworkCore;
using condominio_API.Models;

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
        public DbSet<NotificacaoDestinatario> NotificacaoDestinatarios { get; set; } // ✅ Novo
        public DbSet<QRCodeTemp> QRCodesTemp { get; set; }
        public DbSet<Visitante> Visitantes { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<NotificacaoHistorico> NotificacaoHistoricos { get; set; }
        public DbSet<AtividadeView> AtividadesRecentes { get; set; }



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

            // ✅ Relacionamento Notificação -> Destinatários
            modelBuilder.Entity<Notificacao>()
                .HasMany(n => n.Destinatarios)
                .WithOne(d => d.Notificacao)
                .HasForeignKey(d => d.NotificacaoId)
                .OnDelete(DeleteBehavior.Cascade);

            // ✅ Relacionamento Destinatário -> Usuário
            modelBuilder.Entity<NotificacaoDestinatario>()
                .HasOne(d => d.UsuarioDestino)
                .WithMany()
                .HasForeignKey(d => d.UsuarioDestinoId)
                .OnDelete(DeleteBehavior.Restrict);

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
                .HasForeignKey(u => u.ApartamentoId)
                .IsRequired(false);  // falso para poder cadastrar funcionarios com idapartamento vazio

            modelBuilder.Entity<Notificacao>()
                .HasMany(n => n.Historico)
                .WithOne(h => h.Notificacao)
                .HasForeignKey(h => h.NotificacaoId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<NotificacaoHistorico>()
                .HasOne(h => h.Usuario)
                .WithMany()
                .HasForeignKey(h => h.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AtividadeView>()
                .HasNoKey() // Views não têm chave primária
                .ToView("vw_atividades_recentes"); // Nome exato da sua VIEW no MySQL

            base.OnModelCreating(modelBuilder);
        }
    }
}
