using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCode.Forecasting
{
    public class DetailedJobHistoryProvider : IDetailedJobHistoryProvider
    {
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IJobResultRepository _jobResultRepository;

        public DetailedJobHistoryProvider(IUnitOfWorkFactory unitOfWorkFactory, IJobResultRepository jobResultRepository)
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
                
                foreach (var temp in jobResult.Details.Reverse())
                {
                    if (temp.DetailLevel == DetailLevel.Warning || temp.DetailLevel == DetailLevel.Error)
                    {
                        var jobResultDetails = new DetailedJobHistoryResultModel
                                                   {
                                                       Message = temp.Message,
                                                       TimeStamp = temp.Timestamp,
                                                       ExceptionMessage = temp.ExceptionMessage,
                                                       ExceptionStackTrace = temp.ExceptionStackTrace,
                                                       InnerExceptionMessage = temp.InnerExceptionMessage,
                                                       InnerExceptionStackTrace = temp.InnerExceptionStackTrace
                                                   };

                        jobResultList.Add(jobResultDetails);
                    }
                }
                return jobResultList.ToList();
            }
        }
    }
}
