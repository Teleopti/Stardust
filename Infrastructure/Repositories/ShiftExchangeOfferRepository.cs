using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public interface IShiftExchangeOfferRepository : IRepository<ShiftExchangeOffer>
	{
	}

	public class ShiftExchangeOfferRepository : Repository<ShiftExchangeOffer>, IShiftExchangeOfferRepository
	{
		public ShiftExchangeOfferRepository(ICurrentUnitOfWork currentUnitOfWork)
			: base(currentUnitOfWork)
		{
		}

		public ShiftExchangeOfferRepository(IUnitOfWork unitOfWork)
			: base(unitOfWork)
		{
		}
	}
}