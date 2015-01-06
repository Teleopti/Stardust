using System.Collections.Generic;
using NHibernate.Criterion;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class DifferentialAgentBadgeSettingsRepository : Repository<IDifferentialAgentBadgeSettings>, IDifferentialAgentBadgeSettingRepository
	{
		public DifferentialAgentBadgeSettingsRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
		{
		}

		public DifferentialAgentBadgeSettingsRepository(IUnitOfWorkFactory unitOfWorkFactory) : base(unitOfWorkFactory)
		{
		}

		public DifferentialAgentBadgeSettingsRepository(ICurrentUnitOfWork currentUnitOfWork)
			: base(currentUnitOfWork)
		{
		}

		public IEnumerable<IDifferentialAgentBadgeSettings> FindAllBadgeSettingsByDescription()
		{
			throw new System.NotImplementedException();
		}
	}
}
