using System.Collections.Generic;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Forecasting
{
    public interface IJobResultProvider
    {
        IList<JobResultModel> GetJobResults(PagingDetail pagingDetail);
        IList<JobResultDetailModel> GetJobResultDetails(JobResultModel jobResultModel, int detailLevel);
    }
}