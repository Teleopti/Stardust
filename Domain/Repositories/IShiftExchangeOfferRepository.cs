using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface IShiftExchangeOfferRepository : IRepository<IShiftExchangeOffer>
	{
		IEnumerable<IShiftExchangeOffer> FindPendingOffer(IPerson person, DateOnly date);
	}
}