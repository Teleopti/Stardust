using System;
using System.Collections.Generic;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Infrastructure.Analytics;

namespace Teleopti.Ccc.Infrastructure.Repositories.Analytics
{
	public class AnalyticsDateRepository : AnalyticsDateRepositoryBase, IAnalyticsDateRepository
	{
		public AnalyticsDateRepository(ICurrentAnalyticsUnitOfWork analyticsUnitOfWork) : base(analyticsUnitOfWork)
		{
		}
	}

	public abstract class AnalyticsDateRepositoryBase
	{
		protected readonly ICurrentAnalyticsUnitOfWork AnalyticsUnitOfWork;

		protected AnalyticsDateRepositoryBase(ICurrentAnalyticsUnitOfWork analyticsUnitOfWork)
		{
			AnalyticsUnitOfWork = analyticsUnitOfWork;
		}

		public IAnalyticsDate MaxDate()
		{
			return AnalyticsUnitOfWork.Current().Session().CreateCriteria<AnalyticsDate>()
				.Add(Restrictions.Ge(nameof(AnalyticsDate.DateId), 0))
				.AddOrder(Order.Desc(nameof(AnalyticsDate.DateId)))
				.SetMaxResults(1)
				.SetReadOnly(true)
				.UniqueResult<IAnalyticsDate>();
		}

		public IAnalyticsDate MinDate()
		{
			return AnalyticsUnitOfWork.Current().Session().CreateCriteria<AnalyticsDate>()
				.Add(Restrictions.Ge(nameof(AnalyticsDate.DateId), 0))
				.AddOrder(Order.Asc(nameof(AnalyticsDate.DateId)))
				.SetMaxResults(1)
				.SetReadOnly(true)
				.UniqueResult<IAnalyticsDate>();
		}

		public IAnalyticsDate Date(DateTime dateDate)
		{
			return AnalyticsUnitOfWork.Current().Session().CreateCriteria<AnalyticsDate>()
				.Add(Restrictions.Eq(nameof(AnalyticsDate.DateDate), dateDate.Date))
				.SetReadOnly(true)
				.UniqueResult<IAnalyticsDate>();
		}

		public IList<IAnalyticsDate> GetAllPartial()
		{
			AnalyticsDatePartial analyticsDatePartial = null;
			return AnalyticsUnitOfWork.Current().Session().QueryOver<AnalyticsDate>()
				.Where(ad => ad.DateId >= 0)
				.SelectList(list => list
					.Select(d => d.DateId).WithAlias(() => analyticsDatePartial.DateId)
					.Select(d => d.DateDate).WithAlias(() => analyticsDatePartial.DateDate)
					)
				.OrderBy(ad => ad.DateId)
				.Asc
				.TransformUsing(Transformers.AliasToBean<AnalyticsDatePartial>())
				.List<IAnalyticsDate>();
		}
	}
}
