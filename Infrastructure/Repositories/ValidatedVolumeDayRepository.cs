using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Criterion;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

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
        private bool isOperationCanceled;
        /// <summary>
        /// Initializes a new instance of the <see cref="ValidatedVolumeDayRepository"/> class.
        /// </summary>
        /// <param name="unitOfWork">The unitofwork</param>
        public ValidatedVolumeDayRepository(IUnitOfWork unitOfWork)
            : base(unitOfWork)
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
            InParameter.NotNull("period", period);
            InParameter.NotNull("workload", workload);

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
        /// <param name="taskOwnerList">The task owner list.</param>
        /// <param name="existingValidatedVolumeDays">The existing validated volume days.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-21
        /// </remarks>
        public IList<ITaskOwner> MatchDays(IWorkload workload, IEnumerable<ITaskOwner> taskOwnerList, IEnumerable<IValidatedVolumeDay> existingValidatedVolumeDays)
        {
            return MatchDays(workload, taskOwnerList, existingValidatedVolumeDays,true);
        }

        /// <summary>
        /// Matches the days.
        /// </summary>
        /// <param name="workload">The workload.</param>
        /// <param name="taskOwnerList">The task owner list (Containing all actual days).</param>
        /// <param name="existingValidatedVolumeDays">The existing validated volume days.</param>
        /// <param name="addToRepository">if set to <c>true</c> [add to repository].</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-28
        /// </remarks>
        public IList<ITaskOwner> MatchDays(IWorkload workload, IEnumerable<ITaskOwner> taskOwnerList, IEnumerable<IValidatedVolumeDay> existingValidatedVolumeDays, bool addToRepository)
        {
            IList<ITaskOwner> daysToReturn = new List<ITaskOwner>();
            foreach (ITaskOwner taskOwner in taskOwnerList)
            {
                //If this operation is canceled we will return null and it will be handeled by the caller of the cancel              
                if(isOperationCanceled)
                {
                    isOperationCanceled = false;
                    return null;
                }

                DateTime taskOwnerDate = taskOwner.CurrentDate;
                IValidatedVolumeDay existingValidatedVolumeDay = existingValidatedVolumeDays.FirstOrDefault(e => e.VolumeDayDate == taskOwnerDate);
                if (existingValidatedVolumeDay == null)
                {
                    existingValidatedVolumeDay = new ValidatedVolumeDay(workload, taskOwner.CurrentDate);
                    if (addToRepository) Add(existingValidatedVolumeDay);
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
            InParameter.NotNull("workload", workload);

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
                .Add(Restrictions.In("vd.Workload", skill.WorkloadCollection.ToList()));

            IValidatedVolumeDay validatedVolumeDay = query
                .AddOrder(Order.Desc("vd.UpdatedOn"))
                .AddOrder(Order.Desc("vd.CreatedOn"))
                .SetMaxResults(1)
                .UniqueResult<IValidatedVolumeDay>();

            return validatedVolumeDay;
        }

        /// <summary>
        /// Cancels the match days.
        /// </summary>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 10/1/2008
        /// </remarks>
        public void CancelMatchDays()
        {
            //Session.CancelQuery();
            isOperationCanceled = true;
        }
    }
}