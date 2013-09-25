using System;
using System.Linq;
using log4net.Appender;
using log4net.Repository.Hierarchy;

namespace Teleopti.Ccc.Domain.Common.Logging
{
    public static class Log4NetConfiguration
    {
        public static void SetConnectionString(string connectionString)
        {
            var logHierarchy = log4net.LogManager.GetRepository() as Hierarchy;

            //if (logHierarchy != null && logHierarchy.Configured)
            //{
            //    foreach (IAppender appenderItem in logHierarchy.GetAppenders())
            //    {
            //        if (appenderItem is AdoNetAppender)
            //        {
            //            var adoNetAppender = (AdoNetAppender)appenderItem;
            //            adoNetAppender.ConnectionString = connectionString;
            //            adoNetAppender.ActivateOptions(); //Refresh AdoNetAppenders Settings
            //        }
            //    }
            //}

            if (logHierarchy != null)
            {
                var appender = logHierarchy.GetAppenders()
                                      .OfType<AdoNetAppender>()
                                      .SingleOrDefault();

                if (appender != null)
                {
                    appender.ConnectionString = connectionString;
                    appender.ActivateOptions();
                }

               
            }

           
        }
    }
}
