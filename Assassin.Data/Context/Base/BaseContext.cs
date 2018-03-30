//-----------------------------------------------------------------------
// <copyright file="BaseContext.cs" company="Miltenyi Biotec">
// Copyright (c) Miltenyi Biotec. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Assassin.Data.Base
{
    using Microsoft.EntityFrameworkCore;
    using System.Linq;

    /// <summary>
    /// Defines the <see cref="BaseContext" />
    /// </summary>
    public abstract class BaseContext : DbContext, IContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BaseContext"/> class.
        /// </summary>
        public BaseContext()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseContext"/> class.
        /// </summary>
        public BaseContext(DbContextOptions options) : base(options)
        {
        }

        /// <summary>
        /// Detaches all entities.
        /// </summary>
        public void DetachAllEntities()
        {
            var changedEntriesCopy = this.ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added ||
                            e.State == EntityState.Modified ||
                            e.State == EntityState.Deleted)
                .ToList();
            foreach (var entity in changedEntriesCopy)
            {
                this.Entry(entity.Entity).State = EntityState.Detached;
            }
        }

        /// <inheritdoc />
        public abstract string DatabaseName { get; }

        /// <inheritdoc />
        public abstract void Ensure();
    }
}
