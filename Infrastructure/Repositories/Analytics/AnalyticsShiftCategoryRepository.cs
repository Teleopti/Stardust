using System.Collections.Generic;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Infrastructure.Analytics;

namespace Teleopti.Ccc.Infrastructure.Repositories.Analytics
{
	public class AnalyticsShiftCategoryRepository : IAnalyticsShiftCategoryRepository
	{
		private readonly ICurrentAnalyticsUnitOfWork _currentAnalyticsUnitOfWork;

		public AnalyticsShiftCategoryRepository(ICurrentAnalyticsUnitOfWork currentAnalyticsUnitOfWork)
		{
			_currentAnalyticsUnitOfWork = currentAnalyticsUnitOfWork;
		}

		public IList<IAnalyticsGeneric> ShiftCategories()
		{
			return _currentAnalyticsUnitOfWork.Current().Session().CreateSQLQuery(
				"select shift_category_id Id, shift_category_code Code from mart.dim_shift_category WITH (NOLOCK)")
				.SetResultTransformer(Transformers.AliasToBean(typeof(AnalyticsGeneric)))
				.SetReadOnly(true)
				.List<IAnalyticsGeneric>();
		}
	}
}