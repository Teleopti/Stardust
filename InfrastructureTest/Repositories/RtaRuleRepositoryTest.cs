using System;
using System.Drawing;
using NUnit.Framework;
using Teleopti.Ccc.Domain.RealTimeAdherence;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
    ///<summary>
    /// Tests AlarmTypeRepository
    ///</summary>
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
    }
}
