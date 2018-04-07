﻿using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Fan.Data
{
    /// <summary>
    /// EF implementation of a base repository for commonly used data access methods.
    /// </summary>
    /// <typeparam name="T">
    /// Ideally T should be a sub type of <see cref="Entity"/> which provides an int based PK,
    /// however since UserToken in Fan.Accounts derives from IdentityUserToken which has no int
    /// PK I cannot have T derive from <see cref="Entity"/>, this is unfortunate.
    /// </typeparam>
    /// <remarks>
    /// The sub class will have its specific methods, for example SqlCategoryRepository and SqlTagRepository
    /// has a GetListAsync() which does their specific join.
    /// </remarks>
    public class EntityRepository<T> : IRepository<T> where T : class // Entity
    {
        /// <summary>
        /// The set initialized by sub class.
        /// </summary>
        protected readonly DbSet<T> _entities;
        /// <summary>
        /// The specific context initialized by sub class.
        /// </summary>
        private readonly DbContext _db;

        public EntityRepository(DbContext context) 
        {
            _entities = context.Set<T>();
            _db = context;
        }

        /// <summary>
        /// Creates an entity and the returned object is tracked.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        /// <exception cref="DbUpdateException">
        /// If table has unique key constrain and the record being added violates it, 
        /// this exception will throw, such as <see cref="Meta"/> table.
        /// </exception>
        public virtual async Task<T> CreateAsync(T entity)
        {
            await _entities.AddAsync(entity);
            await _db.SaveChangesAsync();
            return entity;
        }

        /// <summary>
        /// Creates a list of entities.
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        public virtual async Task<IEnumerable<T>> CreateRangeAsync(IEnumerable<T> entities)
        {
            _entities.AddRange(entities);
            await _db.SaveChangesAsync();
            return entities;
        }

        /// <summary>
        /// Returns a list of objects of T based on search predicate.
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        /// <remarks>
        /// Suitable when predicate is very simple and short.  If you take a look at 
        /// SqlTagRepository GetListAsync() that is not suitable for this.
        /// </remarks>
        public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate) 
            => await _entities.Where(predicate).ToListAsync();

        /// <summary>
        /// Updates an entity.
        /// </summary>
        /// <param name="entity">
        /// The entity to be updated, the EF implementation does not use this parameter.
        /// </param>
        /// <returns></returns>
        public virtual async Task UpdateAsync(T entity)
        {
            await _db.SaveChangesAsync();
        }

        /// <summary>
        /// Updates a list of entities.
        /// </summary>
        /// <param name="entities">
        /// The entities to be updated, the EF implementation does not use this parameter.
        /// </param>
        /// <returns></returns>
        public virtual async Task UpdateAsync(IEnumerable<T> entities)
        {
            await _db.SaveChangesAsync();
        }
    }
}
