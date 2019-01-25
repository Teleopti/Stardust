using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Forecasting
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
							Timestamp = TimeZoneHelper.ConvertFromUtc(j.Timestamp, TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal.Regional.TimeZone).ToString()
						}).ToList();
			}
		}
		
		public IList<JobResultDetailModel> GetJobResultDetails(JobResultModel jobResultModel, int detailLevel)
        {
	        using (_unitOfWorkFactory.CreateAndOpenUnitOfWork())
            {
                var jobResult = _jobResultRepository.Get(jobResultModel.JobId.GetValueOrDefault());

				var timeZone = TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal.Regional.TimeZone;
				return jobResult.Details.Where(d => (int)d.DetailLevel > detailLevel).OrderByDescending(d => d.Timestamp).Select(m => new JobResultDetailModel
				{
					Message = m.Message,
					Timestamp = TimeZoneHelper.ConvertFromUtc(m.Timestamp, timeZone),
					ExceptionMessage = m.ExceptionMessage,
					ExceptionStackTrace = m.ExceptionStackTrace,
					InnerExceptionMessage = m.InnerExceptionMessage,
					InnerExceptionStackTrace = m.InnerExceptionStackTrace
				}).ToList();
            }
        }

		private static string determineStatus(IJobResult jobResult)
		{
            if (jobResult.HasError())
                return UserTexts.Resources.Error;
            if (jobResult.IsWorking())
                return UserTexts.Resources.WorkingThreeDots;
            if (jobResult.FinishedOk)
				return UserTexts.Resources.Done;
			return UserTexts.Resources.WaitingThreeDots;
		}
	}
}