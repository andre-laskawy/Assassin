///-----------------------------------------------------------------
///   File:     AssassinImageDataContext.cs
///   Author:   Andre Laskawy           
///   Date:	    30.03.2018 17:20:38
///   Revision History: 
///   Name:  Andre Laskawy         Date:  30.03.2018 17:20:38      Description: Created
///-----------------------------------------------------------------

namespace Assassin.Data.Context
{
    using Assassin.Common.Models;
    using Assassin.Data.Base;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.ChangeTracking;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Defines the <see cref="AssassinImageDataContext" />
    /// </summary>
    public class AssassinImageDataContext : BaseContext
    {
        /// <summary>
        /// Defines the GlobalDatabaseName
        /// </summary>
        public const string GlobalDatabaseName = "AssassinImageDatabase.db3";

        /// <summary>
        /// Initializes a new instance of the <see cref="AssassinImageDataContext"/> class.
        /// </summary>
        public AssassinImageDataContext() : this(new DbContextOptionsBuilder<AssassinImageDataContext>().UseSqlite($"Filename={DefaultDatabasePath}").Options)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserDataContext"/> class.
        /// </summary>
        /// <param name="options">The <see cref="DbContextOptions{QuantImageDataContext}"/></param>
        public AssassinImageDataContext(DbContextOptions<AssassinImageDataContext> options) : base(options)
        {
            ChangeTracker.AutoDetectChangesEnabled = false;
            ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }

        /// <inheritdoc />
        public static string DefaultDatabasePath
        {
            get
            {
                var dir = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                return Path.Combine(dir, GlobalDatabaseName);
            }
        }

        /// <inheritdoc />
        public static string DefaultDatabaseRootPath
        {
            get
            {
                return Path.GetDirectoryName(DefaultDatabasePath);
            }
        }

        /// <inheritdoc />
        public override string DatabaseName { get => GlobalDatabaseName; }

        /// <inheritdoc />
        public override void Ensure()
        {
            using (var ctx = new AssassinImageDataContext())
            {
                ctx.Database.Migrate();
            }
        }

        /// <inheritdoc />
        public List<AssassinImage> IncludeAll(IQueryable<AssassinImage> entities, string query = null)
        {
            if (query != null)
            {
                return entities.AsNoTracking().FromSql("SELECT * FROM ASSASSINIMAGE WHERE " + query).ToList() as List<AssassinImage>;
            }
            else
            {
                return entities.AsNoTracking().ToList();
            }
        }

        /// <summary>
        /// The OnModelCreating
        /// </summary>
        /// <param name="modelBuilder">The <see cref="ModelBuilder"/></param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AssassinImage>(entity =>
            {
                entity.HasIndex(p => p.Id).IsUnique();
                entity.HasIndex(p => p.RelatedTypeDescription);
                entity.HasIndex(p => p.ModifiedDT);
                entity.HasIndex(p => p.CreatedDT);
                entity.HasKey(p => p.Id);
            });
        }

        /// <summary>
        /// Asynchronously saves all changes made in this context to the database.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="T:System.Threading.CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns>
        /// A task that represents the asynchronous save operation. The task result contains the
        /// number of state entries written to the database.
        /// </returns>
        /// <remarks>
        /// <para>
        /// This method will automatically call <see cref="M:Microsoft.EntityFrameworkCore.ChangeTracking.ChangeTracker.DetectChanges" /> to discover any
        /// changes to entity instances before saving to the underlying database. This can be disabled via
        /// <see cref="P:Microsoft.EntityFrameworkCore.ChangeTracking.ChangeTracker.AutoDetectChangesEnabled" />.
        /// </para>
        /// <para>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </para>
        /// </remarks>
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var entries = ChangeTracker.Entries();

            foreach (var entry in entries)
            {
                // Indicate whether a modified entity already exists
                if (entry.State == EntityState.Modified && entry.GetDatabaseValues() == null)
                {
                    entry.State = EntityState.Added;
                }
                else if (entry.State == EntityState.Added && entry.GetDatabaseValues() != null)
                {
                    entry.State = EntityState.Unchanged;
                    continue;
                }
            }

            return base.SaveChanges();
        }
    }
}
