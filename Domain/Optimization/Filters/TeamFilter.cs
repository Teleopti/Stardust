using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

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
			return personPeriod != null && personPeriod.Team.Equals(Team);
		}

		public virtual string FilterType => "organization";

		public override bool Equals(IEntity other)
		{
			return other is TeamFilter otherTeamFilter && Team.Equals(otherTeamFilter.Team);
		}

		public override int GetHashCode()
		{
			return Team.GetHashCode();
		}
	}
}