using System;
using System.Collections;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Template;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using System.Linq;
using NHibernate.Transform;
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
                dates = uow.Session().GetNamedQuery("ValidationReport")
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

				budgetDates = uow.Session().GetNamedQuery("BudgetReport")
					.SetEntity("workload", workload)
					.SetEntity("scenario", scenario)
					.SetDateOnly("startDate", period.StartDate)
					.SetDateOnly("endDate", period.EndDate)
					.SetString("longtermKey", TemplateReference.LongtermTemplateKey)
					.SetResultTransformer(new SingleDateOnlyTransformer())
					.List<DateOnly>();

				detailedDates = uow.Session().GetNamedQuery("DetailReport")
					.SetEntity("workload", workload)
					.SetEntity("scenario", scenario)
					.SetDateOnly("startDate", period.StartDate)
					.SetDateOnly("endDate", period.EndDate)
					.SetString("longtermKey", TemplateReference.LongtermTemplateKey)
					.SetResultTransformer(new SingleDateOnlyTransformer())
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
				dates = uow.Session().GetNamedQuery("DetailReport")
					.SetEntity("workload", workload)
					.SetEntity("scenario", scenario)
					.SetDateOnly("startDate", period.StartDate)
					.SetDateOnly("endDate", period.EndDate)
					.SetString("longtermKey", TemplateReference.LongtermTemplateKey)
					.SetResultTransformer(new SingleDateOnlyTransformer())
					.List<DateOnly>();
			}

            var periods = new DatesToPeriod().Convert(dates);
            var forecasterReport = new ForecastProcessReport(periods);
            return new List<IForecastProcessReport> {forecasterReport};
        }
    }

	public class SingleDateOnlyTransformer : IResultTransformer
	{
		public object TransformTuple(object[] tuple, string[] aliases)
		{
			return new DateOnly((DateTime) tuple[0]);
		}

		public IList TransformList(IList collection)
		{
			return collection.Cast<DateOnly>().ToList();
		}
	}
}
