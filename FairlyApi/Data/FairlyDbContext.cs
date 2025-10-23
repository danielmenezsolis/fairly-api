using FairlyApi.Models;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace FairlyApi.Data
{
    public class FairlyDbContext : DbContext
    {
        public FairlyDbContext(DbContextOptions<FairlyDbContext> options) : base (options)
        { 
        }

        public DbSet<Users> Users { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<GroupMember> GroupMembers { get; set; }
        public DbSet<Expense> Expenses { get; set; }
        public DbSet<ExpenseParticipant> ExpensesParticipant { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.HasDefaultSchema("fairly");

            modelBuilder.Entity<GroupMember>().HasKey(gm => new { gm.UserId, gm.GroupId });

            modelBuilder.Entity<ExpenseParticipant>().HasKey(ep => new { ep.ExpenseId, ep.UserId });

            // Configurar relaciones para User
            modelBuilder.Entity<Users>()
                .HasMany(u => u.CreatedGroups)
                .WithOne(g => g.Creator)
                .HasForeignKey(g => g.CreatorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Users>()
                .HasMany(u => u.GroupMembership)
                .WithOne(gm => gm.User)
                .HasForeignKey(gm => gm.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Users>()
                .HasMany(u => u.PaidExpenses)
                .WithOne(e => e.Payer)
                .HasForeignKey(e => e.PayerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Users>()
                .HasMany(u => u.ExpenseParticipations)
                .WithOne(ep => ep.User)
                .HasForeignKey(ep => ep.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            // Configurar relaciones para Group
            modelBuilder.Entity<Group>()
                .HasMany(g => g.Members)
                .WithOne(gm => gm.Group)
                .HasForeignKey(gm => gm.GroupId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Group>()
                .HasMany(g => g.Expenses)
                .WithOne(e => e.Group)
                .HasForeignKey(e => e.GroupId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configurar relaciones para Expense
            modelBuilder.Entity<Expense>()
                .HasMany(e => e.Participants)
                .WithOne(ep => ep.Expense)
                .HasForeignKey(ep => ep.ExpenseId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configurar índices únicos
            modelBuilder.Entity<Users>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // Configurar precision para decimales
            modelBuilder.Entity<Expense>()
                .Property(e => e.TotalAmount)
                .HasPrecision(10, 2);

            modelBuilder.Entity<ExpenseParticipant>()
                .Property(ep => ep.AmountOwed)
                .HasPrecision(10, 2);
        }

    }
}
