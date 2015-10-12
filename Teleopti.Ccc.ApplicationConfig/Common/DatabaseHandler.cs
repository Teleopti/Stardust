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
                   buildSessionFactory(_commandLineArgument.DestinationConnectionString);
                }
                return _sessionFactory;
            }
        }

	    public ISessionFactory GetSessionFactory(string destinationConnectionString)
	    {
		    buildSessionFactory(destinationConnectionString);
		    return _sessionFactory;
	    }

       private void buildSessionFactory(string destinationConnectionString)
	    {
			Configuration cfg = new Configuration()
							 .AddProperties(DataSourceSettings(destinationConnectionString))
							 .AddAssembly(typeof(ApplicationRole).Assembly);
			_sessionFactory = cfg.BuildSessionFactory();
		}
        public static IDictionary<string, string> DataSourceSettings(string destinationConnectionString)
        {
            return new Dictionary<string, string>
                       {
                           {"connection.provider", "NHibernate.Connection.DriverConnectionProvider" },
                           {"connection.connection_string", destinationConnectionString},
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