using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Criterion;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
    /// <summary>
    /// Ropository for ValidatedVolumeDay
    /// </summary>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2008-01-08
    /// </remarks>
    public class ValidatedVolumeDayRepository : Repository<IValidatedVolumeDay>, IValidatedVolumeDayRepository
    {
				public ValidatedVolumeDayRepository(ICurrentUnitOfWork currentUnitOfWork)
					: base(currentUnitOfWork, null, null)
	    {
		    
	    }

        /// <summary>
        /// Finds the range.
        /// </summary>
        /// <param name="period">The period.</param>
        /// <param name="workload">The workload.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-25
        /// </remarks>
        public ICollection<IValidatedVolumeDay> FindRange(DateOnlyPeriod period, IWorkload workload)
        {
            InParameter.NotNull(nameof(period), period);
            InParameter.NotNull(nameof(workload), workload);

            IList<IValidatedVolumeDay> validatedVolumeDays = Session.CreateCriteria(typeof(ValidatedVolumeDay), "VVD")
                .Add(Restrictions.Eq("Workload", workload))
                .Add(Restrictions.Between("VolumeDayDate", period.StartDate, period.EndDate))
                .List<IValidatedVolumeDay>();

            return validatedVolumeDays;
        }

        /// <summary>
        /// Matches the days.
        /// </summary>
        /// <param name="workload">The workload.</param>
        /// <param name="taskOwnerList">The task owner list (Containing all actual days).</param>
        /// <param name="existingValidatedVolumeDays">The existing validated volume days.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-28
        /// </remarks>
        public IList<ITaskOwner> MatchDays(IWorkload workload, IEnumerable<ITaskOwner> taskOwnerList, IEnumerable<IValidatedVolumeDay> existingValidatedVolumeDays)
        {
            IList<ITaskOwner> daysToReturn = new List<ITaskOwner>();
            foreach (ITaskOwner taskOwner in taskOwnerList)
            {
                var taskOwnerDate = taskOwner.CurrentDate;
                IValidatedVolumeDay existingValidatedVolumeDay = existingValidatedVolumeDays.FirstOrDefault(e => e.VolumeDayDate == taskOwnerDate);
                if (existingValidatedVolumeDay == null)
                {
                    existingValidatedVolumeDay = new ValidatedVolumeDay(workload, taskOwner.CurrentDate);
                }
                existingValidatedVolumeDay.TaskOwner = taskOwner;
                daysToReturn.Add(existingValidatedVolumeDay);
            }

            return daysToReturn;
        }

        /// <summary>
        /// Finds the last validated day.
        /// </summary>
        /// <param name="workload">The workload.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-28
        /// </remarks>
        public DateOnly FindLastValidatedDay(IWorkload workload)
        {
            InParameter.NotNull(nameof(workload), workload);

            DateOnly? latestDate = Session.CreateCriteria(typeof(ValidatedVolumeDay))
                .Add(Restrictions.Eq("Workload", workload))
                .SetProjection(Projections.Max("VolumeDayDate"))
                .UniqueResult<DateOnly?>();

            if (!latestDate.HasValue)
            {
                latestDate = new DateOnly(DateTime.Today.AddMonths(-1));
            }
            return latestDate.Value;
        }

        public IValidatedVolumeDay FindLatestUpdated(ISkill skill)
        {
            ICriteria query = Session.CreateCriteria(typeof(ValidatedVolumeDay), "vd")
                .Add(Restrictions.InG("vd.Workload", skill.WorkloadCollection.ToArray()));

            IValidatedVolumeDay validatedVolumeDay = query
                .AddOrder(Order.Desc("vd.UpdatedOn"))
                .SetMaxResults(1)
                .UniqueResult<IValidatedVolumeDay>();

            return validatedVolumeDay;
        }
    }
}