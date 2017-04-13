using System;
using System.Runtime.InteropServices;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common
{
    public interface IExternalExceptionHandler
    {
        bool ExternalExceptionOccurred(Exception exception);
        bool AttemptToUseExternalResource(Action action);
    }

    public class ExternalExceptionHandler : IExternalExceptionHandler
    {
        public bool ExternalExceptionOccurred(Exception exception)
        {
            if (exception != null)
            {
                var dataSourceException = exception as ExternalException;
                if (dataSourceException == null)
                    return false;
                ViewBase.ShowInformationMessage(
                    UserTexts.Resources.ExternalResourceIsUsingByAnotherProgramCommaPleaseTryAgainLaterDot,
                    UserTexts.Resources.ExternalResourceInUse);
                return true;
            }
            return false;
        }

        public bool AttemptToUseExternalResource(Action action)
        {
            try
            {
                action.Invoke();
                return true;
            }
            catch (ExternalException exception)
            {
                if (ExternalExceptionOccurred(exception))
                    return false;
                throw;
            }
        }
    }
}
