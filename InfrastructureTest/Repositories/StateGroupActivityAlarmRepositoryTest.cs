using System;
using System.Drawing;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.RealTimeAdherence;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
    ///<summary>
    /// Tests StateGroupActivityAlarmRepository
    ///</summary>
    [TestFixture]
    [Category("LongRunning")]
    public class StateGroupActivityAlarmRepositoryTest : RepositoryTest<IStateGroupActivityAlarm>
    {
        private IActivity activity;
        private IRtaStateGroup stateGroup;
        private IRtaRule _rtaRule;

    	protected override void ConcreteSetup()
        {
            stateGroup = new RtaStateGroup("state group", true, true);
            PersistAndRemoveFromUnitOfWork(stateGroup);

            _rtaRule = new RtaRule(new Description("alarma!"), Color.DodgerBlue, TimeSpan.FromSeconds(50),0.0);
            PersistAndRemoveFromUnitOfWork(_rtaRule);
        }

        /// <summary>
        /// Creates an aggregate using the Bu of logged in user.
        /// Should be a "full detailed" aggregate
        /// </summary>
        /// <returns></returns>
        protected override IStateGroupActivityAlarm CreateAggregateWithCorrectBusinessUnit()
        {
			activity = new Activity("roger") { DisplayColor = Color.White };
			PersistAndRemoveFromUnitOfWork(activity);

            IStateGroupActivityAlarm stateGroupActivityAlarm = new StateGroupActivityAlarm(stateGroup, activity);
            stateGroupActivityAlarm.RtaRule = _rtaRule;
            return stateGroupActivityAlarm;
        }

        /// <summary>
        /// Verifies the aggregate graph properties.
        /// </summary>
        /// <param name="loadedAggregateFromDatabase">The loaded aggregate from database.</param>
        protected override void VerifyAggregateGraphProperties(IStateGroupActivityAlarm loadedAggregateFromDatabase)
        {
            IStateGroupActivityAlarm org = CreateAggregateWithCorrectBusinessUnit();
            Assert.AreEqual(org.Activity.Description.Name, loadedAggregateFromDatabase.Activity.Description.Name);
            Assert.AreEqual(org.RtaRule.Id, loadedAggregateFromDatabase.RtaRule.Id);
            Assert.AreEqual(org.StateGroup.Id, loadedAggregateFromDatabase.StateGroup.Id);
        }

        protected override Repository<IStateGroupActivityAlarm> TestRepository(ICurrentUnitOfWork currentUnitOfWork)
        {
            return new StateGroupActivityAlarmRepository(currentUnitOfWork);
        }

        [Test]
        public void VerifyLoadAllCompleteGraph()
        {
            var stateGroupActivityAlarm = CreateAggregateWithCorrectBusinessUnit();
            PersistAndRemoveFromUnitOfWork(stateGroupActivityAlarm);

            var result = new StateGroupActivityAlarmRepository(UnitOfWork).LoadAllCompleteGraph();
            Assert.AreEqual(1, result.Count);
            Assert.IsTrue(LazyLoadingManager.IsInitialized(result[0].Activity));
            Assert.IsTrue(LazyLoadingManager.IsInitialized(result[0].StateGroup));
            Assert.IsTrue(LazyLoadingManager.IsInitialized(result[0].RtaRule));
        }
    }
}
