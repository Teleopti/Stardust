﻿using System.Collections.Generic;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.WinCode.Forecasting
{
    public interface IJobResultProvider
    {
        IList<JobResultModel> GetJobResults(PagingDetail pagingDetail);
        IList<JobResultDetailModel> GetJobResultDetails(JobResultModel jobResultModel, bool showInfo);
    }
}