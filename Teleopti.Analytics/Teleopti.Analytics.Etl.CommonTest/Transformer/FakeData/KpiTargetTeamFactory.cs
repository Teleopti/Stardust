using System;
using System.Collections.Generic;
using System.Drawing;
using Teleopti.Analytics.Etl.Common.Transformer;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Kpi;

namespace Teleopti.Analytics.Etl.CommonTest.Transformer.FakeData
{
    public static class KpiTargetTeamFactory
    {
        public static IList<IKpiTarget> CreateKpiTargetCollection()
        {
            IList<IKpiTarget> retList = new List<IKpiTarget>();

            // Create Kpi and team
            var kpi1 = new KeyPerformanceIndicator();
            ((IEntity) kpi1).SetId(Guid.NewGuid());
            var kpi2 = new KeyPerformanceIndicator();
            ((IEntity) kpi2).SetId(Guid.NewGuid());
            var team1 = new Team();
            ((IEntity) team1).SetId(Guid.NewGuid());
            var team2 = new Team();
            ((IEntity) team2).SetId(Guid.NewGuid());

            var kpiTarget1 = new KpiTarget
            {
	            KeyPerformanceIndicator = kpi1,
	            Team = team1,
	            TargetValue = 5,
	            MinValue = 4,
	            MaxValue = 6,
	            BetweenColor = Color.Yellow,
	            HigherThanMaxColor = Color.Green,
	            LowerThanMinColor = Color.Red
            };
	        RaptorTransformerHelper.SetUpdatedOn(kpiTarget1, DateTime.Now);
            retList.Add(kpiTarget1);

            var kpiTarget2 = new KpiTarget();
            kpiTarget2.KeyPerformanceIndicator = kpi2;
            kpiTarget2.Team = team2;
            kpiTarget2.TargetValue = 100;
            kpiTarget2.MinValue = 40;
            kpiTarget2.MaxValue = 60;
            kpiTarget2.BetweenColor = Color.Blue;
            kpiTarget2.HigherThanMaxColor = Color.LightGreen;
            kpiTarget2.LowerThanMinColor = Color.DarkRed;
            RaptorTransformerHelper.SetUpdatedOn(kpiTarget2, DateTime.Now);
            retList.Add(kpiTarget2);

            return retList;
        }
    }
}