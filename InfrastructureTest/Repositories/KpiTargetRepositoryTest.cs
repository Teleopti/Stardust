using NUnit.Framework;
using Teleopti.Ccc.Domain.Kpi;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
    [TestFixture, Category("LongRunning")]
    public class KpiTargetRepositoryTest : RepositoryTest<IKpiTarget>
    {
        private ITeam _team;
        private IKeyPerformanceIndicator _kpi;

        protected override void ConcreteSetup()
        {
            _team = TeamFactory.CreateTeam("as", "for test");
            PersistAndRemoveFromUnitOfWork(_team.Site);
            PersistAndRemoveFromUnitOfWork(_team);

            _kpi = new KeyPerformanceIndicator();
            PersistAndRemoveFromUnitOfWork(_kpi);
        }

        protected override IKpiTarget CreateAggregateWithCorrectBusinessUnit()
        {
			var kpi = new KeyPerformanceIndicator();
			PersistAndRemoveFromUnitOfWork(kpi);
            var real = new KpiTarget {KeyPerformanceIndicator = kpi, Team = _team, TargetValue = 100};

        	return real;
        }

        protected override void VerifyAggregateGraphProperties(IKpiTarget loadedAggregateFromDatabase)
        {
            IKpiTarget real = CreateAggregateWithCorrectBusinessUnit();
            Assert.AreEqual(real.TargetValue, loadedAggregateFromDatabase.TargetValue);
        }

        protected override Repository<IKpiTarget> TestRepository(ICurrentUnitOfWork currentUnitOfWork)
        {
            return new KpiTargetRepository(currentUnitOfWork.Current());
        }
    }
}
