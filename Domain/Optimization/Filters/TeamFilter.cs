using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.Filters
{
	public class TeamFilter : IFilter
	{
		private readonly ITeam _team;

		public TeamFilter(ITeam team)
		{
			_team = team;
		}

		public bool IsValidFor(IPerson person, DateOnly dateOnly)
		{
			var personPeriod = person.Period(dateOnly);
			return personPeriod != null && person.Period(dateOnly).Team.Equals(_team);
		}

		public string FilterType
		{
			get { return "organization"; }
		}
	}
}