using System;
using System.Collections.Generic;
using NHibernate.Criterion;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.UnitOfWork;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	/// <summary>
	/// Repository for day off templates.
	/// </summary>
	/// <remarks>
	/// Created by: shirang
	/// Created date: 2008-10-28
	/// </remarks>
	public class DayOffTemplateRepository : Repository<IDayOffTemplate>, IDayOffTemplateRepository
	{
		public static DayOffTemplateRepository DONT_USE_CTOR2(ICurrentUnitOfWork currentUnitOfWork)
		{
			return new DayOffTemplateRepository(currentUnitOfWork, null, null);
		}

		public static DayOffTemplateRepository DONT_USE_CTOR(IUnitOfWork unitOfWork)
		{
			return new DayOffTemplateRepository(new ThisUnitOfWork(unitOfWork), null, null);
		}

		public DayOffTemplateRepository(ICurrentUnitOfWork currentUnitOfWork, ICurrentBusinessUnit currentBusinessUnit, Lazy<IUpdatedBy> updatedBy)
			: base(currentUnitOfWork, currentBusinessUnit, updatedBy)
		{
		}

		/// <summary>
		/// Finds all contract by description.
		/// </summary>
		/// <returns>The list of <see cref="IDayOffTemplate"/>.</returns>
		/// <remarks>
		/// Created by: shirang
		/// Created date: 2008-10-28
		/// </remarks>
		public IList<IDayOffTemplate> FindAllDayOffsSortByDescription()
		{
			IList<IDayOffTemplate> retList = Session.CreateCriteria(typeof(IDayOffTemplate))
					   .AddOrder(Order.Asc("Description"))
					   .List<IDayOffTemplate>();
			return retList;
		}
	}
}