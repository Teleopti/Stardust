using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Optimization.Filters
{
	public class SiteFilter : Entity, IFilter
	{
		protected SiteFilter()
		{
		}

		public SiteFilter(ISite site)
		{
			Site = site;
		}

		public virtual ISite Site { get; protected set; }

		public virtual bool IsValidFor(IPerson person, DateOnly dateOnly)
		{
			var personPeriod = person.Period(dateOnly);
			return personPeriod != null && personPeriod.Team.Site.Equals(Site);
		}

		public virtual string FilterType => "organization";

		public override bool Equals(IEntity other)
		{
			return other is SiteFilter otherSiteFilter && Site.Equals(otherSiteFilter.Site);
		}

		public override int GetHashCode()
		{
			return Site.GetHashCode();
		}
	}
}