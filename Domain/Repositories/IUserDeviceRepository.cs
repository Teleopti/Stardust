using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface IUserDeviceRepository
	{
		IList<IUserDevice> Find(IPerson person);
		IUserDevice FindByToken(string token);
		void Add(IUserDevice root);
		
		void Remove(params string[] tokens);
	}
}