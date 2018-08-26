using System;
using System.Collections.Generic;

namespace StatKings.SqlServerAdoNet
{
    public interface ISqlRunner
    {
        T Get<T>(params object[] keyValues);

        IEnumerable<T> GetAll<T>();

        void Delete<T>(params object[] keyValues);

        int InsertForId<T>(T entity);

        void Insert<T>(T entity);

        void Update<T>(T entity);

        void ExecuteNonQuery(CommandSettings commandSettings);

        object ExecuteScalar(CommandSettings commandSettings);

        T ExecuteReaderFirst<T>(CommandSettings commandSettings);

        IEnumerable<T> ExecuteReader<T>(CommandSettings commandSettings);
    }
}
