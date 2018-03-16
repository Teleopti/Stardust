using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries;

namespace Teleopti.Ccc.TestCommon.FakeRepositories.Tenant
{
	public class FakePersistExternalApplicationAccess : IPersistExternalApplicationAccess
	{
		public List<ExternalApplicationAccess> Storage { get; } = new List<ExternalApplicationAccess>();

		public void Persist(ExternalApplicationAccess externalApplicationAccess)
		{
			Storage.Add(externalApplicationAccess);
			typeof(ExternalApplicationAccess)
				.GetProperty(nameof(externalApplicationAccess.Id), BindingFlags.Instance | BindingFlags.Public)
				.SetValue(externalApplicationAccess, new Random().Next());
		}

		public void Remove(int id, Guid personId)
		{
			var item = Storage.FirstOrDefault(a => a.Id == id && a.PersonId == personId);
			if (item != null)
			{
				Storage.Remove(item);
			}
		}
	}
}
