using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Interfaces.Domain;

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
			return personPeriod != null && person.Period(dateOnly).Team.Site.Equals(Site);
		}

		public virtual string FilterType
		{
			get { return "organization"; }
		}

		public override bool Equals(IEntity other)
		{
			var otherSiteFilter = other as SiteFilter;
			if (otherSiteFilter == null)
				return false;

			return Site.Equals(otherSiteFilter.Site);
		}

		public override int GetHashCode()
		{
			return Site.GetHashCode();
		}
	}
}