using System;
using System.Collections.Generic;
using Teleopti.Analytics.Etl.Common.Transformer;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Kpi;

namespace Teleopti.Analytics.Etl.CommonTest.Transformer.FakeData
{
	public static class ScorecardFactory
	{
		public static IList<IScorecard> CreateScorecardCollection(DateTime updatedOnDateTime)
		{
			IList<IScorecard> retList = new List<IScorecard>();
			IScorecardPeriod scorecardPeriodWeek = new ScorecardPeriod(1);
			IScorecardPeriod scorecardPeriodQuarter = new ScorecardPeriod(3);

			IScorecard scorecard1 = new Scorecard();
			scorecard1.SetId(Guid.NewGuid());
			RaptorTransformerHelper.SetUpdatedOn(scorecard1, DateTime.Now);
			RaptorTransformerHelper.SetUpdatedOn(scorecard1, updatedOnDateTime);
			scorecard1.Name = "scorecard week";
			scorecard1.Period = scorecardPeriodWeek;
			scorecard1.AddKpi(CreateKpi());
			retList.Add(scorecard1);

			IScorecard scorecard2 = new Scorecard();
			scorecard2.SetId(Guid.NewGuid());
			RaptorTransformerHelper.SetUpdatedOn(scorecard2, DateTime.Now);
			RaptorTransformerHelper.SetUpdatedOn(scorecard2, updatedOnDateTime);
			scorecard2.Name = "scorecard quarter";
			scorecard2.Period = scorecardPeriodQuarter;
			scorecard2.AddKpi(CreateKpi());
			retList.Add(scorecard2);

			return retList;
		}

		private static IKeyPerformanceIndicator CreateKpi()
		{
			IKeyPerformanceIndicator kpi = new KeyPerformanceIndicator();
			kpi.SetId(Guid.NewGuid());
			return kpi;
		}
	}
}