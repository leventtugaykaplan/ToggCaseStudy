﻿using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using UserPortal.Data.Interfaces;

namespace UserPortal.Data.Repositories
{
    public class UserPortalRepository<T>
        : IGenericRepository<T>
        where T : class
    {
        private DataContext _context;
        private DbSet<T> _dbSet;
        public UserPortalRepository(DataContext Context)
        {
            _context = Context;
            _dbSet = _context.Set<T>();
        }

        public virtual bool SaveChanges()
        {
            return (_context.SaveChanges() >= 0);
        }

        public virtual T FindById(object EntityId)
        {
            return _dbSet.Find(EntityId);
        }

        public virtual IEnumerable<T> Select(Expression<Func<T, bool>> Filter = null)
        {
            if (Filter != null)
            {
                return _dbSet.Where(Filter);
            }
            return _dbSet;
        }

        public virtual void Insert(T entity)
        {
            _dbSet.Add(entity);
        }

        public virtual void Update(T entityToUpdate)
        {
            _dbSet.Attach(entityToUpdate);
            _context.Entry(entityToUpdate).State = EntityState.Modified;
        }

        public virtual void Delete(object EntityId)
        {
            T entityToDelete = _dbSet.Find(EntityId);
            Delete(entityToDelete);
        }

        public virtual void Delete(T Entity)
        {
            if (_context.Entry(Entity).State == EntityState.Detached) //Concurrency için 
            {
                _dbSet.Attach(Entity);
            }
            _dbSet.Remove(Entity);
        }
    }
}
