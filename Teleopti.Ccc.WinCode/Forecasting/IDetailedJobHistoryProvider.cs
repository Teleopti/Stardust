using System.Collections.Generic;

namespace Teleopti.Ccc.WinCode.Forecasting
{
    public interface IDetailedJobHistoryProvider
    {
        IList<DetailedJobHistoryResultModel> GetHistory(JobResultModel jobResultModel);
    }
}
