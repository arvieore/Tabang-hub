using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tabang_Hub.Utils;

namespace Tabang_Hub.Contracts
{
    public interface IBaseRepository<T>
    {
        T Get(object id);
        List<T> GetAll();
        ErrorCode Create(T t, out String errorMsg);
        ErrorCode Update(object id, T t, out String errorMsg);
        ErrorCode Delete(object id, out String errorMsg);

        ErrorCode Create(T t);  //Equivalent to INSERT
        ErrorCode Update(object id, T t);   //Equivalent to Update
        ErrorCode Delete(object id);    //Equivalent to Delete
    }
}
