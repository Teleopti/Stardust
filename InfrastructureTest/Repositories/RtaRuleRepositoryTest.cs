using System;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.RealTimeAdherence;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
    [TestFixture]
    [Category("LongRunning")]
    public class RtaRuleRepositoryTest : RepositoryTest<IRtaRule>
    {
        protected override void ConcreteSetup()
        {
        }  

        protected override IRtaRule CreateAggregateWithCorrectBusinessUnit()
        {
            return new RtaRule(new Description("Wrong state"), Color.DarkTurquoise, TimeSpan.FromSeconds(15),0.8)
            {
	            Adherence = Adherence.In
            };
        }

        protected override void VerifyAggregateGraphProperties(IRtaRule loadedAggregateFromDatabase)
        {
            IRtaRule org = CreateAggregateWithCorrectBusinessUnit();
            Assert.AreEqual(org.Description.Name, loadedAggregateFromDatabase.Description.Name);
            Assert.AreEqual(org.DisplayColor.ToArgb(), loadedAggregateFromDatabase.DisplayColor.ToArgb());
            Assert.AreEqual(org.ThresholdTime,loadedAggregateFromDatabase.ThresholdTime);
            Assert.AreEqual(0.8, loadedAggregateFromDatabase.StaffingEffect );
			Assert.AreEqual(Adherence.In, loadedAggregateFromDatabase.Adherence);
        }

        protected override Repository<IRtaRule> TestRepository(ICurrentUnitOfWork currentUnitOfWork)
        {
            return new RtaRuleRepository(currentUnitOfWork);
        }

	    [Test]
	    public void ShouldBeAlarmByDefault()
	    {
		    var rule = new RtaRule(new Description("."), Color.AliceBlue, TimeSpan.Zero, 0);
			PersistAndRemoveFromUnitOfWork(rule);
			
			var loaded = new RtaRuleRepository(UnitOfWork).LoadAll().Single();

			loaded.IsAlarm.Should().Be.True();
	    }

	    [Test]
	    public void ShouldChangeToNotAnAlarm()
	    {
		    var rule = new RtaRule(new Description("."), Color.AliceBlue, TimeSpan.Zero, 0);
			PersistAndRemoveFromUnitOfWork(rule);
		    rule.IsAlarm = false;
			PersistAndRemoveFromUnitOfWork(rule);
			
			var loaded = new RtaRuleRepository(UnitOfWork).LoadAll().Single();

			loaded.IsAlarm.Should().Be.False();
	    }
    }
}
