using NUnit.Framework;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

//using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
    [TestFixture]
    public class ScheduleTagRepositoryTest : RepositoryTest<IScheduleTag>
    {

        protected override IScheduleTag CreateAggregateWithCorrectBusinessUnit()
        {
            const string text = "Detta är en tag";
            
            var tag = new ScheduleTag {Description = text};

            return tag;
        }

        protected override void VerifyAggregateGraphProperties(IScheduleTag loadedAggregateFromDatabase)
        {
            var org = CreateAggregateWithCorrectBusinessUnit();
            
            if(loadedAggregateFromDatabase != null) Assert.AreEqual(org.Description, loadedAggregateFromDatabase.Description);
        }

        protected override Repository<IScheduleTag> TestRepository(ICurrentUnitOfWork currentUnitOfWork)
        {
            return new ScheduleTagRepository(currentUnitOfWork);
        }   
    }
}