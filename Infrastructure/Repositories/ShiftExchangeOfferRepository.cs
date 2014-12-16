using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public interface IShiftExchangeOfferRepository : IRepository<IShiftExchangeOffer>
	{
	}

	public class ShiftExchangeOfferRepository : Repository<IShiftExchangeOffer>, IShiftExchangeOfferRepository
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