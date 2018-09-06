using System.Linq;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Wfm.Api.Query
{
	public class AllActivitiesHandler : IQueryHandler<AllActivitiesDto,ActivityDto>
	{
		private readonly IActivityRepository _activityRepository;

		public AllActivitiesHandler(IActivityRepository activityRepository)
		{
			_activityRepository = activityRepository;
		}

		[UnitOfWork]
		public virtual QueryResultDto<ActivityDto> Handle(AllActivitiesDto query)
		{
			var activites = _activityRepository.LoadAll();
			return new QueryResultDto<ActivityDto>
			{
				Successful = true,
				Result = activites.Select(x => new ActivityDto
				{
					Id = x.Id.GetValueOrDefault(),
					Name = x.Name,
					AllowOverwrite = x.AllowOverwrite,
					InPaidTime = x.InPaidTime,
					InReadyTime = x.InReadyTime,
					InWorkTime = x.InWorkTime,
					IsOutboundActivity = x.IsOutboundActivity,
					PayrollCode = x.PayrollCode,
					ReportLevelDetail = x.ReportLevelDetail.ToString(),
					RequiresSeat = x.RequiresSeat,
					RequiresSkill = x.RequiresSkill
				})
			};
		}
	}
}