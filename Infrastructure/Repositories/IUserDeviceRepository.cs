using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public interface IUserDeviceRepository
	{
		IList<IUserDevice> Find(IPerson person);
		IUserDevice FindByToken(string token);
		IList<IUserDevice> LoadAll();

		IUserDevice Load(Guid id);

		IUserDevice Get(Guid id);

		void Add(IUserDevice root);

		void Remove(IUserDevice root);

		IUnitOfWork UnitOfWork { get; }
	}
}