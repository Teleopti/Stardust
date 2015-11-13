using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.Filters
{
	public class TeamFilter : Entity, IFilter
	{
		protected TeamFilter()
		{
		}

		public TeamFilter(ITeam team)
		{
			Team = team;
		}

		public virtual ITeam Team { get; protected set; }

		public virtual bool IsValidFor(IPerson person, DateOnly dateOnly)
		{
			var personPeriod = person.Period(dateOnly);
			return personPeriod != null && person.Period(dateOnly).Team.Equals(Team);
		}

		public virtual string FilterType
		{
			get { return "organization"; }
		}
	}
}