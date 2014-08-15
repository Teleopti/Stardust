using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Ccc.Web.Core;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Core
{
	public class PersonScheduleViewModelFactory : IPersonScheduleViewModelFactory
	{
		private readonly IPersonRepository _personRepository;
		private readonly IPersonScheduleDayReadModelFinder _personScheduleDayReadModelRepository;
		private readonly IAbsenceRepository _absenceRepository;
		private readonly IActivityRepository _activityRepository;
		private readonly IPersonScheduleViewModelMapper _personScheduleViewModelMapper;
		private readonly IPersonAbsenceRepository _personAbsenceRepository;
		private readonly IJsonDeserializer _deserializer;
		private readonly IPermissionProvider _permissionProvider;
		private readonly ICommonAgentNameProvider _commonAgentNameProvider;
	    private readonly IIanaTimeZoneProvider _ianaTimeZoneProvider;

	    public PersonScheduleViewModelFactory(IPersonRepository personRepository, IPersonScheduleDayReadModelFinder personScheduleDayReadModelRepository, IAbsenceRepository absenceRepository, IActivityRepository activityRepository, IPersonScheduleViewModelMapper personScheduleViewModelMapper, IPersonAbsenceRepository personAbsenceRepository, IJsonDeserializer deserializer, IPermissionProvider permissionProvider, ICommonAgentNameProvider commonAgentNameProvider, IIanaTimeZoneProvider ianaTimeZoneProvider)
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
                IanaTimeZoneOther = _ianaTimeZoneProvider.WindowsToIana(person.PermissionInformation.DefaultTimeZone().Id)
			};

			if (_permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.ViewUnpublishedSchedules)
				|| new SchedulePublishedSpecification(person.WorkflowControlSet, ScheduleVisibleReasons.Published).IsSatisfiedBy(new DateOnly(date)))
			{
				var personScheduleDayReadModel = _personScheduleDayReadModelRepository.ForPerson(new DateOnly(date), personId);
				data.PersonAbsences = calculatePersonAbsences(date, person, personScheduleDayReadModel);
				if (personScheduleDayReadModel != null && personScheduleDayReadModel.Model != null)
					data.Model = _deserializer.DeserializeObject<Model>(personScheduleDayReadModel.Model);
			}
			return _personScheduleViewModelMapper.Map(data);

		}

		private IEnumerable<IPersonAbsence> calculatePersonAbsences(DateTime date, IPerson person,
													IPersonScheduleDayReadModel personScheduleDayReadModel)
		{
			var previousDayReadModel = _personScheduleDayReadModelRepository.ForPerson(new DateOnly(date).AddDays(-1), person.Id.Value);
			var start = TimeZoneInfo.ConvertTimeToUtc(date, person.PermissionInformation.DefaultTimeZone());
			var end = TimeZoneInfo.ConvertTimeToUtc(date.AddHours(24), person.PermissionInformation.DefaultTimeZone());

			if (personScheduleDayReadModel != null && personScheduleDayReadModel.Start.HasValue)
				start = DateTime.SpecifyKind(personScheduleDayReadModel.Start.Value, DateTimeKind.Utc);
			if (previousDayReadModel != null && previousDayReadModel.End.HasValue &&
				previousDayReadModel.End.Value > start)
				start = DateTime.SpecifyKind(previousDayReadModel.End.Value, DateTimeKind.Utc);
			if (personScheduleDayReadModel != null && personScheduleDayReadModel.End.HasValue)
				end = DateTime.SpecifyKind(personScheduleDayReadModel.End.Value, DateTimeKind.Utc);

			var absencePeriod = new DateTimePeriod(start, end);
			return _personAbsenceRepository.Find(new[] { person }, absencePeriod);
		}
	}
}