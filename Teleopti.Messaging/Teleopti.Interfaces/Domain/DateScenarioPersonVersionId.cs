using System;

namespace Teleopti.Interfaces.Domain
{
	public class DateScenarioPersonVersionId
	{
		public DateScenarioPersonVersionId(Guid id, DateOnly date, IScenario scenario, IPerson person, int version)
		{
			Id = id;
			Date = date;
			Scenario = scenario;
			Person = person;
			Version = version;
		}
		public int Version { get; private set; }
		public Guid Id { get; private set; }
		public DateOnly Date { get; private set; }
		public IScenario Scenario { get; private set; }
		public IPerson Person { get; private set; }

		public bool EqualWith(IPersonAssignment assignment)
		{
			return Person.Equals(assignment.Person) &&
			       Date.Equals(assignment.Date) &&
			       Scenario.Equals(assignment.Scenario);
		}
	}
}