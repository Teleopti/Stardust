using Microsoft.Practices.Composite.Events;

namespace Teleopti.Ccc.WinCode.Events
{
    public interface IEventAggregatorLocator
    {
        IEventAggregator GlobalAggregator();
        IEventAggregator LocalAggregator();
    }
}