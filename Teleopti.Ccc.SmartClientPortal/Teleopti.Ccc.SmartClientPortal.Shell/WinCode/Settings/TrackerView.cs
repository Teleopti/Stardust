using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Tracking;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Settings
{
    public class TrackerView
    {
        private ITracker _tracker;

        public Description Description
        {
            get { return _tracker.Description; }
        }

        public ITracker Tracker
        {
            get { return _tracker; }
            set { _tracker = value; }
        }

        public static ITracker DefaultTracker
        {
            get { return new DefaultTracker(); }
        }

        public TrackerView(ITracker tracker)
        {
            _tracker = tracker;
        }
    }
}
