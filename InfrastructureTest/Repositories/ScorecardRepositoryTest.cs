using NUnit.Framework;
using Teleopti.Ccc.Domain.Kpi;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
    [TestFixture, Category("LongRunning")]
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

        protected override Repository<IScorecard> TestRepository(IUnitOfWork unitOfWork)
        {
            return new ScorecardRepository(unitOfWork);
        }
    }
}
