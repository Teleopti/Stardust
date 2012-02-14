using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Meetings.Events
{
    public class ClipboardStatusEventModel
    {
        public ClipboardAction ClipboardAction { get; set; }

        public bool Enabled { get; set; }
    }
}