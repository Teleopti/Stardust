using System.Collections.Generic;
using NHibernate.Criterion;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Payroll;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Foundation;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
    /// <summary>
    /// Repository for payroll results
    /// </summary>
    public class PayrollResultRepository : Repository<IPayrollResult>, IPayrollResultRepository
    {
				public PayrollResultRepository(ICurrentUnitOfWork currentUnitOfWork)
					: base(currentUnitOfWork, null, null)
	    {
		    
	    }

        public ICollection<IPayrollResult> GetPayrollResultsByPayrollExport(IPayrollExport payrollExport)
        {
            ICollection<IPayrollResult> retList = Session.CreateCriteria(typeof (PayrollResult))
                .Add(Restrictions.Eq("PayrollExport", payrollExport))
                .SetTimeout(10000)
                .List<IPayrollResult>();

            retList.ForEach(i => LazyLoadingManager.Initialize(i.Owner));
            retList.ForEach(i => LazyLoadingManager.Initialize(i.Details));
            return retList;
        }
    }
}