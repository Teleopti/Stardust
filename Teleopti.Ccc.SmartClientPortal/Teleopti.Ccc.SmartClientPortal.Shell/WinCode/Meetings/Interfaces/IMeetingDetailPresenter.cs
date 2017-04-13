using System;

namespace Teleopti.Ccc.WinCode.Meetings.Interfaces
{

    public interface IMeetingDetailPresenter : IDisposable
    {
    	void UpdateView();
        void CancelAllLoads();
    }
}
