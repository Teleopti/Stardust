using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface IUserDeviceRepository
	{
		IList<IUserDevice> Find(IPerson person);
		IUserDevice FindByToken(string token);
		IEnumerable<IUserDevice> LoadAll();

		IUserDevice Load(Guid id);

		IUserDevice Get(Guid id);

		void Add(IUserDevice root);

		void Remove(IUserDevice root);

		void Remove(params string[] tokens);
	}
}