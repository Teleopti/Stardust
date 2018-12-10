using System;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public class DateScenarioPersonId
	{
		private readonly DateOnly _date;
		private readonly Guid _scenarioId;
		private readonly Guid _personId;

		public DateScenarioPersonId(Guid id, DateTime date, Guid scenarioId, Guid personId, int version)
		{
			Id = id;
			_date = new DateOnly(date);
			_scenarioId = scenarioId;
			_personId = personId;
			Version = version;
		}

		public Guid Id { get; private set; }
		public int Version { get; private set; }

		public bool EqualWith(IPersonAssignment assignment)
		{
			return _personId.Equals(assignment.Person.Id.Value) &&
			       _date.Equals(assignment.Date) &&
			       _scenarioId.Equals(assignment.Scenario.Id.Value);
		}
	}
}