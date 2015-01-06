using System.Collections.Generic;
using NHibernate.Criterion;
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

		public GamificationSettingRepository(ICurrentUnitOfWork currentUnitOfWork)
			: base(currentUnitOfWork)
		{
		}

		public IEnumerable<IGamificationSetting> FindAllBadgeSettingsByDescription()
		{
			throw new System.NotImplementedException();
		}
	}
}
