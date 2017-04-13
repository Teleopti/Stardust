using System;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.ExceptionHandling;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common
{
    public interface IGracefulDataSourceExceptionHandler
    {
        bool DataSourceExceptionOccurred(Exception exception);
        bool AttemptDatabaseConnectionDependentAction(Action action);
    }

    public class GracefulDataSourceExceptionHandler : IGracefulDataSourceExceptionHandler
    {
        public bool DataSourceExceptionOccurred(Exception exception)
        {
            if (exception != null)
            {
                var dataSourceException = exception as DataSourceException;
                if (dataSourceException == null)
                    return false;

                using (var view = new SimpleExceptionHandlerView(dataSourceException, UserTexts.Resources.OpenTeleoptiCCC, UserTexts.Resources.ServerUnavailable))
                {
                    view.ShowDialog();
                }

                return true;
            }
            return false;
        }

        public bool AttemptDatabaseConnectionDependentAction(Action action)
        {
            try
            {
                action.Invoke();
                return true;
            }
            catch (DataSourceException dataSourceException)
            {
                if (DataSourceExceptionOccurred(dataSourceException))
                    return false;
                throw;
            }
        }
    }
}
