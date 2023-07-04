using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Umbrella.Api.Entities;
using Umbrella.Api.Entities.Enums;

#pragma warning disable CS8618

namespace Umbrella.Api.Contexts;

public class RepositoryContext : DbContext
{
    public DbSet<Client> Clients { get; set; }
    public DbSet<Insurer> Insurers { get; set; }
    public DbSet<Enrollment> Enrollments { get; set; }

    public DbSet<Taker> Takers { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<ScoreRequest> ScoreRequests { get; set; }
    public DbSet<Issue> InsuranceIssued { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder builder)
    { 
          builder.EnableSensitiveDataLogging();
        //builder.UseSqlite(@"Data Source=C:\Temp\DemoV4.db");
        builder.UseSqlServer(@"Server=tcp:gcb-sql-server.database.windows.net,1433;Initial Catalog=gcb-portal-prod;Persist Security Info=False;User ID=gcb-su;Password=#Su123456;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");
        //builder.UseSqlServer(@"Server=tcp:gcb-sql-server.database.windows.net,1433;Initial Catalog=gcb-portal-dev;Persist Security Info=False;User ID=gcb-su;Password=#Su123456;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<User>(entity =>
                             {
                                 entity.HasKey(x => x.Id);
                                 entity.Ignore(x => x.Authorities);
                                 entity.HasIndex(x => x.Username)
                                       .IsUnique();
                                 entity.Property(x => x.Password)
                                       .HasColumnName("passwordHash");
                                 entity.Property<string>("_authorities")
                                       .HasColumnName("authorities");
                             });

        builder.Entity<Client>(entity =>
        {
              entity.HasKey(x => x.Id).IsClustered();
              entity.HasIndex(x => x.Cnpj).IsUnique();
              entity.Property(x => x.Segment).HasConversion(new EnumToStringConverter<Segment>()); 
        });

        builder.Entity<ProposalParameters>(entity =>
                                           {
                                               entity.Property(x => x.InsurerId)
                                                     .HasColumnName("insurerFK");
                                               entity.HasKey(x => new {x.InsurerId, x.ProposalType});
                                               entity.Property(x => x.ProposalType)
                                                     .HasConversion(new EnumToStringConverter<ProposalType>());
                                           });

        builder.Entity<Insurer>(entity =>
                                {
                                    entity.HasMany(x => x.ProposalParameters)
                                          .WithOne(x => x.Insurer)
                                          .HasForeignKey(x => x.InsurerId)
                                          .OnDelete(DeleteBehavior.Cascade);

                                    entity.HasIndex(x => x.Cnpj)
                                          .IsUnique();
                                    entity.HasKey(x => x.Id);
                                });

        builder.Entity<Taker>(entity =>
                              {
                                  entity.Property(x => x.Category)
                                        .HasConversion(new EnumToStringConverter<Category>());
                              });

        builder.Entity<Issue>(entity =>
                              {
                                  entity.HasKey(x => x.Id);
                                  entity.HasIndex(x => x.DealId)
                                        .IsUnique();
                                  entity.Property(x => x.Product)
                                        .HasConversion(new EnumToStringConverter<Product>());
                                  entity.Property(x => x.Reason)
                                        .HasConversion(new EnumToStringConverter<Reason>());

                                  entity.HasOne(x => x.Client)
                                        .WithMany(x => x.Issued)
                                        .HasForeignKey("clientFK")
                                        .OnDelete(DeleteBehavior.SetNull);

                                  entity.HasOne(x => x.Insurer)
                                        .WithMany(x => x.Issued)
                                        .HasForeignKey("insurerFK")
                                        .OnDelete(DeleteBehavior.SetNull);

                                  entity.HasMany(c => c.Users)
                                        .WithMany(c => c.Issues)
                                        .UsingEntity<Dictionary<string, object>>(x =>
                                         {
                                             x.ToTable("Issue_User", "portal");
                                         });
                              });

        builder.Entity<ScoreRequest>(entity =>
                                     {
                                         entity.Property<int?>("scoreFK")
                                               .IsRequired(false);
                                     });

        builder.Entity<Score>(entity =>
                              {
                                  entity.HasOne(x => x.Request)
                                        .WithOne(x => x.Score)
                                        .HasForeignKey<ScoreRequest>("scoreFK")
                                        .OnDelete(DeleteBehavior.Cascade);
                              });

        builder.Entity<Enrollment>(entity =>
                                   {
                                       entity.Property<int>("clientPK");
                                       entity.Property<int>("insurerPK");
                                       entity.HasKey("clientPK", "insurerPK");

                                       entity.Property(x => x.Status)
                                             .HasConversion(new EnumToStringConverter<Status>());

                                       entity.HasOne(x => x.Client)
                                             .WithMany(x => x.Enrollments)
                                             .HasForeignKey("clientPK")
                                             .OnDelete(DeleteBehavior.Cascade);

                                       entity.HasOne(x => x.Insurer)
                                             .WithMany(x => x.Enrollments)
                                             .HasForeignKey("insurerPK")
                                             .OnDelete(DeleteBehavior.Cascade);

                                       entity.HasMany(x => x.Policyholders)
                                             .WithOne(x => x.Enrollment)
                                             .HasForeignKey("clientFK", "insurerFK")
                                             .OnDelete(DeleteBehavior.Cascade);
                                   });
    }
}