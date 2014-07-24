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

		public GroupScheduleViewModelFactory(IGroupScheduleViewModelMapper mapper, ILoggedOnUser user, IPersonScheduleDayReadModelFinder personScheduleDayReadModelRepository, IPermissionProvider permissionProvider, ISchedulePersonProvider schedulePersonProvider, ICommonAgentNameProvider commonAgentNameProvider)
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
			var dateTimeInUtc= TimeZoneInfo.ConvertTime(dateInUserTimeZone, _user.CurrentUser().PermissionInformation.DefaultTimeZone(), TimeZoneInfo.Utc);
			var dateTimePeriod = new DateTimePeriod(dateTimeInUtc, dateTimeInUtc.AddHours(25));
			var people = _schedulePersonProvider.GetPermittedPersonsForGroup(new DateOnly(dateInUserTimeZone), groupId,
			                                                                 DefinedRaptorApplicationFunctionPaths.
																				 MyTeamSchedules).ToArray();
			var emptyReadModel = new PersonScheduleDayReadModel[] {};
			var data = new GroupScheduleData
				{
					Date = dateTimeInUtc,
					CommonAgentNameSetting = _commonAgentNameProvider.CommonAgentNameSettings,
					UserTimeZone = _user.CurrentUser().PermissionInformation.DefaultTimeZone(),
					Schedules = people.Length == 0 ? emptyReadModel : _personScheduleDayReadModelRepository.ForPeople(dateTimePeriod, people.Select(x => x.Id.GetValueOrDefault()).ToArray()) ?? emptyReadModel,
					CanSeeUnpublishedSchedules =
						_permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.ViewUnpublishedSchedules),
					CanSeeConfidentialAbsencesFor =
						_schedulePersonProvider.GetPermittedPersonsForGroup(new DateOnly(dateInUserTimeZone), groupId,
						                                                    DefinedRaptorApplicationFunctionPaths.ViewConfidential),
					CanSeePersons = people
				};
			return _mapper.Map(data);
		}
	}
}




