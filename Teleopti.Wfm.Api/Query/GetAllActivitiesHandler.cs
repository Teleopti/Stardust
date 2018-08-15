using System.Linq;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Wfm.Api.Query
{
	public class GetAllActivitiesHandler : IQueryHandler<GetAllActivitiesDto,ActivityDto>
	{
		private readonly IActivityRepository _activityRepository;

		public GetAllActivitiesHandler(IActivityRepository activityRepository)
		{
			_activityRepository = activityRepository;
		}

		[UnitOfWork]
		public virtual QueryResultDto<ActivityDto> Handle(GetAllActivitiesDto command)
		{
			var activites = _activityRepository.LoadAll();
			return new QueryResultDto<ActivityDto>
			{
				Successful = true,
				Result = activites.Select(x => new ActivityDto
				{
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