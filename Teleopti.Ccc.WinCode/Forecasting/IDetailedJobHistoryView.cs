using System;
using System.Collections.Generic;
using Teleopti.Ccc.WinCode.Forecasting;

namespace Teleopti.Ccc.Win.Forecasting
{
    public interface IDetailedJobHistoryView
    {
        Guid? JobId { get; set; }
        String JobType { get; set; }
        void BindData(IList<DetailedJobHistoryResultModel> jobHistoryEntries);
    }
}
