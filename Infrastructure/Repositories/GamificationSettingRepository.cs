using System.Collections.Generic;
using NHibernate.Criterion;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class GamificationSettingRepository : Repository<IGamificationSetting>, IGamificationSettingRepository
	{
#pragma warning disable 618
		public GamificationSettingRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
#pragma warning restore 618
		{
		}

		public GamificationSettingRepository(ICurrentUnitOfWork currentUnitOfWork) : base(currentUnitOfWork, null, null)
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
