using Microsoft.Practices.Composite.Presentation.Events;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.Requests
{
    public enum RequestHistoryPage
    {
        First,
        Next,
        Previous
    }
    public class RequestHistoryPageChanged : CompositePresentationEvent<RequestHistoryPage>
    {
    }
}