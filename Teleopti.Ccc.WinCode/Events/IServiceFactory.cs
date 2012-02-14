using Microsoft.Practices.Composite.Events;

namespace Teleopti.Ccc.WinCode.Events
{
    /// <summary>
    /// Provides services
    /// </summary>
    public interface IServiceFactory
    {
        IEventAggregator EventService { get; }
    }
}
