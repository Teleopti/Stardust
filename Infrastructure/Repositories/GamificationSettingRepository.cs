using System;
using System.Collections.Generic;
using NHibernate.Criterion;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.UnitOfWork;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class GamificationSettingRepository : Repository<IGamificationSetting>, IGamificationSettingRepository
	{
		public static GamificationSettingRepository DONT_USE_CTOR(ICurrentUnitOfWork currentUnitOfWork)
		{
			return new GamificationSettingRepository(currentUnitOfWork, null, null);
		}

		public static GamificationSettingRepository DONT_USE_CTOR(IUnitOfWork unitOfWork)
		{
			return new GamificationSettingRepository(new ThisUnitOfWork(unitOfWork), null, null);
		}

		public GamificationSettingRepository(ICurrentUnitOfWork currentUnitOfWork, ICurrentBusinessUnit currentBusinessUnit, Lazy<IUpdatedBy> updatedBy) 
			: base(currentUnitOfWork, currentBusinessUnit, updatedBy)
		{
		}

		public IEnumerable<IGamificationSetting> FindAllGamificationSettingsSortedByDescription()
		{
			ICollection<IGamificationSetting> retList = Session.CreateCriteria(typeof(GamificationSetting))
				.AddOrder(Order.Asc("Description"))
				.List<IGamificationSetting>();

			return retList;
		}

		public ICollection<IGamificationSetting> FindSettingByDescriptionName(string name)
		{
			ICollection<IGamificationSetting> retList = Session.CreateCriteria<GamificationSetting>()
					   .Add(Restrictions.Eq("Description.Name", name))
					  .List<IGamificationSetting>();
			return retList;
		}
	}
}
