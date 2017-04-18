using System;
using Microsoft.Practices.Composite.Presentation.Events;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Grouping.Events
{
    public class GroupPageAdded : CompositePresentationEvent<EventGroupPage>
    {
        
    }

    public class EventGroupPage
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }
}