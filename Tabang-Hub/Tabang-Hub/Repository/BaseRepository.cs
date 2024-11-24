using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using Tabang_Hub.Contracts;
using Tabang_Hub.Utils;

namespace Tabang_Hub.Repository
{
    public class BaseRepository<T> : IBaseRepository<T>
        where T : class
    {
        public DbContext _db;
        public DbSet<T> _table;
        public BaseRepository() 
        {
            _db = new TabangHubEntities();
            _table = _db.Set<T>();
        }

        public T Get(object id)
        {
            return _table.Find(id);
        }

        public List<T> GetAll()
        {
            return _table.ToList();
        }

        public ErrorCode Create(T t, out string errorMsg)
        {
            try
            {
                _table.Add(t);
                _db.SaveChanges();
                errorMsg = "Success";

                return ErrorCode.Success;
            }
            catch (Exception ex)
            {
                errorMsg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return ErrorCode.Error;
            }
        }

        public ErrorCode Create(T t)
        {
            try
            {
                _table.Add(t);
                _db.SaveChanges();             

                return ErrorCode.Success;
            }
            catch (Exception ex)
            {             
                return ErrorCode.Error;
            }
        }
        public ErrorCode Delete(object id)
        {
            try
            {
                var obj = Get(id);
                _table.Remove(obj);
                _db.SaveChanges();

                return ErrorCode.Success;
            }
            catch (Exception ex)
            {
                return ErrorCode.Error;
            }
        }    

    
        public ErrorCode Update(object id, T t, out string errorMsg)
        {
            try
            {
                var oldObj = Get(id);
                _db.Entry(oldObj).CurrentValues.SetValues(t);
                _db.SaveChanges();
                errorMsg = "Updated";

                return ErrorCode.Success;
            }
            catch (Exception ex)
            {
                errorMsg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return ErrorCode.Error;
            }
        }

        public ErrorCode Delete(object id, out string errorMsg)
        {
            throw new NotImplementedException();
        }

        ErrorCode IBaseRepository<T>.Create(T t, out string errorMsg)
        {
            throw new NotImplementedException();
        }

        ErrorCode IBaseRepository<T>.Update(object id, T t, out string errorMsg)
        {
            throw new NotImplementedException();
        }

        ErrorCode IBaseRepository<T>.Delete(object id, out string errorMsg)
        {
            throw new NotImplementedException();
        }

        public ErrorCode Update(object id, T t)
        {
            throw new NotImplementedException();
        }
    }
}