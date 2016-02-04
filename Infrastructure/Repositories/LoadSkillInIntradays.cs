using System.Collections.Generic;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class LoadSkillInIntradays : ILoadAllSkillInIntradays
	{
		private readonly ICurrentUnitOfWork _currentUnitOfWork;

		public LoadSkillInIntradays(ICurrentUnitOfWork currentUnitOfWork)
		{
			_currentUnitOfWork = currentUnitOfWork;
		}

		public IEnumerable<SkillInIntraday> Skills()
		{
			return _currentUnitOfWork.Current().Session()
				.GetNamedQuery("loadIntradaySkillsWithAtLeastOneQueue")
				.SetResultTransformer(Transformers.AliasToBean<SkillInIntraday>())
				.SetReadOnly(true)
				.List<SkillInIntraday>();
		}
	}
}