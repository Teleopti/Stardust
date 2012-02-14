using System;
using System.Collections.Generic;
using System.Drawing;
using Teleopti.Analytics.Etl.Transformer;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Kpi;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Analytics.Etl.TransformerTest.FakeData
{
    public static class KpiTargetTeamFactory
    {
        public static IList<IKpiTarget> CreateKpiTargetCollection()
        {
            IList<IKpiTarget> retList = new List<IKpiTarget>();

            // Create Kpi and team
            KeyPerformanceIndicator kpi1 = new KeyPerformanceIndicator();
            ((IEntity) kpi1).SetId(Guid.NewGuid());
            KeyPerformanceIndicator kpi2 = new KeyPerformanceIndicator();
            ((IEntity) kpi2).SetId(Guid.NewGuid());
            Team team1 = new Team();
            ((IEntity) team1).SetId(Guid.NewGuid());
            Team team2 = new Team();
            ((IEntity) team2).SetId(Guid.NewGuid());

            KpiTarget kpiTarget1 = new KpiTarget();
            kpiTarget1.KeyPerformanceIndicator = kpi1;
            kpiTarget1.Team = team1;
            kpiTarget1.TargetValue = 5;
            kpiTarget1.MinValue = 4;
            kpiTarget1.MaxValue = 6;
            kpiTarget1.BetweenColor = Color.Yellow;
            kpiTarget1.HigherThanMaxColor = Color.Green;
            kpiTarget1.LowerThanMinColor = Color.Red;
            RaptorTransformerHelper.SetCreatedOn(kpiTarget1, DateTime.Now);
            retList.Add(kpiTarget1);

            KpiTarget kpiTarget2 = new KpiTarget();
            kpiTarget2.KeyPerformanceIndicator = kpi2;
            kpiTarget2.Team = team2;
            kpiTarget2.TargetValue = 100;
            kpiTarget2.MinValue = 40;
            kpiTarget2.MaxValue = 60;
            kpiTarget2.BetweenColor = Color.Blue;
            kpiTarget2.HigherThanMaxColor = Color.LightGreen;
            kpiTarget2.LowerThanMinColor = Color.DarkRed;
            RaptorTransformerHelper.SetCreatedOn(kpiTarget2, DateTime.Now);
            retList.Add(kpiTarget2);

            return retList;
        }
    }
}