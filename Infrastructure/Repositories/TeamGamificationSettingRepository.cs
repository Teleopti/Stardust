using System;
using System.Collections.Generic;
using NHibernate.Criterion;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class TeamGamificationSettingRepository : Repository<ITeamGamificationSetting>, ITeamGamificationSettingRepository
	{
		public TeamGamificationSettingRepository(ICurrentUnitOfWork currentUnitOfWork)
			: base(currentUnitOfWork)
		{
		}

		public IEnumerable<ITeamGamificationSetting> FindAllTeamGamificationSettingsSortedByTeam()
		{
			ICollection<ITeamGamificationSetting> retList = Session.CreateCriteria(typeof(TeamGamificationSetting))
				.AddOrder(Order.Asc("Team"))
				.List<ITeamGamificationSetting>();
			return retList;
		}

		public ITeamGamificationSetting FindTeamGamificationSettingsByTeam(ITeam myTeam)
		{
			if (myTeam == null)
				return null;
			var ret = Session.CreateCriteria<TeamGamificationSetting>()
					   .Add(Restrictions.Eq("Team", myTeam))
					   .UniqueResult<ITeamGamificationSetting>();

			return ret;
		}

		public IEnumerable<ITeamGamificationSetting> FetchTeamGamificationSettings(Guid gamificationId)
		{
			return Session.QueryOver<ITeamGamificationSetting>()
				.Where(s => s.GamificationSetting.Id.Value == gamificationId)
				.List<ITeamGamificationSetting>();
		}
	}
}
