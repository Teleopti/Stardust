using System;
using System.Drawing;
using System.Linq;
using Autofac;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Wfm.Adherence.Configuration;
using Teleopti.Wfm.Adherence.Configuration.Repositories;
using Description = Teleopti.Wfm.Adherence.Configuration.Description;

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
			activity.SetBusinessUnit(BusinessUnit);
			PersistAndRemoveFromUnitOfWork(activity);

            IRtaMap rtaMap = new RtaMap{StateGroup = stateGroup, Activity = activity.Id.Value};
            rtaMap.RtaRule = _rtaRule;
            return rtaMap;
        }

		/// <summary>
		/// Verifies the aggregate graph properties.
		/// </summary>
		/// <param name="saved"></param>
		/// <param name="loaded"></param>
		protected override void VerifyAggregateGraphProperties(IRtaMap saved, IRtaMap loaded)
        {
            IRtaMap org = CreateAggregateWithCorrectBusinessUnit();
            Assert.AreEqual(org.RtaRule.Id, loaded.RtaRule.Id);
            Assert.AreEqual(org.StateGroup.Id, loaded.StateGroup.Id);
        }

        protected override Repository<IRtaMap> ResolveRepository()
		{
			return Container.Resolve<Adherence.Configuration.IRepository<IRtaMap>>() as Repository<IRtaMap> ;
        }

        [Test]
        public void VerifyLoadAllCompleteGraph()
        {
            var rtaMap = CreateAggregateWithCorrectBusinessUnit();
            PersistAndRemoveFromUnitOfWork(rtaMap);

            var result = (ResolveRepository() as RtaMapRepository).LoadAllCompleteGraph();
            Assert.AreEqual(1, result.Count());
			Session.Close();
			result.Single().Activity.ToString();
			result.Single().StateGroup.ToString();
			result.Single().RtaRule.ToString();
        }
    }
}
