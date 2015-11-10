using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.Filters
{
	public class SiteFilter : IFilter
	{
		private readonly ISite _site;

		public SiteFilter(ISite site)
		{
			_site = site;
		}

		public bool IsValidFor(IPerson person, DateOnly dateOnly)
		{
			var personPeriod = person.Period(dateOnly);
			return personPeriod != null && person.Period(dateOnly).Team.Site.Equals(_site);
		}
	}
}