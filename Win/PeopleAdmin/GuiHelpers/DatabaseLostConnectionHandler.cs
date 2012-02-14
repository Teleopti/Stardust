using System;
using Teleopti.Ccc.Win.ExceptionHandling;

namespace Teleopti.Ccc.Win.PeopleAdmin.GuiHelpers
{
    public static class DatabaseLostConnectionHandler
    {
        public static void ShowConnectionLostFromCloseDialog(Exception exception)
        {
            using (var view = new SimpleExceptionHandlerView(exception, UserTexts.Resources.OpenTeleoptiCCC, UserTexts.Resources.DatabaseConnectionLostFormClose))
            {
                view.ShowDialog();
            }
        }

        public static void ShowConnectionLostOperationAbort(Exception exception)
        {
            using (var view = new SimpleExceptionHandlerView(exception, UserTexts.Resources.OpenTeleoptiCCC, UserTexts.Resources.ServerUnavailable))
            {
                view.ShowDialog();
            }
        }
    }
}
