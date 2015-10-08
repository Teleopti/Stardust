using System.Collections.Generic;
using NHibernate.Criterion;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Payroll;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
    /// <summary>
    /// Repository for payroll results
    /// </summary>
    public class PayrollResultRepository : Repository<IPayrollResult>, IPayrollResultRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PayrollResultRepository"/> class.
        /// </summary>
        /// <param name="unitOfWork">The unitofwork</param>
        public PayrollResultRepository(IUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        public PayrollResultRepository(IUnitOfWorkFactory unitOfWorkFactory)
#pragma warning disable 618
            : base(unitOfWorkFactory)
#pragma warning restore 618
        {
        }

				public PayrollResultRepository(ICurrentUnitOfWork currentUnitOfWork)
					: base(currentUnitOfWork)
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

        //public override bool ValidateUserLoggedOn
        //{
        //    get
        //    {
        //        return false;
        //    }
        //}
    }
}