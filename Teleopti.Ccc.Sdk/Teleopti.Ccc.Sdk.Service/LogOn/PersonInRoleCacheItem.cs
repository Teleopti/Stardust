using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.WcfService.LogOn
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