using System;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.ExceptionHandling;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.PeopleAdmin.GuiHelpers
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
    }
}
