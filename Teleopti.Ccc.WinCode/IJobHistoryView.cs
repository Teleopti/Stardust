using System.Collections.Generic;

namespace Teleopti.Ccc.WinCode.Forecasting
{
    public interface IJobHistoryView
    {
        void BindData(IEnumerable<JobResultModel> jobResultModels);
    	void TogglePrevious(bool enabled);
    	void ToggleNext(bool enabled);
    	void SetResultDescription(string description);
    }
}