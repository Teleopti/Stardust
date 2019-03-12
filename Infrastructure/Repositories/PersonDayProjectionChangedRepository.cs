using NHibernate.Envers;
using NHibernate.Envers.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public interface IPersonDayProjectionChangedRepository
	{
		ICollection<PersonDayAbsenceProjectionChanged> LoadPersonDayAbsenceChanges(DateTime changesFromUTC, DateTime changesToUTC, IScenario scenario);
		ICollection<PersonDayProjectionChanged> LoadPersonDayAssignmentChanges(DateTime changesFromUTC, DateTime changesToUTC, IScenario scenario);
	}

	public class PersonDayProjectionChangedRepository : IPersonDayProjectionChangedRepository
	{
		private readonly ICurrentUnitOfWork _currentUnitOfWork;

		public PersonDayProjectionChangedRepository(ICurrentUnitOfWork currentUnitOfWork)
		{
			_currentUnitOfWork = currentUnitOfWork;
		}

		public ICollection<PersonDayAbsenceProjectionChanged> LoadPersonDayAbsenceChanges(DateTime changesFromUTC, DateTime changesToUTC, IScenario scenario)
		{
			var session = _currentUnitOfWork.Current().Session();
			var absence = session.Auditer().CreateQuery()
							.ForRevisionsOf<PersonAbsence>(true)
							.Add(AuditEntity.Property("Scenario").Eq(scenario))
							.Add(AuditEntity.RevisionProperty("ModifiedAt").Gt(changesFromUTC))
							.Add(AuditEntity.RevisionProperty("ModifiedAt").Lt(changesToUTC))
							.Results();

			return absence.Select(x => new PersonDayAbsenceProjectionChanged() { PersonId = x.Person.Id.Value, Period = x.Period }).ToList();
		}

		public ICollection<PersonDayProjectionChanged> LoadPersonDayAssignmentChanges(DateTime changesFromUTC, DateTime changesToUTC, IScenario scenario)
		{
			var session = _currentUnitOfWork.Current().Session();
			var scheduleData = session.Auditer().CreateQuery()
							.ForRevisionsOf<PersonAssignment>(true)
							.Add(AuditEntity.Property("Scenario").Eq(scenario))
							.Add(AuditEntity.RevisionProperty("ModifiedAt").Gt(changesFromUTC))
							.Add(AuditEntity.RevisionProperty("ModifiedAt").Lt(changesToUTC))
							.Results();

			return scheduleData.Select(x => new PersonDayProjectionChanged(x.Person.Id.Value, x.Date, x.Date)).ToList();
		}
	}
}
