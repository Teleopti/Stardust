using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer
{
	public class AddFullDayAbsenceCommandHandler : IHandleCommand<AddFullDayAbsenceCommand>
	{
		private readonly ICurrentDataSource _dataSource;
		private readonly ICurrentScenario _scenario;
		private readonly IWriteSideRepository<IPerson> _personRepository;
		private readonly IWriteSideRepository<IAbsence> _absenceRepository;
		private readonly IWriteSideRepository<IPersonAbsence> _personAbsenceRepository;
		private readonly IPersonScheduleDayReadModelFinder _personScheduleDayReadModelRepository;
		private readonly IJsonDeserializer _deserializer;

		public AddFullDayAbsenceCommandHandler(ICurrentDataSource dataSource, ICurrentScenario scenario, IWriteSideRepository<IPerson> personRepository, IWriteSideRepository<IAbsence> absenceRepository, IWriteSideRepository<IPersonAbsence> personAbsenceRepository, IPersonScheduleDayReadModelFinder personScheduleDayReadModelRepository, IJsonDeserializer deserializer)
		{
			_dataSource = dataSource;
			_scenario = scenario;
			_personRepository = personRepository;
			_absenceRepository = absenceRepository;
			_personAbsenceRepository = personAbsenceRepository;
			_personScheduleDayReadModelRepository = personScheduleDayReadModelRepository;
			_deserializer = deserializer;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public void Handle(AddFullDayAbsenceCommand command)
		{
			var person = _personRepository.Load(command.PersonId);
			var absence = _absenceRepository.Load(command.AbsenceId);

			var personAbsence = new PersonAbsence(_scenario.Current());

			var shiftEndOnDayBeforeStartDate = getUnderlyingShiftEnd(command.StartDate.AddDays(-1), person);
			var shiftEndOnEndDate = getUnderlyingShiftEnd(command.EndDate, person);

			personAbsence.FullDayAbsence(_dataSource.CurrentName(), person, absence, command.StartDate, command.EndDate, shiftEndOnDayBeforeStartDate, shiftEndOnEndDate);

			_personAbsenceRepository.Add(personAbsence);
		}

		private DateTime getUnderlyingShiftEnd(DateTime date, IPerson person)
		{
			var personScheduleDayReadModel =
				person.Id.HasValue
					? _personScheduleDayReadModelRepository.ForPerson(new DateOnly(date), person.Id.Value)
					: null;
			var shiftEnd = DateTime.MinValue;
			if (personScheduleDayReadModel != null && personScheduleDayReadModel.Shift != null)
			{
				dynamic shift =
					_deserializer.DeserializeObject(personScheduleDayReadModel.Shift);

				if (shift != null && shift.HasUnderlyingShift)
				{
					var projection = shift.Projection as IEnumerable<dynamic>;
					if (projection != null && !projection.IsEmpty())
					{
						shiftEnd = projection.Max(p => (DateTime) p.End);
					}
				}
			}
			return shiftEnd;
		}
	}
}