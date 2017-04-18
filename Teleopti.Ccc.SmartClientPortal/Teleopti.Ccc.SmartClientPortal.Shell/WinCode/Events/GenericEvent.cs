using Microsoft.Practices.Composite.Presentation.Events;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Events
{
    public class GenericEvent<TValue> : CompositePresentationEvent<EventParameters<TValue>> { }

    public class EventParameters<TValue>
    {
        public string Topic { get; set; }
        public TValue Value { get; set; }
    }
}
