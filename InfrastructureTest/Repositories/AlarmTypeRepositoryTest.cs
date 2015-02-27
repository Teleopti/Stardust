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
    public class AlarmTypeRepositoryTest : RepositoryTest<IAlarmType>
    {
        protected override void ConcreteSetup()
        {
        }  

        /// <summary>
        /// Creates an aggregate using the Bu of logged in user.
        /// Should be a "full detailed" aggregate
        /// </summary>
        /// <returns></returns>
        protected override IAlarmType CreateAggregateWithCorrectBusinessUnit()
        {
            return new AlarmType(new Description("Wrong state"), Color.DarkTurquoise, TimeSpan.FromSeconds(15),0.8);
        }

        /// <summary>
        /// Verifies the aggregate graph properties.
        /// </summary>
        /// <param name="loadedAggregateFromDatabase">The loaded aggregate from database.</param>
        protected override void VerifyAggregateGraphProperties(IAlarmType loadedAggregateFromDatabase)
        {
            IAlarmType org = CreateAggregateWithCorrectBusinessUnit();
            Assert.AreEqual(org.Description.Name, loadedAggregateFromDatabase.Description.Name);
            Assert.AreEqual(org.DisplayColor.ToArgb(), loadedAggregateFromDatabase.DisplayColor.ToArgb());
            Assert.AreEqual(org.ThresholdTime,loadedAggregateFromDatabase.ThresholdTime);
            Assert.AreEqual(0.8, loadedAggregateFromDatabase.StaffingEffect );
        }

        protected override Repository<IAlarmType> TestRepository(IUnitOfWork unitOfWork)
        {
            return new AlarmTypeRepository(unitOfWork);
        }
    }
}
