using AcxiomCRM.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
namespace AcxiomCRM.Data;
public class AppDbContext(DbContextOptions<AppDbContext> options):IdentityDbContext<AppUser>(options){public DbSet<Customer> Customers=>Set<Customer>();public DbSet<Lead> Leads=>Set<Lead>();public DbSet<Opportunity> Opportunities=>Set<Opportunity>();public DbSet<FollowUp> FollowUps=>Set<FollowUp>();public DbSet<Activity> Activities=>Set<Activity>();public DbSet<AuditLog> AuditLogs=>Set<AuditLog>();public override int SaveChanges(){ProtectAudit();return base.SaveChanges();}
public override Task<int> SaveChangesAsync(CancellationToken cancellationToken=default){ProtectAudit();return base.SaveChangesAsync(cancellationToken);}
private void ProtectAudit(){if(ChangeTracker.Entries<AuditLog>().Any(x=>x.State==EntityState.Modified||x.State==EntityState.Deleted))throw new InvalidOperationException("Audit records are append-only.");}
protected override void OnModelCreating(ModelBuilder b){base.OnModelCreating(b);b.Entity<Opportunity>().HasOne<Customer>().WithMany().HasForeignKey(x=>x.CustomerId).OnDelete(DeleteBehavior.Restrict);
b.Entity<Opportunity>().HasOne<Lead>().WithMany().HasForeignKey(x=>x.LeadId).OnDelete(DeleteBehavior.Restrict);
b.Entity<FollowUp>().HasOne<Customer>().WithMany().HasForeignKey(x=>x.CustomerId).OnDelete(DeleteBehavior.Restrict);
b.Entity<FollowUp>().HasOne<Lead>().WithMany().HasForeignKey(x=>x.LeadId).OnDelete(DeleteBehavior.Restrict);
b.Entity<Activity>().HasOne<Customer>().WithMany().HasForeignKey(x=>x.CustomerId).OnDelete(DeleteBehavior.Restrict);
b.Entity<Activity>().HasOne<Lead>().WithMany().HasForeignKey(x=>x.LeadId).OnDelete(DeleteBehavior.Restrict);
b.Entity<Lead>().HasIndex(x=>new{x.OwnerId,x.Status});
b.Entity<Opportunity>().HasIndex(x=>new{x.OwnerId,x.Stage});
b.Entity<FollowUp>().HasIndex(x=>new{x.OwnerId,x.FollowUpDate});
b.Entity<Activity>().HasIndex(x=>new{x.OwnerId,x.ActivityDate});
b.Entity<Customer>().HasIndex(x=>x.Email).IsUnique();b.Entity<Opportunity>().HasOne<Customer>().WithMany().HasForeignKey(x=>x.CustomerId).OnDelete(DeleteBehavior.Restrict);
b.Entity<Opportunity>().HasOne<Lead>().WithMany().HasForeignKey(x=>x.LeadId).OnDelete(DeleteBehavior.Restrict);
b.Entity<FollowUp>().HasOne<Customer>().WithMany().HasForeignKey(x=>x.CustomerId).OnDelete(DeleteBehavior.Restrict);
b.Entity<FollowUp>().HasOne<Lead>().WithMany().HasForeignKey(x=>x.LeadId).OnDelete(DeleteBehavior.Restrict);
b.Entity<Activity>().HasOne<Customer>().WithMany().HasForeignKey(x=>x.CustomerId).OnDelete(DeleteBehavior.Restrict);
b.Entity<Activity>().HasOne<Lead>().WithMany().HasForeignKey(x=>x.LeadId).OnDelete(DeleteBehavior.Restrict);
b.Entity<Lead>().HasIndex(x=>new{x.OwnerId,x.Status});
b.Entity<Opportunity>().HasIndex(x=>new{x.OwnerId,x.Stage});
b.Entity<FollowUp>().HasIndex(x=>new{x.OwnerId,x.FollowUpDate});
b.Entity<Activity>().HasIndex(x=>new{x.OwnerId,x.ActivityDate});
b.Entity<Customer>().HasIndex(x=>x.Phone).IsUnique();b.Entity<Opportunity>().Property(x=>x.Amount).HasPrecision(18,2);b.Entity<Lead>().Property(x=>x.ExpectedValue).HasPrecision(18,2);}}