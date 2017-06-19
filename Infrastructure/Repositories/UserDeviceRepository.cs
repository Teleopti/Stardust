using System.Collections.Generic;
using NHibernate.Criterion;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class UserDeviceRepository : Repository<IUserDevice>, IUserDeviceRepository
	{
		public UserDeviceRepository(IUnitOfWork unitOfWork)
#pragma warning disable 618
			: base(unitOfWork)
#pragma warning restore 618
		{
		}

		public UserDeviceRepository(ICurrentUnitOfWork currentUnitOfWork) : base(currentUnitOfWork)
		{
		}

		public IList<IUserDevice> Find(IPerson person)
		{
			var result = Session.CreateCriteria(typeof(IUserDevice), "pd")
				.Add(Restrictions.Eq("Person", person))
				.List<IUserDevice>();
			return result;
		}

		public IUserDevice FindByToken(string token)
		{
			return Session.CreateCriteria(typeof(IUserDevice), "pd")
				.Add(Restrictions.Eq("Token", token))
				.UniqueResult<IUserDevice>();

		}
	}
}