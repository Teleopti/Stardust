using System.Collections.Generic;
using Teleopti.Ccc.Infrastructure.Rta;

namespace Teleopti.Ccc.Rta.Server.Adherence
{
	public class PersonOrganizationProvider : IPersonOrganizationProvider
	{
		private IEnumerable<PersonOrganizationData> _alreadyFetchedPersonOrganizationData;
		private readonly IPersonOrganizationReader _personOrganizationReader;

		public PersonOrganizationProvider(IPersonOrganizationReader personOrganizationReader)
		{
			_personOrganizationReader = personOrganizationReader;
		}

		public IEnumerable<PersonOrganizationData> LoadAll()
		{
			ensurePersonOrganazitionDataIsLoaded();
			return _alreadyFetchedPersonOrganizationData;
		}

		private void ensurePersonOrganazitionDataIsLoaded()
		{
			if (_alreadyFetchedPersonOrganizationData == null)
			{
				_alreadyFetchedPersonOrganizationData = _personOrganizationReader.LoadAll();
			}
		}
	}
}