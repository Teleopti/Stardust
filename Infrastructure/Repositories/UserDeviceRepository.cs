using System.Collections.Generic;
using NHibernate.Criterion;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.SystemSetting;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class UserDeviceRepository : IUserDeviceRepository
	{
		private ICurrentUnitOfWork _unitOfWork;

		public UserDeviceRepository(ICurrentUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public void Add(IUserDevice root)
		{
			_unitOfWork.Session().SaveOrUpdate(root);
		}

		public IList<IUserDevice> Find(IPerson person)
		{
			var result = _unitOfWork.Session().CreateCriteria(typeof(UserDevice), "pd")
				.Add(Restrictions.Eq("Owner", person))
				.List<IUserDevice>();
			return result;
		}

		public IUserDevice FindByToken(string token)
		{
			return _unitOfWork.Session().CreateCriteria(typeof(UserDevice), "pd")
				.Add(Restrictions.Eq("Token", token))
				.UniqueResult<IUserDevice>();
		}

		public void Remove(params string[] tokens)
		{
			foreach (var batch in tokens.Batch(20))
			{
				var session = _unitOfWork.Session();
				var devices = session.CreateCriteria(typeof(UserDevice), "pd")
				.Add(Restrictions.In("Token", tokens))
				.List<IUserDevice>();
				devices?.ForEach(device => session.Delete(device));
			}
		}
	}
}