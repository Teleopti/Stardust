using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Kpi;
using Teleopti.Ccc.Infrastructure.Repositories;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
    [TestFixture, Category("BucketB")]
    public class ScorecardRepositoryTest : RepositoryTest<IScorecard>
    {
        protected override void ConcreteSetup()
        {
        }

        protected override IScorecard CreateAggregateWithCorrectBusinessUnit()
        {
            Scorecard real = new Scorecard();
            real.Name = "DUMMYNAME";
            return real;
        }

        protected override void VerifyAggregateGraphProperties(IScorecard loadedAggregateFromDatabase)
        {
            Assert.IsNotNull(loadedAggregateFromDatabase);
            Assert.AreEqual("DUMMYNAME", CreateAggregateWithCorrectBusinessUnit().Name);
        }

        protected override Repository<IScorecard> TestRepository(ICurrentUnitOfWork currentUnitOfWork)
        {
            return ScorecardRepository.DONT_USE_CTOR(currentUnitOfWork.Current());
        }
    }
}
