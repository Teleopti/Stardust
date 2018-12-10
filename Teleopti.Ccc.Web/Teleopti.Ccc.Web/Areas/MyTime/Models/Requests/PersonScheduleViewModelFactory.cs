using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.Web.Core;


namespace Teleopti.Ccc.Web.Areas.MyTime.Models.Requests
{
	public class PersonScheduleViewModelFactory : IPersonScheduleViewModelFactory
	{
		private readonly IPersonRepository _personRepository;
		private readonly IPersonScheduleDayReadModelFinder _personScheduleDayReadModelRepository;
		private readonly IAbsenceRepository _absenceRepository;
		private readonly IActivityRepository _activityRepository;
		private readonly PersonScheduleViewModelMapper _personScheduleViewModelMapper;
		private readonly IPersonAbsenceRepository _personAbsenceRepository;
		private readonly IJsonDeserializer _deserializer;
		private readonly IPermissionProvider _permissionProvider;
		private readonly ICommonAgentNameProvider _commonAgentNameProvider;
	    private readonly IIanaTimeZoneProvider _ianaTimeZoneProvider;
		private readonly IUserTimeZone _userTimeZone;
		private readonly ICurrentScenario _scenarioRepository;

		public PersonScheduleViewModelFactory(IPersonRepository personRepository, IPersonScheduleDayReadModelFinder personScheduleDayReadModelRepository, IAbsenceRepository absenceRepository, IActivityRepository activityRepository, PersonScheduleViewModelMapper personScheduleViewModelMapper, IPersonAbsenceRepository personAbsenceRepository, IJsonDeserializer deserializer, IPermissionProvider permissionProvider, ICommonAgentNameProvider commonAgentNameProvider, IIanaTimeZoneProvider ianaTimeZoneProvider, IUserTimeZone userTimeZone, ICurrentScenario scenarioRepository)
		{
			_personRepository = personRepository;
			_personScheduleDayReadModelRepository = personScheduleDayReadModelRepository;
			_absenceRepository = absenceRepository;
			_activityRepository = activityRepository;
			_personScheduleViewModelMapper = personScheduleViewModelMapper;
			_personAbsenceRepository = personAbsenceRepository;
			_deserializer = deserializer;
			_permissionProvider = permissionProvider;
			_commonAgentNameProvider = commonAgentNameProvider;
		    _ianaTimeZoneProvider = ianaTimeZoneProvider;
		    _userTimeZone = userTimeZone;
			_scenarioRepository = scenarioRepository;
		}

		public PersonScheduleViewModel CreateViewModel(Guid personId, DateTime date)
		{
			var person = _personRepository.Get(personId);
			var hasViewConfidentialPermission = _permissionProvider.HasPersonPermission(DefinedRaptorApplicationFunctionPaths.ViewConfidential,
																				  DateOnly.Today, person);
			var data = new PersonScheduleData
			{
				Person = person,
				Date = date,
				CommonAgentNameSetting = _commonAgentNameProvider.CommonAgentNameSettings,
				Absences = _absenceRepository.LoadAllSortByName(),
				Activities = _activityRepository.LoadAllSortByName(),
				HasViewConfidentialPermission = hasViewConfidentialPermission,
                IanaTimeZoneOther = _ianaTimeZoneProvider.WindowsToIana(person.PermissionInformation.DefaultTimeZone().Id),
				IanaTimeZoneLoggedOnUser = _ianaTimeZoneProvider.WindowsToIana(_userTimeZone.TimeZone().Id)
			};

			if (_permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.ViewUnpublishedSchedules)
				|| new SchedulePublishedSpecification(person.WorkflowControlSet, ScheduleVisibleReasons.Published).IsSatisfiedBy(new DateOnly(date)))
			{
				var personScheduleDayReadModel = _personScheduleDayReadModelRepository.ForPerson(new DateOnly(date), personId);
				data.PersonAbsences = calculatePersonAbsences(date, person, personScheduleDayReadModel);
				if (personScheduleDayReadModel?.Model != null)
					data.Model = _deserializer.DeserializeObject<Model>(personScheduleDayReadModel.Model);
			}
			return _personScheduleViewModelMapper.Map(data);

		}

		private IEnumerable<IPersonAbsence> calculatePersonAbsences(DateTime date, IPerson person,
													IPersonScheduleDayReadModel personScheduleDayReadModel)
		{
			var previousDayReadModel = _personScheduleDayReadModelRepository.ForPerson(new DateOnly(date).AddDays(-1), person.Id.Value);
			var start = TimeZoneHelper.ConvertToUtc(date, person.PermissionInformation.DefaultTimeZone());
			var end = TimeZoneHelper.ConvertToUtc(date.AddHours(24), person.PermissionInformation.DefaultTimeZone());

			if (personScheduleDayReadModel != null && personScheduleDayReadModel.Start.HasValue)
				start = DateTime.SpecifyKind(personScheduleDayReadModel.Start.Value, DateTimeKind.Utc);
			if (previousDayReadModel != null && previousDayReadModel.End.HasValue &&
				previousDayReadModel.End.Value > start)
				start = DateTime.SpecifyKind(previousDayReadModel.End.Value, DateTimeKind.Utc);
			if (personScheduleDayReadModel != null && personScheduleDayReadModel.End.HasValue)
				end = DateTime.SpecifyKind(personScheduleDayReadModel.End.Value, DateTimeKind.Utc);

			var absencePeriod = new DateTimePeriod(start, end);
			var scenario = _scenarioRepository.Current();
			return _personAbsenceRepository.Find(new[] { person }, absencePeriod, scenario);
		}
	}
}