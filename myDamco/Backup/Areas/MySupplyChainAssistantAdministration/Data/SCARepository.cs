using System.Collections.Generic;
using System.Data.Common;
using myDamco.Areas.MySupplyChainAssistantAdministration.Models;

namespace myDamco.Areas.MySupplyChainAssistantAdministration.Data
{
    public abstract class SCARepository<T> : Repository<T> where T : DbConnection
    {
        public abstract IEnumerable<Application> GetApplications();
        public abstract IEnumerable<Application> GetApplications(IEnumerable<int> ids);
        public abstract Application GetApplication(int id);
        public abstract int CreateApplication(Application application);
        public abstract void UpdateApplication(Application application);
        public abstract void DeleteApplication(int id);

        public abstract IEnumerable<Function> GetFunctions();
        //public abstract IEnumerable<Function> GetFunctions(IEnumerable<int> ids);
        //public abstract Function GetFunction(int id);
        public abstract int CreateFunction(Function function);
        public abstract void UpdateFunction(Function function);
        public abstract void DeleteFunction(int id);
        public abstract IEnumerable<int> FunctionsExists(IEnumerable<int> id);

        public abstract void Log(LogEntry entry);
        public abstract IEnumerable<string> GetEntityTypes();

        //public abstract IEnumerable<Function> SearchFunctions(string query);
    }
}