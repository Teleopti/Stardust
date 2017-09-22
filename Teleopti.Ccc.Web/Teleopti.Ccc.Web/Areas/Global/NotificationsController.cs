using System;
using System.Linq;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Web.Areas.Global
{
	public class NotificationsController : ApiController
	{
		private readonly IJobResultRepository _jobResultRepository;
		private readonly IUserTimeZone _timeZone;

		public NotificationsController(IJobResultRepository jobResultRepository, IUserTimeZone timeZone)
		{
			_jobResultRepository = jobResultRepository;
			_timeZone = timeZone;
		}

		[UnitOfWork]
		[HttpGet, Route("api/notifications")]
		public virtual IHttpActionResult GetNotifications()
		{
			//only getting the jobnotificatiosn for now but this module will group the nofications
			var loadedJobs =
				_jobResultRepository.LoadHistoryWithPaging(new PagingDetail {Skip = 0, Take = 5, TotalNumberOfResults = 5},
					JobCategory.QuickForecast, JobCategory.ForecastsImport, JobCategory.MultisiteExport);
			var result = loadedJobs.Select(
				x =>
					new JobResultNotificationModel
					{
						Status = determineStatus(x),
						JobCategory = x.JobCategory,
						Owner = x.Owner.Name.ToString(),
						Timestamp = TimeZoneInfo.ConvertTimeFromUtc(x.Timestamp, _timeZone.TimeZone())
					}).OrderByDescending(x=>x.Timestamp).ToArray();
			return Ok(result);
		}

		private static string determineStatus(IJobResult jobResult)
		{
			if (jobResult.HasError())
				return UserTexts.Resources.Error;
			if (jobResult.IsWorking())
				return UserTexts.Resources.WorkingThreeDots;
			return jobResult.FinishedOk ? UserTexts.Resources.Done : UserTexts.Resources.WaitingThreeDots;
		}
	}

	public class JobResultNotificationModel
	{
		public string JobCategory { get; set; }
		public virtual string Owner { get; set; }
		public virtual DateTime Timestamp { get; set; }
		public virtual string Status { get; set; }
	}
}