using Microsoft.Practices.Composite.Events;

namespace Teleopti.Ccc.WinCode.Events
{
    public static class EventExtensions
    {
        //Supplying event broking mechanizm to each object in the application.
        public static void PublishEvent<T>(this T eventArgs, string eventTopic,IEventAggregator eventAggregator)
        {
            eventAggregator.GetEvent<GenericEvent<T>>()
                  .Publish(new EventParameters<T> { Topic = eventTopic, Value = eventArgs });
        }
    }
}
