using System;
using System.Collections.Generic;
using System.Linq;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public interface IPersonOrganizationProvider
	{
		IDictionary<Guid, PersonOrganizationData> PersonOrganizationData(string tenant);
	}

	public class PersonOrganizationProvider : IPersonOrganizationProvider
	{
		private readonly IPersonOrganizationReader _personOrganizationReader;

		public PersonOrganizationProvider(IPersonOrganizationReader personOrganizationReader)
		{
			_personOrganizationReader = personOrganizationReader;
		}

		public IDictionary<Guid, PersonOrganizationData> PersonOrganizationData(string tenant)
		{
			return _personOrganizationReader.PersonOrganizationData(tenant).ToDictionary(data => data.PersonId);
		}
	}
}
