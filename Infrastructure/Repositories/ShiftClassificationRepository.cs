using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Expression;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
    /// <summary>
    /// Repository for ShiftClassification
    /// </summary>
    public class ShiftClassificationRepository : Repository<ShiftClassification>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ShiftClassificationRepository"/> class.
        /// </summary>
        /// <param name="unitOfWork">The unitofwork</param>
        public ShiftClassificationRepository(IUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        /// <summary>
        /// Finds the specified WorkShifts.
        /// </summary>
        /// <param name="site">The site.</param>
        /// <param name="contract">The contract.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-10-31
        /// </remarks>
        public ICollection<WorkShift> Find(Site site, Contract contract)
        {
            InParameter.NotNull("site", site);
            InParameter.NotNull("contract", contract);

            ICollection<ShiftClassification> shiftClassifications = Session.CreateCriteria(typeof(ShiftClassification), "sc")
                .CreateAlias("sc.SiteCollection", "site")
                .CreateAlias("sc.ContractCollection", "contract")
                .Add(Expression.Eq("site.Id",site.Id))
                .Add(Expression.Eq("contract.Id", contract.Id))
                .SetResultTransformer(CriteriaUtil.DistinctRootEntity)
                .List<ShiftClassification>();

            var workShifts = from sc in shiftClassifications
                             from ws in sc.WorkShiftCollection
                             select ws;

            return workShifts.ToList<WorkShift>();
        }
    }
}