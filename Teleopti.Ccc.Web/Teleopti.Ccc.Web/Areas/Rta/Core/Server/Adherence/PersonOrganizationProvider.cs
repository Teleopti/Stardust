using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Infrastructure.Rta;

namespace Teleopti.Ccc.Web.Areas.Rta.Core.Server.Adherence
{
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