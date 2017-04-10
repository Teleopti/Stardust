using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Sdk.WcfHost.Service.LogOn
{
	public class PersonInRoleCacheItem
	{
		public PersonInRoleCacheItem(IPerson person)
		{
			Roles = person.PermissionInformation.ApplicationRoleCollection.Select(r => r.Id.GetValueOrDefault()).ToList();
		}

		public IEnumerable<Guid> Roles { get; private set; }
	}
}