using System;
using System.Drawing;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Wfm.Adherence.Configuration;
using Teleopti.Wfm.Adherence.Configuration.Repositories;
using Teleopti.Wfm.Adherence.Test.InfrastructureTesting;

namespace Teleopti.Wfm.Adherence.Test.Configuration.Infrastructure
{

    [TestFixture]
    [Category("BucketB")]
    public class RtaMapRepositoryTest : RepositoryTest<IRtaMap>
    {
        private IActivity activity;
        private IRtaStateGroup stateGroup;
        private IRtaRule _rtaRule;

    	protected override void ConcreteSetup()
        {
            stateGroup = new RtaStateGroup("state group", true, true);
            PersistAndRemoveFromUnitOfWork(stateGroup);

            _rtaRule = new RtaRule(new Description("alarma!"), Color.DodgerBlue, TimeSpan.FromSeconds(50).Seconds, 0.0);
            PersistAndRemoveFromUnitOfWork(_rtaRule);
        }

        /// <summary>
        /// Creates an aggregate using the Bu of logged in user.
        /// Should be a "full detailed" aggregate
        /// </summary>
        /// <returns></returns>
        protected override IRtaMap CreateAggregateWithCorrectBusinessUnit()
        {
			activity = new Activity("roger") { DisplayColor = Color.White };
			PersistAndRemoveFromUnitOfWork(activity);

            IRtaMap rtaMap = new RtaMap{StateGroup = stateGroup, Activity = activity.Id.Value};
            rtaMap.RtaRule = _rtaRule;
            return rtaMap;
        }

        /// <summary>
        /// Verifies the aggregate graph properties.
        /// </summary>
        /// <param name="loadedAggregateFromDatabase">The loaded aggregate from database.</param>
        protected override void VerifyAggregateGraphProperties(IRtaMap loadedAggregateFromDatabase)
        {
            IRtaMap org = CreateAggregateWithCorrectBusinessUnit();
            Assert.AreEqual(org.RtaRule.Id, loadedAggregateFromDatabase.RtaRule.Id);
            Assert.AreEqual(org.StateGroup.Id, loadedAggregateFromDatabase.StateGroup.Id);
        }

        protected override Repository<IRtaMap> TestRepository(ICurrentUnitOfWork currentUnitOfWork)
        {
            return new RtaMapRepository(currentUnitOfWork);
        }

        [Test]
        public void VerifyLoadAllCompleteGraph()
        {
            var rtaMap = CreateAggregateWithCorrectBusinessUnit();
            PersistAndRemoveFromUnitOfWork(rtaMap);

            var result = new RtaMapRepository(UnitOfWork).LoadAllCompleteGraph();
            Assert.AreEqual(1, result.Count);
            Assert.IsTrue(LazyLoadingManager.IsInitialized(result[0].Activity));
            Assert.IsTrue(LazyLoadingManager.IsInitialized(result[0].StateGroup));
            Assert.IsTrue(LazyLoadingManager.IsInitialized(result[0].RtaRule));
        }
    }
}
