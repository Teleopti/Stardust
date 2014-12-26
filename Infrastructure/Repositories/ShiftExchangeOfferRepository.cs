using System.Collections.Generic;
using NHibernate.Criterion;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{

	//RobTODO - Examine and think about refactoring this.
	//public class ShiftExchangeOfferRepository : Repository<IShiftExchangeOffer>, IShiftExchangeOfferRepository
	//{
	//	public ShiftExchangeOfferRepository(ICurrentUnitOfWork currentUnitOfWork)
	//		: base(currentUnitOfWork)
	//	{
	//	}

	//	public ShiftExchangeOfferRepository(IUnitOfWork unitOfWork)
	//		: base(unitOfWork)
	//	{
	//	}

	//	public IEnumerable<IShiftExchangeOffer> FindPendingOffer(IPerson person, DateOnly date)
	//	{
	//		return Session.CreateCriteria(typeof(IShiftExchangeOffer))
	//				 .Add(Restrictions.Eq("Person", person))
	//				 .Add(Restrictions.Eq("Date", date))
	//				 .Add(Restrictions.Eq("Status", 0))
	//				 .List<IShiftExchangeOffer>();
	//	}
	//}
}