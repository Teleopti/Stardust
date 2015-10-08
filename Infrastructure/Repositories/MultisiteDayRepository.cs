using System.Collections;
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
    /// Ropository for MultisiteDay
    /// </summary>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2008-01-08
    /// </remarks>
    public class MultisiteDayRepository : Repository<IMultisiteDay>, IMultisiteDayRepository
    {
#pragma warning disable 618
    	public MultisiteDayRepository(IUnitOfWorkFactory unitOfWorkFactory) :base(unitOfWorkFactory)
#pragma warning restore 618
    	{
    	}
        /// <summary>
        /// Initializes a new instance of the <see cref="MultisiteDayRepository"/> class.
        /// </summary>
        /// <param name="unitOfWork">The unitofwork</param>
        public MultisiteDayRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

				public MultisiteDayRepository(ICurrentUnitOfWork currentUnitOfWork)
					: base(currentUnitOfWork)
	    {
		    
	    }

        /// <summary>
        /// Finds the range.
        /// </summary>
        /// <param name="period">The period.</param>
        /// <param name="skill">The skill.</param>
        /// <param name="scenario">The scenario.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-25
        /// </remarks>
        public ICollection<IMultisiteDay> FindRange(DateOnlyPeriod period, ISkill skill, IScenario scenario)
        {
            InParameter.NotNull("skill", skill);
            InParameter.NotNull("period", period);
            InParameter.NotNull("scenario", scenario);

            DetachedCriteria multisiteDaySubquery = DetachedCriteria.For<MultisiteDay>("md")
                .Add(Restrictions.Eq("md.Scenario", scenario))
                .Add(Restrictions.Between("md.MultisiteDayDate", period.StartDate, period.EndDate))
                .Add(Restrictions.Eq("Skill", skill))
                .SetProjection(Projections.Property("md.Id"));

            DetachedCriteria multisiteDayPeriod = DetachedCriteria.For<MultisiteDay>("md")
                .Add(Subqueries.PropertyIn("Id", multisiteDaySubquery))
                .SetFetchMode("MultisitePeriodCollection", FetchMode.Join);

            DetachedCriteria multisitePeriodDistribution = DetachedCriteria.For<MultisitePeriod>()
                .Add(Subqueries.PropertyIn("Parent", multisiteDaySubquery))
                .SetFetchMode("Distribution", FetchMode.Join);

            IList result = Session.CreateMultiCriteria()
                .Add(multisiteDayPeriod)
                .Add(multisitePeriodDistribution)
                .List();

            ICollection<IMultisiteDay> multisiteDays = CollectionHelper.ToDistinctGenericCollection<IMultisiteDay>(result[0]);

            return multisiteDays;
        }

        /// <summary>
        /// Gets all multisite days.
        /// </summary>
        /// <param name="period">The period.</param>
        /// <param name="multisiteDays">The multisite days.</param>
        /// <param name="skill">The skill.</param>
        /// <param name="scenario">The scenario.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-23
        /// </remarks>
        public ICollection<IMultisiteDay> GetAllMultisiteDays(DateOnlyPeriod period, ICollection<IMultisiteDay> multisiteDays, IMultisiteSkill skill, IScenario scenario)
        {
            return GetAllMultisiteDays(period, multisiteDays, skill, scenario, true);
        }

        /// <summary>
        /// Gets all multisite days.
        /// </summary>
        /// <param name="period">The period.</param>
        /// <param name="multisiteDays">The multisite days.</param>
        /// <param name="skill">The skill.</param>
        /// <param name="scenario">The scenario.</param>
        /// <param name="addToRepository">if set to <c>true</c> [add to repository].</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-23
        /// </remarks>
        public ICollection<IMultisiteDay> GetAllMultisiteDays(DateOnlyPeriod period, ICollection<IMultisiteDay> multisiteDays, IMultisiteSkill skill, IScenario scenario, bool addToRepository)
        {
            //CreateProjection Date Collection for the local dates in the current time zone (from skill)
            var uniqueDays = period.DayCollection();

            if (uniqueDays.Count != multisiteDays.Count)
            {
                var currentDateTimes = multisiteDays.Select(s => s.MultisiteDayDate);
                var datesToProcess = uniqueDays.Where(u => !currentDateTimes.Contains(u));

                foreach (var uniqueDate in datesToProcess)
                {
                    IMultisiteDay multisiteDay = new MultisiteDay();
                    IMultisiteDayTemplate multisiteDayTemplate = (IMultisiteDayTemplate) skill.GetTemplateAt(TemplateTarget.Multisite,(int)uniqueDate.DayOfWeek);

                    multisiteDay.CreateFromTemplate(uniqueDate, skill, scenario, multisiteDayTemplate);

                    multisiteDays.Add(multisiteDay);
                    if (addToRepository) Add(multisiteDay);
                }
            }

            multisiteDays = multisiteDays.OrderBy(wd => wd.MultisiteDayDate).ToList();
            return multisiteDays;
        }

        /// <summary>
        /// Deletes the specified date time period.
        /// </summary>
        /// <param name="dateTimePeriod">The date time period.</param>
        /// <param name="multisiteSkill">The multisite skill.</param>
        /// <param name="scenario">The scenario.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-12-22
        /// </remarks>
        public void Delete(DateOnlyPeriod dateTimePeriod, IMultisiteSkill multisiteSkill, IScenario scenario)
        {
            ICollection<IMultisiteDay> multisiteDays = FindRange(dateTimePeriod, multisiteSkill, scenario);
            foreach (IMultisiteDay multisiteDay in multisiteDays)
            {
                Remove(multisiteDay);
            }
            UnitOfWork.PersistAll();
        }
    }
}
