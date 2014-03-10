using System;
using System.Linq;

namespace Teleopti.Ccc.Rta.Server.Adherence
{
	public class SiteIdForPerson : ISiteIdForPerson
	{
		private readonly IPersonOrganizationReader _personOrganizationReader;

		public SiteIdForPerson(IPersonOrganizationReader personOrganizationReader)
		{
			_personOrganizationReader = personOrganizationReader;
		}

		public Guid GetSiteId(Guid personId)
		{
			var personData = _personOrganizationReader.LoadAll();
			return personData.Single(x => x.PersonId == personId).SiteId;
		}
	}
}