using System;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings.Interfaces
{

    public interface IMeetingDetailPresenter : IDisposable
    {
    	void UpdateView();
        void CancelAllLoads();
    }
}
