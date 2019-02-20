using System.Collections.Generic;
using NHibernate.Criterion;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;

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
		public DayOffTemplateRepository(IUnitOfWork unitOfWork)
#pragma warning disable 618
			: base(unitOfWork)
#pragma warning restore 618
		{
		}

		public DayOffTemplateRepository(ICurrentUnitOfWork currentUnitOfWork)
			: base(currentUnitOfWork, null, null)
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