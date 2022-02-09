using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using myDamco.Areas.MySupplyChainAssistantAdministration.Models;

namespace myDamco.Areas.MySupplyChainAssistantAdministration.Data
{
    public class SCASqlCERepository : SCARepository<SqlConnection>
    {
        protected override SqlConnection GetConnection()
        {
            //var path = "DataSource=\"" + AppDomain.CurrentDomain.GetData("DataDirectory").ToString() + "\\MySCA.sdf\";";
            var path = System.Configuration.ConfigurationManager.
                        ConnectionStrings["myDamcoDatabase"].ConnectionString;
            return new SqlConnection(path);
        }


        #region Applications
        public override int CreateApplication(Application application)
        {
            SqlCommand cmd = new SqlCommand("Insert into Assistent_Applications (Name, Abbreviation) Values (@Name, @Abbreviation);", conn);
            cmd.Parameters.Add(new SqlParameter("@Name", application.Name));
            cmd.Parameters.Add(new SqlParameter("@Abbreviation", application.Abbreviation));
            return cmd.ExecuteNonQuery();
        }

        public override IEnumerable<Application> GetApplications()
        {
            return GetApplications(null);
        }

        public override IEnumerable<Application> GetApplications(IEnumerable<int> ids)
        {
            string clause = (ids != null && ids.Count() > 0) ? " Where ApplicationId in (" + string.Join(",", ids) + ")" : "";
            var applications = new List<Application>();
            SqlCommand cmd = new SqlCommand("Select ApplicationId, Name, Abbreviation from Assistent_Applications" + clause + ";", conn);
            using (SqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    var app = new Application
                    {
                        ApplicationId = reader.GetInt32(0),
                        Name = reader.GetString(1),
                        Abbreviation = reader.GetString(2)
                    };
                    applications.Add(app);
                }
            }
            return applications.AsQueryable();
        }

        public override Application GetApplication(int id)
        {
            return GetApplications(new List<int> { id }).First();
        }

        public override void UpdateApplication(Application application)
        {
            SqlCommand cmd = new SqlCommand("Update Applications Set Name = @Name, Abbreviation = @Abbreviation Where ApplicationId = @ApplicationId;", conn);
            cmd.Parameters.Add(new SqlParameter("@ApplicationId", application.ApplicationId));
            cmd.Parameters.Add(new SqlParameter("@Name", application.Name));
            cmd.Parameters.Add(new SqlParameter("@Abbreviation", application.Abbreviation));
            cmd.ExecuteNonQuery();
        }

        public override void DeleteApplication(int id)
        {
            SqlCommand cmd = new SqlCommand("Delete from Assistent_Applications Where ApplicationId = @ApplicationId;", conn);
            cmd.Parameters.Add(new SqlParameter("@ApplicationId", id));
            cmd.ExecuteNonQuery();
        }
        #endregion

        #region Functions
        public override int CreateFunction(Function function)
        {
            SqlCommand cmd = new SqlCommand("Insert Into Assistent_Functions (ApplicationId, Name, Description, URLFormat, FallbackURL, Protocol, Host, Port, Path) Values (@ApplicationId, @Name, @Description, @URLFormat, @FallbackURL, @Protocol, @Host, @Port, @Path);", conn);
            cmd.Parameters.Add(new SqlParameter("@ApplicationId", function.applicationId));
            cmd.Parameters.Add(new SqlParameter("@Name", function.name));
            cmd.Parameters.Add(new SqlParameter("@Description", function.description));
            cmd.Parameters.Add(new SqlParameter("@URLFormat", function.urlFormat));
            cmd.Parameters.Add(new SqlParameter("@FallbackURL", function.fallbackUrl));
            if (function.protocol == null)
            {
                cmd.Parameters.Add(new SqlParameter("@Protocol", DBNull.Value));
            }
            else
            {
                cmd.Parameters.Add(new SqlParameter("@Protocol", function.protocol));
            }
            cmd.Parameters.Add(new SqlParameter("@Host", function.host));
            if (function.port == null)
            {
                cmd.Parameters.Add(new SqlParameter("@Port", DBNull.Value));
            }
            else
            {
                cmd.Parameters.Add(new SqlParameter("@Port", function.port));
            }
            cmd.Parameters.Add(new SqlParameter("@Path", function.path));
            var fid = cmd.ExecuteNonQuery();

            // Insert references.
            if (function.references != null && function.references.Count() > 0)
            {
                cmd = new SqlCommand("Insert Into Assistent_FunctionReferences (FunctionId, ReferenceId) Values (@FunctionId, @ReferenceId);", conn);
                foreach (int func in function.references)
                {
                    cmd.Parameters.Add(new SqlParameter("@FunctionId", fid));
                    cmd.Parameters.Add(new SqlParameter("@ReferenceId", func));
                    cmd.ExecuteNonQuery();
                }
            }

            // Insert arguments.
            if (function.arguments != null && function.arguments.Count > 0)
            {
                cmd = new SqlCommand("Insert Into Assistent_Arguments (FunctionId, Id, Alias, Matcher) Values (@FunctionId, @Id, @Alias, @Matcher);", conn);
                foreach (Argument argument in function.arguments)
                {
                    cmd.Parameters.Add(new SqlParameter("@FunctionId", function.functionId));
                    cmd.Parameters.Add(new SqlParameter("@Id", argument.id));
                    cmd.Parameters.Add(new SqlParameter("@Alias", argument.alias));
                    cmd.Parameters.Add(new SqlParameter("@Matcher", argument.matcher));
                    cmd.ExecuteNonQuery();
                }
            }

            return fid;
        }

