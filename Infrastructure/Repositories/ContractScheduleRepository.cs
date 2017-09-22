using System.Collections;
using System.Collections.Generic;
using NHibernate;
using NHibernate.Criterion;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
    /// <summary>
    /// Repository for contract schedules
    /// </summary>
    public class ContractScheduleRepository : Repository<IContractSchedule>, IContractScheduleRepository
    {
        public ContractScheduleRepository(IUnitOfWork unitOfWork)
#pragma warning disable 618
            : base(unitOfWork)
#pragma warning restore 618
        {
        }

				public ContractScheduleRepository(ICurrentUnitOfWork currentUnitOfWork)
					: base(currentUnitOfWork)
	    {
	    }
		
         public ICollection<IContractSchedule> FindAllContractScheduleByDescription()
         {
             ICollection<IContractSchedule> retList = Session.CreateCriteria(typeof(ContractSchedule))
                       .AddOrder(Order.Asc("Description"))
                       .List<IContractSchedule>();
             return retList;
         }

        public ICollection<IContractSchedule> LoadAllAggregate()
        {
            DetachedCriteria q1 = DetachedCriteria.For<ContractSchedule>()
                        .SetFetchMode("ContractScheduleWeeks", FetchMode.Join);
            DetachedCriteria q2 = DetachedCriteria.For<ContractScheduleWeek>()
                                    .SetFetchMode("workDays", FetchMode.Join);

            IList res = Session.CreateMultiCriteria()
                                        .Add(q1)
                                        .Add(q2)
                                        .List();

            ICollection<IContractSchedule> retlist = CollectionHelper.ToDistinctGenericCollection<IContractSchedule>(res[0]);
            return retlist;
        }
    }
}
