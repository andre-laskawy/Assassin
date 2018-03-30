///-----------------------------------------------------------------
///   File:     ImageRepository.cs
///   Author:   Andre Laskawy           
///   Date:	    30.03.2018 17:15:06
///   Revision History: 
///   Name:  Andre Laskawy         Date:  30.03.2018 17:15:06      Description: Created
///-----------------------------------------------------------------

namespace Assassin.Data.Repository
{
    using Assassin.Common.Models;
    using Assassin.Data.Context;
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;

    /// <summary>
    /// Defines the <see cref="ImageRepository" />
    /// </summary>
    public class ImageRepository
    {
        /// <summary>
        /// The Create
        /// </summary>
        /// <param name="data">The <see cref="AssassinImage"/></param>
        /// <returns>The <see cref="Task{AssassinImage}"/></returns>
        internal async Task<AssassinImage> Create(AssassinImage data)
        {
            try
            {
                using (var ctx = NewContext())
                {
                    ctx.Set<AssassinImage>().AddRange(data);
                    await ctx.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Could not create entity.", ex);
            }

            return data;
        }

        /// <summary>
        /// The DeleteRange
        /// </summary>
        /// <param name="data">The <see cref="List{AssassinImage}"/></param>
        /// <returns>The <see cref="Task"/></returns>
        internal async Task DeleteRange(List<AssassinImage> data)
        {
            try
            {
                foreach (var item in data as List<AssassinImage>)
                {
                    await Delete((item).Id);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Could not delete entities.", ex);
            }
        }

        /// <summary>
        /// The Delete
        /// </summary>
        /// <param name="data">The <see cref="AssassinImage"/></param>
        /// <returns>The <see cref="Task"/></returns>
        internal async Task Delete(AssassinImage data)
        {
            try
            {
                await Delete(data.Id);
            }
            catch (Exception ex)
            {
                throw new Exception("Could not delete entity.", ex);
            }
        }

        /// <summary>
        /// The Delete
        /// </summary>
        /// <param name="id">The <see cref="Guid"/></param>
        /// <returns>The <see cref="Task"/></returns>
        internal async Task Delete(Guid id)
        {
            try
            {
                using (var ctx = NewContext())
                {
                    var entity = this.Get(id, true);
                    if (entity != null)
                    {
                        ctx.Set<AssassinImage>().Remove(entity);
                        await ctx.SaveChangesAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Could not delete entity.", ex);
            }
        }

        /// <summary>
        /// The Update
        /// </summary>
        /// <param name="data">The <see cref="object"/></param>
        /// <returns>The <see cref="Task"/></returns>
        internal async Task Update(object data)
        {
            try
            {
                using (var ctx = NewContext())
                {
                    List<AssassinImage> dataList = new List<AssassinImage>();
                    if (data.GetType().GenericTypeArguments != null
                        && data.GetType().GenericTypeArguments.Count() > 0)
                    {
                        dataList = data as List<AssassinImage>;
                    }
                    else
                    {
                        dataList.Add(data as AssassinImage);
                    }

                    ctx.DetachAllEntities();
                    ctx.Set<AssassinImage>().UpdateRange(dataList);
                    await ctx.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Could not update entity.", ex);
            }
        }

        /// <inheritdoc />
        internal IList<Guid> GetIdList(string type, string query, int maxItem)
        {
            try
            {
                using (var ctx = NewContext())
                {
                    IQueryable<AssassinImage> tempList = null;
                    IList<Guid> list = new List<Guid>();
                    if (query != null)
                    {
                        tempList = ctx.Set<AssassinImage>()
                            .FromSql("SELECT * FROM ASSASSINIMAGE WHERE " + query);
                    }

                    list = tempList
                        .OrderByDescending(p => p.CreatedDT)
                        .Select(p => p.Id)
                        .Take(maxItem)
                        .ToList();

                    ctx.DetachAllEntities();
                    return list;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Could not get entities.", ex);
            }
        }

        /// <inheritdoc />
        internal IList GetListByQuery(string type, string sql, bool includeAll)
        {
            try
            {
                using (var ctx = NewContext())
                {
                    var result = ctx.Set<AssassinImage>() as IQueryable<AssassinImage>;
                    IList list = DoLazyLoading(result.AsNoTracking(), sql, includeAll) as IList;
                    ctx.DetachAllEntities();
                    return list;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Could not get entities.", ex);
            }
        }

        /// <inheritdoc />
        internal IList GetFirst(string type, int takeCount, int skip, string sql, bool includeAll = false)
        {
            try
            {
                using (var ctx = NewContext())
                {
                    var temp = ctx.Set<AssassinImage>()
                        .OrderByDescending(p => p.CreatedDT)
                        .Skip(skip)
                        .Take(takeCount) as IQueryable<AssassinImage>;

                    IList result = DoLazyLoading(temp.AsNoTracking(), sql, includeAll) as IList;
                    ctx.DetachAllEntities();
                    return result;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Could not get entities.", ex);
            }
        }

        /// <inheritdoc />
        internal virtual IList GetByIdList(List<Guid> idList, bool includeAll)
        {
            try
            {
                using (var ctx = NewContext())
                {
                    var list = DoLazyLoading(ctx.Set<AssassinImage>().Where(p => idList.Contains(p.Id)), null, includeAll);
                    ctx.DetachAllEntities();
                    return list as IList;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Could not get entities.", ex);
            }
        }

        /// <inheritdoc />
        internal virtual AssassinImage Get(Guid id, bool includeAll)
        {
            try
            {
                using (var ctx = NewContext())
                {
                    var list = DoLazyLoading(ctx.Set<AssassinImage>().Where(p => p.Id == id), null, includeAll) as IQueryable<AssassinImage>;
                    ctx.DetachAllEntities();
                    return list.FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Could not get entity.", ex);
            }
        }

        /// <inheritdoc />
        internal virtual object Get(AssassinImage data, bool includeAll)
        {
            try
            {
                using (var ctx = NewContext())
                {
                    var list = DoLazyLoading(ctx.Set<AssassinImage>().Where(p => p.Id == data.Id), null, includeAll) as IQueryable<AssassinImage>;
                    ctx.DetachAllEntities();
                    return list.FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Could not get entity.", ex);
            }
        }

        /// <inheritdoc />
        internal void Shrink()
        {
            try
            {
                using (var ctx = NewContext())
                {
                    ctx.Database.ExecuteSqlCommand("VACUUM;");
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Could not shrink database", ex);
            }
        }

        /// <inheritdoc />
        internal void EnsureDatabase()
        {
            NewContext().Ensure();
        }

        /// <summary>
        /// Does the lazy loading.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="queryable">The queryable.</param>
        /// <param name="func">The function.</param>
        /// <param name="includeAll">if set to <c>true</c> [include all].</param>
        /// <returns></returns>
        private IList<T> DoLazyLoading<T>(IQueryable<T> queryable, string func, bool includeAll) where T : AssassinImage
        {
            if (includeAll)
            {
                if (func != null)
                {
                    return NewContext().IncludeAll(queryable, func).ToList() as List<T>;
                }
                else
                {
                    return NewContext().IncludeAll(queryable) as List<T>;
                }
            }
            else
            {
                if (func != null)
                {
                    queryable = queryable.FromSql("SELECT * FROM ASSASSINIMAGE WHERE " + func);
                }
                return queryable.ToList();
            }
        }

        /// <summary>
        /// To the expression.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="f">The f.</param>
        /// <returns>the converted expression</returns>
        private Expression<Func<T, bool>> ToExpression<T>(Func<T, bool> f)
        {
            return x => f(x);
        }

        /// <summary>
        /// News the context.
        /// </summary>
        /// <returns></returns>
        private AssassinImageDataContext NewContext()
        {
            return new AssassinImageDataContext();
        }
    }
}
