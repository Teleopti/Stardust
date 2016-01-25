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
    public class MultisiteDayRepository : Repository<IMultisiteDay>, IMultisiteDayRepository
    {
#pragma warning disable 618
        public MultisiteDayRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
#pragma warning restore 618
        {
        }

				public MultisiteDayRepository(ICurrentUnitOfWork currentUnitOfWork)
					: base(currentUnitOfWork)
	    {
	    }
		
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
		
        public ICollection<IMultisiteDay> GetAllMultisiteDays(DateOnlyPeriod period, ICollection<IMultisiteDay> multisiteDays, IMultisiteSkill skill, IScenario scenario, bool addToRepository = true)
        {
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
