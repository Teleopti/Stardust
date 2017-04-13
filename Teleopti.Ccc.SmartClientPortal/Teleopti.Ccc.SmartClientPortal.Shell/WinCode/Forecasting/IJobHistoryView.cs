using System.Collections.Generic;

namespace Teleopti.Ccc.WinCode.Forecasting
{
    public interface IJobHistoryView
    {
        void BindJobResultData(IEnumerable<JobResultModel> jobResultModels);
        void BindJobResultDetailData(IList<JobResultDetailModel> jobHistoryEntries);
    	void TogglePrevious(bool enabled);
    	void ToggleNext(bool enabled);
    	void SetResultDescription(string description);
		int DetailLevel { get; }
    }
}