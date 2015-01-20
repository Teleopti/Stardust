using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate.Criterion;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class TeamGamificationSettingRepository : Repository<ITeamGamificationSetting>, ITeamGamificationSettingRepository
	{
		public TeamGamificationSettingRepository(IUnitOfWork unitOfWork)
			: base(unitOfWork)
		{
		}

		public TeamGamificationSettingRepository(IUnitOfWorkFactory unitOfWorkFactory)
			: base(unitOfWorkFactory)
		{
		}

		public TeamGamificationSettingRepository(ICurrentUnitOfWork currentUnitOfWork)
			: base(currentUnitOfWork)
		{
		}

		public IEnumerable<ITeamGamificationSetting> FindAllTeamGamificationSettingsSortedByTeam()
		{
			ICollection<ITeamGamificationSetting> retList = Session.CreateCriteria(typeof(TeamGamificationSetting))
				.AddOrder(Order.Asc("Team"))
				.SetResultTransformer(Transformers.DistinctRootEntity)
				.List<ITeamGamificationSetting>();
			return retList;
		}
	}

}
