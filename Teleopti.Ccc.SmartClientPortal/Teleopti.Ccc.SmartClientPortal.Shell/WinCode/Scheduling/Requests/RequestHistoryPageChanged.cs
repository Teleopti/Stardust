using Microsoft.Practices.Composite.Presentation.Events;

namespace Teleopti.Ccc.WinCode.Scheduling.Requests
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