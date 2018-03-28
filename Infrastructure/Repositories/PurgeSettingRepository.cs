using NHibernate.Transform;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class PurgeSettingRepository : IPurgeSettingRepository
	{
		private readonly ICurrentUnitOfWork _currentUnitOfWork;
		public IEnumerable<PurgeSetting> FindAllPurgeSettings()
		{
			var result = _currentUnitOfWork.Session().CreateSQLQuery(
					@"SELECT * FROM [dbo].[PurgeSetting]")
				.SetResultTransformer(Transformers.AliasToBean<PurgeSetting>())
				.List<PurgeSetting>();

			return result;
		}

		public PurgeSettingRepository(ICurrentUnitOfWork currentUnitOfWork)
		{
			_currentUnitOfWork = currentUnitOfWork;
		}
	}
}
