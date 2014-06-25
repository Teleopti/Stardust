using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Infrastructure.Rta;

namespace Teleopti.Ccc.Rta.Server.Adherence
{
	public class PersonOrganizationProvider : IPersonOrganizationProvider
	{
		private readonly IPersonOrganizationReader _personOrganizationReader;

		public PersonOrganizationProvider(IPersonOrganizationReader personOrganizationReader)
		{
			_personOrganizationReader = personOrganizationReader;
		}

		public IDictionary<Guid, PersonOrganizationData> LoadAll()
		{
			return _personOrganizationReader.LoadAll().ToDictionary(data => data.PersonId);
		}
	}
}