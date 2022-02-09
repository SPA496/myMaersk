using System;
using System.Data.Common;

namespace myDamco.Areas.MySupplyChainAssistantAdministration.Data
{
    public abstract class Repository<T> : IDisposable where T : DbConnection
    {
        protected T conn;

        public Repository()
        {
            this.conn = GetConnection();
            if (conn != null)
                conn.Open();
        }
        public void Dispose()
        {
            if (conn != null)
            {
                conn.Close();
                conn.Dispose();
            }
        }

        protected abstract T GetConnection();
    }
}