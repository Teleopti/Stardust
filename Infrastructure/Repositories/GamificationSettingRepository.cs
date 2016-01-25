using System.Collections.Generic;
using NHibernate.Criterion;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class GamificationSettingRepository : Repository<IGamificationSetting>, IGamificationSettingRepository
	{
#pragma warning disable 618
		public GamificationSettingRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
#pragma warning restore 618
		{
		}

		public GamificationSettingRepository(ICurrentUnitOfWork currentUnitOfWork) : base(currentUnitOfWork)
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
