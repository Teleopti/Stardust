using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Meetings.Overview
{
    public interface IMeetingClipboardHandler
    {
        void SetData(IMeeting meeting);
        IMeeting GetData();
        bool HasData();
    }

    public class MeetingClipboardHandler : IMeetingClipboardHandler
    {
        private IMeeting _data;
        public void SetData(IMeeting meeting)
        {
            _data = meeting;
        }

        public IMeeting GetData()
        {
            return _data.NoneEntityClone();
        }

        public bool HasData()
        {
            return _data != null;
        }
    }
}