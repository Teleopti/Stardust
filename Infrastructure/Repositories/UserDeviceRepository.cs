using System.Collections.Generic;
using NHibernate.Criterion;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.SystemSetting;

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
			var result = Session.CreateCriteria(typeof(UserDevice), "pd")
				.Add(Restrictions.Eq("Owner", person))
				.List<IUserDevice>();
			return result;
		}

		public IUserDevice FindByToken(string token)
		{
			return Session.CreateCriteria(typeof(UserDevice), "pd")
				.Add(Restrictions.Eq("Token", token))
				.UniqueResult<IUserDevice>();

		}
	}
}