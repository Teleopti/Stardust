using System;
using System.Collections.Generic;
using System.Linq;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta
{
	public interface IPersonOrganizationProvider
	{
		IDictionary<Guid, PersonOrganizationData> PersonOrganizationData();
	}

	public class PersonOrganizationProvider : IPersonOrganizationProvider
	{
		private readonly IPersonOrganizationReader _personOrganizationReader;

		public PersonOrganizationProvider(IPersonOrganizationReader personOrganizationReader)
		{
			_personOrganizationReader = personOrganizationReader;
		}

		public IDictionary<Guid, PersonOrganizationData> PersonOrganizationData()
		{
			return _personOrganizationReader.PersonOrganizationData().ToDictionary(data => data.PersonId);
		}
	}
}
