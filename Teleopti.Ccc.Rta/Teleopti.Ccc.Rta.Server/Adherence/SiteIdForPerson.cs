using System;
using System.Linq;

namespace Teleopti.Ccc.Rta.Server.Adherence
{
	public class SiteIdForPerson : ISiteIdForPerson
	{
		private readonly IPersonOrganizationProvider _personOrganizationProvider;

		public SiteIdForPerson(IPersonOrganizationProvider personOrganizationProvider)
		{
			_personOrganizationProvider = personOrganizationProvider;
		}

		public Guid GetSiteId(Guid personId)
		{
			return _personOrganizationProvider.LoadAll()
				.Single(x => x.PersonId == personId).SiteId;
		}
	}
}