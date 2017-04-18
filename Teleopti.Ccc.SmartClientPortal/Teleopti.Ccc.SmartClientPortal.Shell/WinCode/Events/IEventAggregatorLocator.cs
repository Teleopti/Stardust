using Microsoft.Practices.Composite.Events;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Events
{
    public interface IEventAggregatorLocator
    {
        IEventAggregator GlobalAggregator();
        IEventAggregator LocalAggregator();
    }
}