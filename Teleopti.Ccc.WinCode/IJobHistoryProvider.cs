using System.Collections.Generic;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.WinCode.Forecasting
{
    public interface IJobHistoryProvider
    {
        IList<JobResultModel> GetHistory(PagingDetail pagingDetail);
    }
}