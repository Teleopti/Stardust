﻿using System;
using System.Collections.Generic;
using System.Dynamic;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Core
{
	public class PersonScheduleViewModelFactory : IPersonScheduleViewModelFactory
	{
		private readonly IPersonRepository _personRepository;
		private readonly IPersonScheduleDayReadModelFinder _personScheduleDayReadModelRepository;
		private readonly IAbsenceRepository _absenceRepository;
		private readonly IPersonScheduleViewModelMapper _personScheduleViewModelMapper;
		private readonly IPersonAbsenceRepository _personAbsenceRepository;
		private readonly IJsonDeserializer _deserializer;
		private readonly IPermissionProvider _permissionProvider;

		public PersonScheduleViewModelFactory(IPersonRepository personRepository,
											  IPersonScheduleDayReadModelFinder personScheduleDayReadModelRepository,
											  IAbsenceRepository absenceRepository,
											  IPersonScheduleViewModelMapper personScheduleViewModelMapper,
											  IPersonAbsenceRepository personAbsenceRepository,
											  IJsonDeserializer deserializer,
											  IPermissionProvider permissionProvider)
		{
			_personRepository = personRepository;
			_personScheduleDayReadModelRepository = personScheduleDayReadModelRepository;
			_absenceRepository = absenceRepository;
			_personScheduleViewModelMapper = personScheduleViewModelMapper;
			_personAbsenceRepository = personAbsenceRepository;
			_deserializer = deserializer;
			_permissionProvider = permissionProvider;
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
				Absences = _absenceRepository.LoadAllSortByName(),
				HasViewConfidentialPermission = hasViewConfidentialPermission
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

			if (personScheduleDayReadModel != null && personScheduleDayReadModel.ShiftStart.HasValue)
				start = DateTime.SpecifyKind(personScheduleDayReadModel.ShiftStart.Value, DateTimeKind.Utc);
			if (previousDayReadModel != null && previousDayReadModel.ShiftEnd.HasValue &&
				previousDayReadModel.ShiftEnd.Value > start)
				start = DateTime.SpecifyKind(previousDayReadModel.ShiftEnd.Value, DateTimeKind.Utc);
			if (personScheduleDayReadModel != null && personScheduleDayReadModel.ShiftEnd.HasValue)
				end = DateTime.SpecifyKind(personScheduleDayReadModel.ShiftEnd.Value, DateTimeKind.Utc);

			var absencePeriod = new DateTimePeriod(start, end);
			return _personAbsenceRepository.Find(new[] { person }, absencePeriod);
		}
	}
}