using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCode.Forecasting
{
	public class JobResultProvider : IJobResultProvider
	{
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IJobResultRepository _jobResultRepository;

		public JobResultProvider(IUnitOfWorkFactory unitOfWorkFactory, IJobResultRepository jobResultRepository)
		{
			_unitOfWorkFactory = unitOfWorkFactory;
			_jobResultRepository = jobResultRepository;
		}

		public IList<JobResultModel> GetJobResults(PagingDetail pagingDetail)
		{
			using (_unitOfWorkFactory.CreateAndOpenUnitOfWork())
			{
				var jobResult = _jobResultRepository.LoadHistoryWithPaging(pagingDetail, JobCategory.QuickForecast,
																		   JobCategory.MultisiteExport, JobCategory.ForecastsImport);
				return
					jobResult.Select(
						j =>
						new JobResultModel
						{
							JobId = j.Id,
                            JobCategory = j.JobCategory,
							Owner = j.Owner.Name.ToString(),
							Status = determineStatus(j),
							Timestamp = TimeZoneHelper.ConvertFromUtc(j.Timestamp).ToString()
						}).ToList();
			}
		}

        public IList<JobResultDetailModel> GetJobResultDetails(JobResultModel jobResultModel)
        {
            using (_unitOfWorkFactory.CreateAndOpenUnitOfWork())
            {
                var jobResult = _jobResultRepository.Get(jobResultModel.JobId.GetValueOrDefault());

                return jobResult.Details.Where(d => d.DetailLevel != DetailLevel.Info).OrderByDescending(d => d.Timestamp).Select(m =>
                                                                                              new JobResultDetailModel
                                                                                                  {
                                                                                                      Message =
                                                                                                          m.Message,
                                                                                                      Timestamp =
                                                                                                          TimeZoneHelper.ConvertFromUtc(m.Timestamp),
                                                                                                      ExceptionMessage =
                                                                                                          m.ExceptionMessage,
                                                                                                      ExceptionStackTrace=
                                                                                                          m.ExceptionStackTrace,
                                                                                                      InnerExceptionMessage=
                                                                                                          m.InnerExceptionMessage,
                                                                                                      InnerExceptionStackTrace=
                                                                                                          m.InnerExceptionStackTrace
                                                                                                  }).ToList();
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