using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Data.Objects;
using System.Linq;
using System.Web;

namespace GenericRepositoryWithCatching.Models
{
    public class DataRepository : IDisposable
    {
        #region Context
        private BlogEntities _Context;
        public BlogEntities Context
        {
            get
            {
                if (_Context == null)
                {
                    _Context = new BlogEntities();
                }
                return _Context;
            }

        }
        private RepositoryCaching _Cache;
        public RepositoryCaching Cache
        {
            get
            {
                if (_Cache == null)
                {
                    _Cache = new RepositoryCaching();
                }
                return _Cache;
            }

        }
        #endregion

        /// <summary>
        /// Retrieve will support 2 cases :
        ///1) retrieve ALL records : if parameter  PRED is empty  
        ///2) retrieve just ONE record :  if the PRED parameter is set :
        /// </summary>       
        public IQueryable<T> Retrieve<T>(Func<T, bool> pred) where T : class
        {
            List<T> list = new List<T>();
            string sType = typeof(T).ToString();
            if (Cache.IsInMemory(sType))
            {

                if (pred != null)
                {
                    List<T> oCachedList = Cache.FetchData<T>(sType).ToList<T>();
                    if (oCachedList != null)
                    {
                        list.Add(oCachedList.SingleOrDefault(pred));
                    }

                }
                else list = Cache.FetchData<T>(sType).ToList<T>();
                return list.AsQueryable();

            }
            else
            {
                if (pred != null)
                {
                    list = Context.Set<T>().Where(pred).ToList();
                }
                else
                {
                    list = Context.Set<T>().ToList();
                    Cache.Add(sType, list, 60);
                }
                return list.AsQueryable();
            }
        }


        public void Create<T>(T entity) where T : class
        {
            Context.Set<T>().Add(entity);
            Type tType = typeof(T);
            Cache.Remove(tType.ToString());
        }

        public void Update<T>(T entity) where T : class
        {
            var e = Context.Entry<T>(entity);
            e.State = EntityState.Modified;
            Type tType = typeof(T);
            Cache.Remove(tType.ToString());
        }

        public void Delete<T>(T entity) where T : class
        {

            var entry = Context.Entry<T>(entity);
            if (entry != null)
            {
                entry.State = EntityState.Deleted;
            }


            Type tType = typeof(T);
            Cache.Remove(tType.ToString());
        }



        public bool Save(object target, int RecordsNumber)
        {
            try
            {
                return Context.SaveChanges() == RecordsNumber;
            }
            catch (OptimisticConcurrencyException)
            {
                ObjectContext ctx = ((IObjectContextAdapter)Context).ObjectContext;
                ctx.Refresh(RefreshMode.ClientWins, target);
                return Context.SaveChanges() == RecordsNumber;
            }
        }

        public void Dispose()
        {
            if (Context != null)
            {
                Context.Dispose();
                GC.Collect();
            }
        }
    }
}
