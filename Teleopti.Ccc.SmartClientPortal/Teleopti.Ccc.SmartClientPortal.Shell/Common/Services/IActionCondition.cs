using Microsoft.Practices.CompositeUI;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Common.Services
{
    public interface IActionCondition
    {
        bool CanExecute(string action, WorkItem context, object caller, object target);
    }
}