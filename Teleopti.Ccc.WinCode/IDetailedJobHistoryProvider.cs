using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Teleopti.Ccc.WinCode.Forecasting
{
    public interface IDetailedJobHistoryProvider
    {
        IList<DetailedJobHistoryResultModel> GetHistory(JobResultModel jobResultModel);
    }
}