        /*public override IEnumerable<Function> GetFunctions()
        {
            return GetFunctions(null);
        }*/

        public override IEnumerable<int> FunctionsExists(IEnumerable<int> ids)
        {
            List<int> fids = new List<int>(), exists = new List<int>();

            // Get the functions.
            SqlCommand cmd = new SqlCommand("Select FunctionId From Assistent_Functions Where FunctionId In (" + string.Join(",", ids) + ";", conn);
            using (SqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    exists.Add(reader.GetInt32(0));
                }
            }

            return fids.Except(exists);
        }

        public override IEnumerable<Function> GetFunctions()//IEnumerable<int> ids)
        {
            //string clause = (ids != null && ids.Count() > 0) ? " Where FunctionId in (" + string.Join(",", ids) + ")" : "";
            var functions = new List<Function>();
            List<int> fids = new List<int>();

            // Get the functions.
            SqlCommand cmd = new SqlCommand("Select FunctionId, ApplicationId, Name, Description, URLFormat, FallbackURL, Protocol, Host, Port, Path From Assistent_Functions;", conn);
            using (SqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    Function func = new Function
                    {
                        functionId = reader.GetInt32(0),
                        applicationId = reader.GetInt32(1),
                        name = reader.GetString(2),
                        description = reader.GetString(3),
                        urlFormat = reader.GetString(4),
                        fallbackUrl = reader.GetString(5),
                        protocol = reader.IsDBNull(6) ? null : reader.GetString(6),
                        host = reader.GetString(7),
                        port = reader.IsDBNull(8) ? null : reader.GetString(8),
                        path = reader.GetString(9),
                        arguments = new List<Argument>(),
                        references = new List<int>(),
                        entityIdentifiers = new List<EntityIdentifier>(),
                        hooks = new List<EventHook>()
                    };
                    functions.Add(func);
                    fids.Add(func.functionId);
                }
            }

            // Do not look for referenced functions or arguments if no functions exists. Duh...
            if (fids.Count() > 0)
            {
                Dictionary<int, List<int>> references = new Dictionary<int,List<int>>();
                // Get their references.
                cmd = new SqlCommand("Select FunctionId, ReferenceId From Assistent_FunctionReferences;", conn);// Where FunctionId in (" + string.Join(", ", fids) + ");", conn);
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var cid = reader.GetInt32(0);
                        Function func = functions.Find(f => f.functionId == cid);
                        if (func != null) func.references.Add(reader.GetInt32(1));
                        /*if (!references.Keys.Contains(cid))
                        {
                            references[cid] = new List<int>();
                        }
                        references[cid].Add(reader.GetInt32(1));*/
                    }
                }

                // Retrieve arguments.
                cmd = new SqlCommand("Select FunctionId, Id, Alias, Matcher From Assistent_Arguments Where FunctionId in (" + string.Join(", ", fids) + ");", conn);
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var fid = reader.GetInt32(0);
                        var arg = new Argument
                        {
                            id = reader.GetString(1),
                            alias = reader.GetString(2),
                            matcher = reader.GetString(3)
                        };
                        functions.Where(f => f.functionId == fid).ToList().ForEach(a => a.arguments.Add(arg));
                    }
                }

                // Retrieve entityIdentifiers.
                cmd = new SqlCommand("Select FunctionId, EntityId, Selector From Assistent_EntityIdentifiers Where FunctionId in (" + string.Join(", ", fids) + ");", conn);
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var fid = reader.GetInt32(0);
                        var arg = new EntityIdentifier
                        {
                            entityId = reader.GetString(1),
                            selector = reader.GetString(2)
                        };
                        functions.Where(f => f.functionId == fid).ToList().ForEach(a => a.entityIdentifiers.Add(arg));
                    }
                }
            }

            // Retrieve hooks.
            cmd = new SqlCommand("Select FunctionId, Title, Selector, Hook From Assistent_EventHooks Where FunctionId in (" + string.Join(", ", fids) + ");", conn);
            using (SqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    var fid = reader.GetInt32(0);
                    var arg = new EventHook
                    {
                        title = reader.GetString(1),
                        selector = reader.GetString(2),
                        hook = reader.GetString(3)
                    };
                    functions.Where(f => f.functionId == fid).ToList().ForEach(a => a.hooks.Add(arg));
                }
            }

            return functions.AsQueryable();
        }

        /*public override Function GetFunction(int id)
        {
            return GetFunctions(new List<int> { id }).First();
        }*/

        public override void UpdateFunction(Function function)
        {
            // Update base info.
            SqlCommand cmd = new SqlCommand("Update Functions Set ApplicationId = @ApplicationId, Name = @Name, Description = @Description, URLFormat = @URLFormat, FallbackURL = @FallbackURL, Protocol = @Protocol, Host = @Host, Port = @Port, Path = @Path Where FunctionId = @FunctionId;", conn);
            cmd.Parameters.Add(new SqlParameter("@FunctionId", function.functionId));
            cmd.Parameters.Add(new SqlParameter("@ApplicationId", function.applicationId));
            cmd.Parameters.Add(new SqlParameter("@Name", function.name));
            cmd.Parameters.Add(new SqlParameter("@Description", function.description));
            cmd.Parameters.Add(new SqlParameter("@URLFormat", function.urlFormat));
            cmd.Parameters.Add(new SqlParameter("@FallbackURL", function.fallbackUrl));
            if (function.protocol == null)
            {
                cmd.Parameters.Add(new SqlParameter("@Protocol", DBNull.Value));
            }
            else
            {
                cmd.Parameters.Add(new SqlParameter("@Protocol", function.protocol));
            }
            cmd.Parameters.Add(new SqlParameter("@Host", function.host));
            if (function.port == null)
            {
                cmd.Parameters.Add(new SqlParameter("@Port", DBNull.Value));
            }
            else
            {
                cmd.Parameters.Add(new SqlParameter("@Port", function.port));
            }
            cmd.Parameters.Add(new SqlParameter("@Path", function.path));
            cmd.ExecuteNonQuery();

            // Delete old references.
            cmd = new SqlCommand("Delete From Assistent_FunctionReferences Where FunctionId = @FunctionId;", conn);
            cmd.Parameters.Add(new SqlParameter("@FunctionId", function.functionId));
            cmd.ExecuteNonQuery();
            // Insert new references.
            if (function.references != null && function.references.Count > 0)
            {
                cmd = new SqlCommand("Insert Into Assistent_FunctionReferences (FunctionId, ReferenceId) Values (@FunctionId, @ReferenceId);", conn);
                foreach (int func in function.references)
                {
                    cmd.Parameters.Add(new SqlParameter("@FunctionId", function.functionId));
                    cmd.Parameters.Add(new SqlParameter("@ReferenceId", func));
                    cmd.ExecuteNonQuery();
                }
            }

            // Delete old arguments.
            cmd = new SqlCommand("Delete From Assistent_Arguments Where FunctionId = @FunctionId;", conn);
            cmd.Parameters.Add(new SqlParameter("@FunctionId", function.functionId));
            cmd.ExecuteNonQuery();

            // Insert new arguments.
            if (function.arguments != null && function.arguments.Count > 0)
            {
                cmd = new SqlCommand("Insert Into Assistent_Arguments (FunctionId, Id, Alias, Matcher) Values (@FunctionId, @Id, @Alias, @Matcher);", conn);
                foreach (Argument argument in function.arguments)
                {
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add(new SqlParameter("@FunctionId", function.functionId));
                    cmd.Parameters.Add(new SqlParameter("@Id", argument.id));
                    cmd.Parameters.Add(new SqlParameter("@Alias", argument.alias));
                    cmd.Parameters.Add(new SqlParameter("@Matcher", argument.matcher));
                    cmd.ExecuteNonQuery();
                }
            }

            // Delete old entityidentifiers.
            cmd = new SqlCommand("Delete From Assistent_EntityIdentifiers Where FunctionId = @FunctionId;", conn);
            cmd.Parameters.Add(new SqlParameter("@FunctionId", function.functionId));
            cmd.ExecuteNonQuery();

            // Insert new arguments.
            if (function.entityIdentifiers != null && function.entityIdentifiers.Count > 0)
            {
                cmd = new SqlCommand("Insert Into Assistent_EntityIdentifiers (FunctionId, EntityId, Selector) Values (@FunctionId, @EntityId, @Selector);", conn);
                foreach (EntityIdentifier entity in function.entityIdentifiers)
                {
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add(new SqlParameter("@FunctionId", function.functionId));
                    cmd.Parameters.Add(new SqlParameter("@EntityId", entity.entityId));
                    cmd.Parameters.Add(new SqlParameter("@Selector", entity.selector));
                    cmd.ExecuteNonQuery();
                }
            }

            // Delete old arguments.
            cmd = new SqlCommand("Delete From Assistent_EventHooks Where FunctionId = @FunctionId;", conn);
            cmd.Parameters.Add(new SqlParameter("@FunctionId", function.functionId));
            cmd.ExecuteNonQuery();

            // Insert new arguments.
            if (function.hooks != null && function.hooks.Count > 0)
            {
                cmd = new SqlCommand("Insert Into Assistent_Arguments (FunctionId, Title, Selector, Hook) Values (@FunctionId, @Title, @Selector, @Hook);", conn);
                foreach (EventHook hook in function.hooks)
                {
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add(new SqlParameter("@FunctionId", function.functionId));
                    cmd.Parameters.Add(new SqlParameter("@Id", hook.title));
                    cmd.Parameters.Add(new SqlParameter("@Alias", hook.selector));
                    cmd.Parameters.Add(new SqlParameter("@Matcher", hook.hook));
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public override void DeleteFunction(int id)
        {
            SqlCommand cmd = new SqlCommand("Delete From Assistent_Functions Where FunctionId = @FunctionId;", conn);
            cmd.Parameters.Add(new SqlParameter("@FunctionId", id));
            cmd.ExecuteNonQuery();
            cmd = new SqlCommand("Delete From Assistent_FunctionReferences Where FunctionId = @FunctionId Or ReferenceId = @ReferenceId;", conn);
            cmd.Parameters.Add(new SqlParameter("@FunctionId", id));
            cmd.Parameters.Add(new SqlParameter("@ReferenceId", id));
            cmd.ExecuteNonQuery();
            cmd = new SqlCommand("Delete From Assistent_Arguments Where FunctionId = @FunctionId;", conn);
            cmd.Parameters.Add(new SqlParameter("@FunctionId", id));
            cmd.ExecuteNonQuery();
            cmd = new SqlCommand("Delete From Assistent_EntityIdentifiers Where FunctionId = @FunctionId;", conn);
            cmd.Parameters.Add(new SqlParameter("@FunctionId", id));
            cmd.ExecuteNonQuery();
            cmd = new SqlCommand("Delete From Assistent_EventHooks Where FunctionId = @FunctionId;", conn);
            cmd.Parameters.Add(new SqlParameter("@FunctionId", id));
            cmd.ExecuteNonQuery();
        }
        #endregion

        public override void Log(LogEntry entry)
        {
            var cmd = new SqlCommand("Insert Into LogEntries (Username, Role, Function, Hook, Data) Values (@Username, @Role, @Function, @Hook, @Data);", conn);
            cmd.Parameters.Add(new SqlParameter("@Username", entry.user.name));
            cmd.Parameters.Add(new SqlParameter("@Role", entry.user.role));
            cmd.Parameters.Add(new SqlParameter("@Function", entry.function));
            cmd.Parameters.Add(new SqlParameter("@Hook", entry.hook));
            cmd.Parameters.Add(new SqlParameter("@Data", entry.data));
            cmd.ExecuteNonQuery();
        }

        public override IEnumerable<string> GetEntityTypes()
        {
            List<string> entityIds = new List<string>();
            var cmd = new SqlCommand("Select EntityId From Assistent_EntityIdentifiers Union Select Id As EntityId From Assistent_Arguments;", conn);
            using (SqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    entityIds.Add(reader.GetString(0));
                }
            }
            return entityIds;
        }
    }
}