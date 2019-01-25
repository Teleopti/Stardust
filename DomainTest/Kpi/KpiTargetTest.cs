using System.Drawing;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Kpi;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.DomainTest.Kpi
{

    /// <summary>
    /// Test class for KpiTarget
    /// </summary>
    [TestFixture]
	[TestWithStaticDependenciesDONOTUSE]
	public class KpiTargetTest
    {
        private KpiTarget target;
        private Team targetTeam;
        /// <summary>
        /// Runs once for every test
        /// </summary>
        [SetUp]
        public void Setup()
        {
            target = new KpiTarget();
            target.MinValue = 10;
            target.TargetValue = 15;
            target.MaxValue = 20;
            targetTeam = new Team();
            targetTeam.SetDescription(new Description("Test", "TS"));
        }


        /// <summary>
        /// Verifies that an instance can be created.
        /// </summary>
        [Test]
        public void CanCreateKpi()
        {
            Assert.IsNotNull(target);
            Assert.IsNotNull(targetTeam);
            Assert.AreEqual(15,target.TargetValue);
        }

        /// <summary>
        /// Verifies that properties can be set.
        /// </summary>
        [Test]
        public void CanSetProperties()
        {
            target.LowerThanMinColor = Color.DarkSalmon;
            target.HigherThanMaxColor = Color.Violet;
            target.BetweenColor = Color.Yellow;
            target.Team = targetTeam;
            
            target.MinValue = 37;
            target.MaxValue = 500;
            target.TargetValue = 250;

            Assert.AreEqual(Color.DarkSalmon.ToArgb(), target.LowerThanMinColor.ToArgb());
            Assert.AreEqual(Color.Violet.ToArgb(), target.HigherThanMaxColor.ToArgb());
            Assert.AreEqual(Color.Yellow.ToArgb(), target.BetweenColor.ToArgb());

            Assert.AreEqual(37, target.MinValue);
            Assert.AreEqual(500, target.MaxValue);
            Assert.AreEqual(250, target.TargetValue);

            Assert.AreEqual(BusinessUnitFactory.BusinessUnitUsedInTest, target.BusinessUnit);
            
        }
        [Test]
        public void CanReadProperties()
        {
            target.KeyPerformanceIndicator = new KeyPerformanceIndicator();
            Assert.IsNotNull(target.KeyPerformanceIndicator);
            target.Team = targetTeam;
            Assert.IsNotNull(target.Team);
            Assert.IsNotEmpty(target.TeamDescription);
        }

        /// <summary>
        /// If you set the MinValue To a value higher than Target And/or MAx they will get the same value as Min
        /// </summary>
        [Test]
        public void MinCannotBeHigherThanTargetAndMax()
        {
            target.MinValue = 18;
            Assert.AreEqual(18, target.TargetValue);
            Assert.AreEqual(20, target.MaxValue);

            target.MinValue = 50;
            Assert.AreEqual(50, target.TargetValue);
            Assert.AreEqual(50,target.MaxValue);

        }

        /// <summary>
        /// If you set the MaxValue To a value lower than Target And/or Min they will get the same value as Max
        /// </summary>
        [Test]
        public void MaxCannotBeLowerThanTargetAndMin()
        {
            target.MaxValue = 13;
            Assert.AreEqual(13, target.TargetValue);
            Assert.AreEqual(10, target.MinValue);

            target.MaxValue = 5;
            Assert.AreEqual(5, target.TargetValue);
            Assert.AreEqual(5, target.MinValue);

        }

        /// <summary>
        /// If you set the TargetValue To a value lower than Min, Min will get the same value as Target
        /// </summary>
        [Test]
        public void TargetCannotBeLowerThanMin()
        {
            target.TargetValue = 7;
            Assert.AreEqual(7, target.MinValue);
            Assert.AreEqual(20, target.MaxValue);


        }

        /// <summary>
        /// If you set the TargetValue To a value higher than Max, Max will get the same value as Target
        /// </summary>
        [Test]
        public void TargetCannotBeHigherThanMax()
        {
            target.TargetValue = 25;
            Assert.AreEqual(25, target.MaxValue);
            Assert.AreEqual(10, target.MinValue);


        }
    }
}
