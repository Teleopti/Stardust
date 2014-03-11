using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Infrastructure.Rta;

namespace Teleopti.Ccc.Rta.Server.Adherence
{
	public class SiteIdForPerson : ISiteIdForPerson
	{
		private readonly IPersonOrganizationReader _personOrganizationReader;
		private IEnumerable<PersonOrganizationData> _personData;

		public SiteIdForPerson(IPersonOrganizationReader personOrganizationReader)
		{
			_personOrganizationReader = personOrganizationReader;
		}

		public Guid GetSiteId(Guid personId)
		{
			ensureDataIsLoaded();
			return _personData.Single(x => x.PersonId == personId).SiteId;
		}

		private void ensureDataIsLoaded()
		{
			if (_personData != null) return;
			_personData = _personOrganizationReader.LoadAll();
		}
	}
}