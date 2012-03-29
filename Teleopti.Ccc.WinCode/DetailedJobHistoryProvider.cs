using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCode.Forecasting
{
    public class DetailedJobHistoryProvider : IDetailedJobHistoryProvider
    {
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IJobResultRepository _jobResultRepository;

        public DetailedJobHistoryProvider(IUnitOfWorkFactory unitOfWorkFactory, JobResultRepository jobResultRepository)
		{
			_unitOfWorkFactory = unitOfWorkFactory;
			_jobResultRepository = jobResultRepository;
		}
        public IList<DetailedJobHistoryResultModel> GetHistory(JobResultModel jobResultModel)
        {
            using (_unitOfWorkFactory.CreateAndOpenUnitOfWork())
            {
                var jobResult = _jobResultRepository.Get(jobResultModel.JobId.GetValueOrDefault());

                var jobResultList = new List<DetailedJobHistoryResultModel>();
                
                foreach (var temp in jobResult.Details)
                {
                    var jobResultDetails = new DetailedJobHistoryResultModel();
                    jobResultDetails.Message = temp.Message;
                    jobResultDetails.TimeStamp = temp.Timestamp;
                    jobResultDetails.ExceptionMessage = temp.ExceptionMessage;
                    jobResultDetails.ExceptionStackTrace = temp.ExceptionStackTrace;
                    jobResultDetails.InnerExceptionMessage = temp.InnerExceptionMessage;
                    jobResultDetails.InnerExceptionStackTrace = temp.InnerExceptionStackTrace;

                    jobResultList.Add(jobResultDetails);
                }

                return jobResultList.ToList();
            }
        }

        private static string determineStatus(IJobResult jobResult)
        {
            if (jobResult.FinishedOk)
                return UserTexts.Resources.Done;
            if (jobResult.HasError())
                return UserTexts.Resources.Error;
            if (jobResult.IsWorking())
                return UserTexts.Resources.WorkingThreeDots;
            return UserTexts.Resources.WaitingThreeDots;
        }
    }
}
