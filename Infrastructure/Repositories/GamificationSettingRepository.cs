using System.Collections.Generic;
using NHibernate.Criterion;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class GamificationSettingRepository : Repository<IGamificationSetting>, IGamificationSettingRepository
	{
		public GamificationSettingRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
		{
		}

		public GamificationSettingRepository(IUnitOfWorkFactory unitOfWorkFactory) : base(unitOfWorkFactory)
		{
		}

		public GamificationSettingRepository(ICurrentUnitOfWork currentUnitOfWork) : base(currentUnitOfWork)
		{
		}

		public IEnumerable<IGamificationSetting> FindAllGamificationSettingsSortedByDescription()
		{
			ICollection<IGamificationSetting> retList = Session.CreateCriteria(typeof(GamificationSetting))
				.AddOrder(Order.Asc("Description"))
				.SetResultTransformer(Transformers.DistinctRootEntity)
				.List<IGamificationSetting>();

			return retList;
		}
	}
}
