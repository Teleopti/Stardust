using System.Collections.Generic;
using NHibernate;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Template;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
    public static class ForecastProcessReportRepository
    {
        public static IList<IForecastProcessReport> ValidationReport(IWorkload workload, DateOnlyPeriod period)
        {
            IList<DateOnly> dates;
            using (IStatelessUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenStatelessUnitOfWork())
            {
                dates = session(uow).GetNamedQuery("ValidationReport")
                            .SetEntity("workload", workload)
							.SetDateOnly("startDate", period.StartDate)
							.SetDateOnly("endDate", period.EndDate)
                            .List<DateOnly>();
            }

            var periods = new DatesToPeriod().Convert(dates);
            ForecastProcessReport forecasterReport = new ForecastProcessReport(periods);
            return new List<IForecastProcessReport> {forecasterReport};
        }


        public static IList<IForecastProcessReport> BudgetReport(IScenario scenario, IWorkload workload, DateOnlyPeriod period)
        {
            IList<DateOnly> budgetDates;
            IList<DateOnly> detailedDates;
            using (IStatelessUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenStatelessUnitOfWork())
            {

                budgetDates = session(uow).GetNamedQuery("BudgetReport")
                            .SetEntity("workload", workload)
                            .SetEntity("scenario", scenario)
							.SetDateOnly("startDate", period.StartDate)
							.SetDateOnly("endDate", period.EndDate)
                            .SetString("longtermKey", TemplateReference.LongtermTemplateKey)
                            .List<DateOnly>();

                detailedDates = session(uow).GetNamedQuery("DetailReport")
                            .SetEntity("workload", workload)
                            .SetEntity("scenario", scenario)
							.SetDateOnly("startDate", period.StartDate)
							.SetDateOnly("endDate", period.EndDate)
                            .SetString("longtermKey", TemplateReference.LongtermTemplateKey)
                            .List<DateOnly>();
            }

            ICollection<DateOnlyPeriod> periods = new DatesToPeriod().Convert(budgetDates.Union(detailedDates));
            ForecastProcessReport forecasterReport = new ForecastProcessReport(periods);
            return new List<IForecastProcessReport> {forecasterReport};
        }

        public static IList<IForecastProcessReport> DetailReport(IScenario scenario, IWorkload workload, DateOnlyPeriod period)
        {
            IList<DateOnly> dates;
            using (IStatelessUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenStatelessUnitOfWork())
            {
                dates = session(uow).GetNamedQuery("DetailReport")
                            .SetEntity("workload", workload)
                            .SetEntity("scenario", scenario)
							.SetDateOnly("startDate", period.StartDate)
							.SetDateOnly("endDate", period.EndDate)
                            .SetString("longtermKey", TemplateReference.LongtermTemplateKey)
                            .List<DateOnly>();
            }

            var periods = new DatesToPeriod().Convert(dates);
            var forecasterReport = new ForecastProcessReport(periods);
            return new List<IForecastProcessReport> {forecasterReport};
        }

        private static IStatelessSession session(IStatelessUnitOfWork uow)
        {
            return ((NHibernateStatelessUnitOfWork)uow).Session;
        }
    }
}
