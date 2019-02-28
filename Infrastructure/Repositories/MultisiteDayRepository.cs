using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Multi;
using NHibernate.Criterion;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.UnitOfWork;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
    public class MultisiteDayRepository : Repository<IMultisiteDay>, IMultisiteDayRepository
    {
		public static MultisiteDayRepository DONT_USE_CTOR(IUnitOfWork unitOfWork)
		{
			return new MultisiteDayRepository(new ThisUnitOfWork(unitOfWork), null, null);
		}

		public static MultisiteDayRepository DONT_USE_CTOR(ICurrentUnitOfWork currentUnitOfWork)
		{
			return new MultisiteDayRepository(currentUnitOfWork, null, null);
		}

		public MultisiteDayRepository(ICurrentUnitOfWork currentUnitOfWork, ICurrentBusinessUnit currentBusinessUnit, Lazy<IUpdatedBy> updatedBy)
			: base(currentUnitOfWork, currentBusinessUnit, updatedBy)
		{
		}
		
        public ICollection<IMultisiteDay> FindRange(DateOnlyPeriod period, ISkill skill, IScenario scenario)
        {
            InParameter.NotNull(nameof(skill), skill);
            InParameter.NotNull(nameof(period), period);
            InParameter.NotNull(nameof(scenario), scenario);

            DetachedCriteria multisiteDaySubquery = DetachedCriteria.For<MultisiteDay>("md")
                .Add(Restrictions.Eq("md.Scenario", scenario))
                .Add(Restrictions.Between("md.MultisiteDayDate", period.StartDate, period.EndDate))
                .Add(Restrictions.Eq("Skill", skill))
                .SetProjection(Projections.Property("md.Id"));

            DetachedCriteria multisiteDayPeriod = DetachedCriteria.For<MultisiteDay>("md")
                .Add(Subqueries.PropertyIn("Id", multisiteDaySubquery))
                .Fetch("MultisitePeriodCollection");

            DetachedCriteria multisitePeriodDistribution = DetachedCriteria.For<MultisitePeriod>()
                .Add(Subqueries.PropertyIn("Parent", multisiteDaySubquery))
                .Fetch("Distribution");

            var result = Session.CreateQueryBatch()
				.Add<MultisitePeriod>(multisitePeriodDistribution)
                .Add<MultisiteDay>(multisiteDayPeriod);

            return CollectionHelper.ToDistinctGenericCollection<IMultisiteDay>(result.GetResult<MultisiteDay>(1));
        }
		
        public ICollection<IMultisiteDay> GetAllMultisiteDays(DateOnlyPeriod period, ICollection<IMultisiteDay> multisiteDays, IMultisiteSkill skill, IScenario scenario, bool addToRepository = true)
        {
            var uniqueDays = period.DayCollection();

            if (uniqueDays.Count != multisiteDays.Count)
            {
                var currentDateTimes = multisiteDays.Select(s => s.MultisiteDayDate);
                var datesToProcess = uniqueDays.Except(currentDateTimes);

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
