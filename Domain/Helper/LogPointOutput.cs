using log4net;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.Domain.Helper
{
    public static class LogPointOutput
    {
        private static ILog _log;

        public static void LogInfo(string info, string eventType)
        {
            if (getLog().IsInfoEnabled)
            {
                GlobalContext.Properties["EventType"] = eventType;
                getLog().Info(info);
            }
        }

        private static ILog getLog()
        {
            if (_log == null)
            {
                var identity = TeleoptiPrincipal.Current.Identity as ITeleoptiIdentity;
                if (identity != null)
                {
                    GlobalContext.Properties["BU"] = identity.BusinessUnit.Name;
                    GlobalContext.Properties["DataSource"] = identity.DataSource.Server;
                    GlobalContext.Properties["InitialCatalog"] = identity.DataSource.InitialCatalog;
                }
                _log = LogManager.GetLogger("Teleopti.LogPointOutput");
            }

            return _log;
        }
    }
}