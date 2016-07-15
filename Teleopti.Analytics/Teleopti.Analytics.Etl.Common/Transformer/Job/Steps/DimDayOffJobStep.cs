using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Analytics;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories.Analytics;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;
using IJobResult = Teleopti.Analytics.Etl.Common.Interfaces.Transformer.IJobResult;

namespace Teleopti.Analytics.Etl.Common.Transformer.Job.Steps
{
	public class DimDayOffJobStep : JobStepBase
	{
		private readonly IAnalyticsDayOffRepository _analyticsDayOffRepository;
		private readonly IDayOffTemplateRepository _dayOffTemplateRepository;
		private readonly IAnalyticsBusinessUnitRepository _analyticsBusinessUnitRepository;

		public DimDayOffJobStep(IJobParameters jobParameters)
			: base(jobParameters)
		{
			_dayOffTemplateRepository = new DayOffTemplateRepository(CurrentUnitOfWork.Make());
			_analyticsDayOffRepository = new AnalyticsDayOffRepository(CurrentAnalyticsUnitOfWork.Make());
			_analyticsBusinessUnitRepository = new AnalyticsBusinessUnitRepository(CurrentAnalyticsUnitOfWork.Make());
			Name = "dim_day_off";
		}

		protected override int RunStep(IList<IJobResult> jobResultCollection, bool isLastBusinessUnit)
		{
			using (JobParameters.Helper.SelectedDataSource.Application.CreateAndOpenUnitOfWork())
			using (var auow = JobParameters.Helper.SelectedDataSource.Analytics.CreateAndOpenUnitOfWork())
			{
				var changedRows = 0;
				var dayOffs = _dayOffTemplateRepository.FindAllDayOffsSortByDescription();
				var analyticsDayOffs = _analyticsDayOffRepository.DayOffs();
				var analyticsCurrentBusinessUnit = _analyticsBusinessUnitRepository.Get(RaptorTransformerHelper.CurrentBusinessUnit.Id.GetValueOrDefault());

				foreach (var dayOffTemplate in dayOffs)
				{
					if (analyticsDayOffs.Any(a => a.DayOffCode == dayOffTemplate.Id.GetValueOrDefault() &&
												  a.DatasourceUpdateDate == DateHelper.GetSmallDateTime(dayOffTemplate.UpdatedOn.GetValueOrDefault()) &&
												  a.BusinessUnitId == analyticsCurrentBusinessUnit.DatasourceId))
						continue;

					changedRows++;
					_analyticsDayOffRepository.AddOrUpdate(new AnalyticsDayOff
					{
						DayOffCode = dayOffTemplate.Id.GetValueOrDefault(),
						DayOffName = dayOffTemplate.Description.Name,
						DayOffShortname = dayOffTemplate.Description.ShortName,
						BusinessUnitId = analyticsCurrentBusinessUnit.BusinessUnitId,
						DatasourceId = 1,
						DatasourceUpdateDate = dayOffTemplate.UpdatedOn.GetValueOrDefault(DateTime.Today),
						DisplayColor = -8355712,
						DisplayColorHtml = "#808080",
					});
				}

				if (analyticsDayOffs.All(a => a.DayOffId != -1))
				{
					changedRows++;
					_analyticsDayOffRepository.AddNotDefined();
				}

				auow.PersistAll();
				return changedRows;
			}
		}
	}
}