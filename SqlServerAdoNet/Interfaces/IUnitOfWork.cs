using System;
using System.Data;

namespace StatKings.SqlServerAdoNet
{
    public interface IUnitOfWork : IDisposable
    {
        IDbCommand CreateCommand();
        void BeginTransaction();
        void Commit();
        void Rollback();
    }
}
