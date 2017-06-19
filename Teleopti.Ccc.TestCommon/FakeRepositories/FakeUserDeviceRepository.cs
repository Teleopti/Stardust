using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeUserDeviceRepository : IUserDeviceRepository
	{
		private IList<IUserDevice> _savedUserDevices = new List<IUserDevice>();
		public IList<IUserDevice> Find(IPerson person)
		{
			return _savedUserDevices.Where(d => d.Owner.Id == person.Id).ToList();
		}

		public IUserDevice FindByToken(string token)
		{
			return _savedUserDevices.SingleOrDefault(d => d.Token == token);
		}

		public IList<IUserDevice> LoadAll()
		{
			throw new NotImplementedException();
		}

		public IUserDevice Load(Guid id)
		{
			throw new NotImplementedException();
		}

		public IUserDevice Get(Guid id)
		{
			throw new NotImplementedException();
		}

		public void Add(IUserDevice userDevice)
		{
			_savedUserDevices.Add(userDevice);
		}

		public void Remove(IUserDevice userDevice)
		{
			_savedUserDevices.Remove(userDevice);
		}

		public IUnitOfWork UnitOfWork { get; }
	}
}