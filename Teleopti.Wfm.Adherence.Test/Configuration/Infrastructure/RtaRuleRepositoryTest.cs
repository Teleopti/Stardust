using System;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Wfm.Adherence.Configuration;
using Teleopti.Wfm.Adherence.Configuration.Repositories;
using Description = Teleopti.Wfm.Adherence.Configuration.Description;

namespace Teleopti.Wfm.Adherence.Test.Configuration.Infrastructure
{
    [TestFixture]
    [Category("BucketB")]
    public class RtaRuleRepositoryTest : RepositoryTest<IRtaRule>
    {
        protected override void ConcreteSetup()
        {
        }  

        protected override IRtaRule CreateAggregateWithCorrectBusinessUnit()
        {
            return new RtaRule(new Description("Wrong state"), Color.DarkTurquoise, TimeSpan.FromSeconds(15).Seconds, 0.8)
            {
	            Adherence = Adherence.Configuration.Adherence.In
            };
        }

        protected override void VerifyAggregateGraphProperties(IRtaRule saved, IRtaRule loaded)
        {
            IRtaRule org = CreateAggregateWithCorrectBusinessUnit();
            Assert.AreEqual(org.Description.Name, loaded.Description.Name);
            Assert.AreEqual(org.DisplayColor.ToArgb(), loaded.DisplayColor.ToArgb());
            Assert.AreEqual(org.ThresholdTime,loaded.ThresholdTime);
            Assert.AreEqual(0.8, loaded.StaffingEffect );
			Assert.AreEqual(Adherence.Configuration.Adherence.In, loaded.Adherence);
        }

        protected override Repository<IRtaRule> TestRepository(ICurrentUnitOfWork currentUnitOfWork)
        {
            return new RtaRuleRepository(currentUnitOfWork);
        }

	    [Test]
	    public void ShouldNotBeAlarmByDefault()
	    {
		    var rule = new RtaRule(new Description("."), Color.AliceBlue, 0, 0);
			PersistAndRemoveFromUnitOfWork(rule);
			
			var loaded = new RtaRuleRepository(CurrentUnitOfWork).LoadAll().Single();

			loaded.IsAlarm.Should().Be.False();
	    }

	    [Test]
	    public void ShouldChangeToNotAnAlarm()
	    {
		    var rule = new RtaRule(new Description("."), Color.AliceBlue, 0, 0);
			PersistAndRemoveFromUnitOfWork(rule);
		    rule.IsAlarm = false;
			PersistAndRemoveFromUnitOfWork(rule);
			
			var loaded = new RtaRuleRepository(CurrentUnitOfWork).LoadAll().Single();

			loaded.IsAlarm.Should().Be.False();
	    }

		[Test]
		public void ShouldHaveAlarmColorTheSameAsRuleColorByDefault()
		{
			var rule = new RtaRule(new Description("."), Color.AliceBlue, 0, 0);
			PersistAndRemoveFromUnitOfWork(rule);

			var loaded = new RtaRuleRepository(CurrentUnitOfWork).LoadAll().Single();

			loaded.AlarmColor.ToArgb().Should().Be(Color.AliceBlue.ToArgb());
		}

		[Test]
		public void ShouldHaveNewAlarmColor()
		{
			var rule = new RtaRule(new Description("."), Color.AliceBlue, 0, 0);
			PersistAndRemoveFromUnitOfWork(rule);
			rule.AlarmColor = Color.AntiqueWhite;
			PersistAndRemoveFromUnitOfWork(rule);

			var loaded = new RtaRuleRepository(CurrentUnitOfWork).LoadAll().Single();

			loaded.AlarmColor.ToArgb().Should().Be(Color.AntiqueWhite.ToArgb());
		}
		
	}
}
