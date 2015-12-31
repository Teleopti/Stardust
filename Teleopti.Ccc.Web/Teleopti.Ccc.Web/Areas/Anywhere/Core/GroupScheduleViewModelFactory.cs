using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Core
{
	public class GroupScheduleViewModelFactory : IGroupScheduleViewModelFactory
	{
		private readonly IGroupScheduleViewModelMapper _mapper;
		private readonly ILoggedOnUser _user;
		private readonly IPersonScheduleDayReadModelFinder _personScheduleDayReadModelRepository;
		private readonly IPermissionProvider _permissionProvider;
		private readonly ISchedulePersonProvider _schedulePersonProvider;
		private readonly ICommonAgentNameProvider _commonAgentNameProvider;

		public GroupScheduleViewModelFactory(IGroupScheduleViewModelMapper mapper, ILoggedOnUser user,
			IPersonScheduleDayReadModelFinder personScheduleDayReadModelRepository, IPermissionProvider permissionProvider,
			ISchedulePersonProvider schedulePersonProvider, ICommonAgentNameProvider commonAgentNameProvider)
		{
			_mapper = mapper;
			_user = user;
			_personScheduleDayReadModelRepository = personScheduleDayReadModelRepository;
			_permissionProvider = permissionProvider;
			_schedulePersonProvider = schedulePersonProvider;
			_commonAgentNameProvider = commonAgentNameProvider;
		}

		public IEnumerable<GroupScheduleShiftViewModel> CreateViewModel(Guid groupId, DateTime dateInUserTimeZone)
		{
			var people = _schedulePersonProvider.GetPermittedPersonsForGroup(new DateOnly(dateInUserTimeZone), groupId,
				DefinedRaptorApplicationFunctionPaths.MyTeamSchedules).ToArray();
			var peopleCanSeeConfidentialAbsencesFor =
				_schedulePersonProvider.GetPermittedPersonsForGroup(new DateOnly(dateInUserTimeZone), groupId,
					DefinedRaptorApplicationFunctionPaths.ViewConfidential);

			return getScheduleViewModel(dateInUserTimeZone, people, peopleCanSeeConfidentialAbsencesFor);
		}

		private IEnumerable<GroupScheduleShiftViewModel> getScheduleViewModel(DateTime dateInUserTimeZone,
			IEnumerable<IPerson> people, IEnumerable<IPerson> peopleCanSeeConfidentialAbsencesFor)
		{
			var userTimeZone = _user.CurrentUser().PermissionInformation.DefaultTimeZone();
			var dateTimeInUtc = TimeZoneInfo.ConvertTime(dateInUserTimeZone, userTimeZone, TimeZoneInfo.Utc);

			var yesterdayIsDaylightSavingTime = userTimeZone.IsDaylightSavingTime(dateTimeInUtc.AddDays(-1));
			var todayIsDaylightSavingTime = userTimeZone.IsDaylightSavingTime(dateTimeInUtc);

			var periodLengthInHour = 24;
			// First day of day light saving time period
			if (!yesterdayIsDaylightSavingTime && todayIsDaylightSavingTime)
			{
				periodLengthInHour = 23;
			}
			// First day after day light saving time period
			else if (yesterdayIsDaylightSavingTime && !todayIsDaylightSavingTime)
			{
				periodLengthInHour = 25;
			}

			var dateTimePeriod = new DateTimePeriod(dateTimeInUtc, dateTimeInUtc.AddHours(periodLengthInHour));

			var allPeople = people.ToList();
			var emptyReadModel = new PersonScheduleDayReadModel[] {};
			var data = new GroupScheduleData
			{
				Date = dateTimeInUtc,
				CommonAgentNameSetting = _commonAgentNameProvider.CommonAgentNameSettings,
				UserTimeZone = _user.CurrentUser().PermissionInformation.DefaultTimeZone(),
				Schedules =
					allPeople.Any()
						? _personScheduleDayReadModelRepository.ForPeople(dateTimePeriod,
							allPeople.Select(x => x.Id.GetValueOrDefault()).ToArray()) ?? emptyReadModel
						: emptyReadModel,
				CanSeeUnpublishedSchedules =
					_permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.ViewUnpublishedSchedules),
				CanSeeConfidentialAbsencesFor = peopleCanSeeConfidentialAbsencesFor,
				CanSeePersons = allPeople
			};
			return _mapper.Map(data);
		}
	}
}
