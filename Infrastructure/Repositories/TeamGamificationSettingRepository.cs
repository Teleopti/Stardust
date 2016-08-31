﻿using System.Collections.Generic;
using NHibernate.Criterion;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

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
	}

}
