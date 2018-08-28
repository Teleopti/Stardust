using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class FindPersonAssignmentAsync : IFindPersonAssignment
	{
		private readonly ICurrentUnitOfWork _currentUnitOfWork;

		public FindPersonAssignmentAsync(ICurrentUnitOfWork currentUnitOfWork)
		{
			_currentUnitOfWork = currentUnitOfWork;
		}

		public async Task<IEnumerable<IPersonAssignment>> Find(IEnumerable<IPerson> persons, DateOnlyPeriod period, IScenario scenario)
		{
			var retList = new List<IPersonAssignment>();
			foreach (var personList in persons.Batch(400))
			{
				var res = await _currentUnitOfWork.Current().Session().CreateCriteria(typeof(PersonAssignment), "ass")
					.Add(Restrictions.InG("ass.Person", personList))
					.Add(Restrictions.Eq("ass.Scenario", scenario))
					.Add(Restrictions.Between("ass.Date", period.StartDate, period.EndDate))
					.SetTimeout(300)
					.SetFetchMode("ShiftLayers", FetchMode.Join)
					.SetResultTransformer(Transformers.DistinctRootEntity)
					.ListAsync<PersonAssignment>();
				retList.AddRange(res);
			}
			return retList;
		}

	}
}