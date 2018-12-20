using System;
using System.Data.SqlClient;
using log4net;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.Domain.Helper
{
    public static class LogPointOutput
    {
        private static readonly Lazy<ILog> _log = new Lazy<ILog>(getLog);

        public static void LogInfo(string info, string eventType)
        {
            if (_log.Value.IsInfoEnabled)
            {
                GlobalContext.Properties["EventType"] = eventType;
                _log.Value.Info(info);
            }
        }

	    private static ILog getLog()
	    {
		    var identity = TeleoptiPrincipalForLegacy.CurrentPrincipal.Identity as ITeleoptiIdentity;
		    var appConnString = new SqlConnectionStringBuilder(identity.DataSource.Application.ConnectionString);
		    if (identity != null)
		    {
			    GlobalContext.Properties["BU"] = identity.BusinessUnit.Name;
			    GlobalContext.Properties["DataSource"] = appConnString.DataSource;
			    GlobalContext.Properties["InitialCatalog"] = appConnString.InitialCatalog;
		    }
		    var log = LogManager.GetLogger("Teleopti.LogPointOutput");

		    return log;
	    }
    }
}