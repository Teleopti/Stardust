using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Core
{
	public class TeamScheduleViewModelFactory : ITeamScheduleViewModelFactory
	{
		private readonly ITeamScheduleViewModelMapper _mapper;
		private readonly ILoggedOnUser _user;
		private readonly IPersonScheduleDayReadModelFinder _personScheduleDayReadModelRepository;
		private readonly IPermissionProvider _permissionProvider;
		private readonly ISchedulePersonProvider _schedulePersonProvider;

		public TeamScheduleViewModelFactory(ITeamScheduleViewModelMapper mapper, ILoggedOnUser user, IPersonScheduleDayReadModelFinder personScheduleDayReadModelRepository, IPermissionProvider permissionProvider, ISchedulePersonProvider schedulePersonProvider)
		{
			_mapper = mapper;
			_user = user;
			_personScheduleDayReadModelRepository = personScheduleDayReadModelRepository;
			_permissionProvider = permissionProvider;
			_schedulePersonProvider = schedulePersonProvider;
		}

		public IEnumerable<TeamScheduleShiftViewModel> CreateViewModel(Guid teamId, DateTime date)
		{
			date = new DateTime(date.Year, date.Month, date.Day, 0, 0, 0, DateTimeKind.Utc);
			var dateTimePeriod = new DateTimePeriod(date, date.AddHours(25));
			var data = new TeamScheduleData
				{
					Date = date,
					UserTimeZone = _user.CurrentUser().PermissionInformation.DefaultTimeZone(),
					Schedules = _personScheduleDayReadModelRepository.ForTeam(dateTimePeriod, teamId) ?? new PersonScheduleDayReadModel[] {},
					CanSeeUnpublishedSchedules = _permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.ViewUnpublishedSchedules),
					CanSeeConfidentialAbsencesFor = _schedulePersonProvider.GetPermittedPersonsForTeam(new DateOnly(date), teamId, DefinedRaptorApplicationFunctionPaths.ViewConfidential),
					CanSeePersons = _schedulePersonProvider.GetPermittedPersonsForTeam(new DateOnly(date), teamId, DefinedRaptorApplicationFunctionPaths.SchedulesAnywhere)
				};
			return _mapper.Map(data);
		}
	}
}




