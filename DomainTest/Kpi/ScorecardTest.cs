using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Kpi;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.DomainTest.Kpi
{
    [TestFixture]
	[TestWithStaticDependenciesDONOTUSE]
	public class ScorecardTest
    {
        private Scorecard  target;
        private KeyPerformanceIndicator kpi1;
        private KeyPerformanceIndicator kpi2;
        private IEnumerable<IScorecardPeriod> lstPeriods;

        [SetUp]
        public void Setup()
        {
            target = new Scorecard();
            target.Name = "NEW SCORE";
            kpi1 = new KeyPerformanceIndicator();
            kpi2 = new KeyPerformanceIndicator();
            lstPeriods = ScorecardPeriodService.ScorecardPeriodList();
        }

        /// <summary>
        /// Verifies that an instance can be created.
        /// </summary>
        [Test]
        public void CanCreateScorecard()
        {
            Assert.IsNotNull(target);
            Assert.AreEqual("NEW SCORE", target.Name);
        }

        /// <summary>
        /// Verifies that properties can be set.
        /// </summary>
        [Test]
        public void CanSetProperties()
        {
            target.Name = "SOME NEW NAME";
            target.AddKpi(kpi1);
            target.AddKpi(kpi2);
            target.Period = lstPeriods.ElementAt(2);

            Assert.AreEqual("SOME NEW NAME", target.Name);
            Assert.AreEqual(2,target.KeyPerformanceIndicatorCollection.Count);
            Assert.AreEqual(kpi1,target.KeyPerformanceIndicatorCollection[0]);
            Assert.AreEqual(target.Period.Id, lstPeriods.ElementAt(2).Id);

            target.RemoveKpi(kpi1);
            Assert.AreEqual(1,target.KeyPerformanceIndicatorCollection.Count);
            Assert.AreEqual(kpi2,target.KeyPerformanceIndicatorCollection[0]);

            Assert.AreEqual(BusinessUnitUsedInTests.BusinessUnit, target.GetOrFillWithBusinessUnit_DONTUSE());

        }
        [Test]
        public void CanReadPeriod()
        {
            var per = new ScorecardPeriod(4);
            target.Period = per;
            Assert.AreEqual(4,target.Period.Id);

        }
        [Test]
        public void CanReadProperties()
        {
            Assert.IsNotEmpty(kpi1.Name);
            kpi1 = new KeyPerformanceIndicator("Name", "KpiAverageHandleTime", EnumTargetValueType.TargetValueTypeNumber, 50, 40, 60, Color.DodgerBlue, Color.DeepPink, Color.DarkSlateBlue);
            Assert.IsNotNull(kpi1.KpiTargetCollection);
            Assert.IsNotEmpty(kpi1.Name);
            Assert.IsNotEmpty(kpi1.ToString());

        }
    }
}
