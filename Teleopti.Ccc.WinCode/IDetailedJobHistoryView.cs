using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Teleopti.Ccc.WinCode.Forecasting
{
    public interface IDetailedJobHistoryView
    {
        Guid? JobId { get; set; }
        String JobType { get; set; }
        void BindData(IList<DetailedJobHistoryResultModel> jobHistoryEntries);
    }
}
