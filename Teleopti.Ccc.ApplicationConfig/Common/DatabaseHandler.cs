using System.Collections.Generic;
using NHibernate;
using NHibernate.Cfg;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.ApplicationConfig.Common
{
    public class DatabaseHandler
    {
        private readonly ICommandLineArgument _commandLineArgument;
        private ISessionFactory _sessionFactory;

        public DatabaseHandler(ICommandLineArgument commandLineArgument)
        {
            _commandLineArgument = commandLineArgument;
        }

        public ISessionFactory SessionFactory
        {
            get
            {
                if (_sessionFactory == null)
                {
                    Configuration cfg = new Configuration()
                        .AddProperties(DataSourceSettings())
                        .AddAssembly(typeof(ApplicationRole).Assembly);
                    _sessionFactory = cfg.BuildSessionFactory();
                }
                return _sessionFactory;
            }
        }

        public IDictionary<string, string> DataSourceSettings()
        {
            return new Dictionary<string, string>
                       {
                           {"connection.provider", "NHibernate.Connection.DriverConnectionProvider" },
                           {"connection.connection_string", _commandLineArgument.DestinationConnectionString},
                           {"show_sql", "false"},
                           {"dialect", "NHibernate.Dialect.MsSql2008Dialect"},
                           {"adonet.batch_size", "50"},
                           {Environment.CurrentSessionContextClass, "thread_static"},
                           {"connection.driver_class", "NHibernate.Driver.SqlClientDriver"},
                           {Environment.UseSecondLevelCache, "false"},
                       };
        }

    }
}