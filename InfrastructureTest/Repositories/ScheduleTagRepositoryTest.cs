using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Infrastructure.Repositories;

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
            
            Assert.AreEqual(org.Description, loadedAggregateFromDatabase.Description);
        }

        protected override Repository<IScheduleTag> TestRepository(ICurrentUnitOfWork currentUnitOfWork)
        {
            return ScheduleTagRepository.DONT_USE_CTOR(currentUnitOfWork);
        }

		[Test]
	    public void ShouldFindAllSorted()
		{
			var tag = CreateAggregateWithCorrectBusinessUnit();
			var tagFirst = CreateAggregateWithCorrectBusinessUnit();
			tagFirst.Description = "A";
			PersistAndRemoveFromUnitOfWork(tag);
			PersistAndRemoveFromUnitOfWork(tagFirst);

			var repository = ScheduleTagRepository.DONT_USE_CTOR(UnitOfWork);
			repository.FindAllScheduleTags()[0].Description.Should().Be.EqualTo("A");
		}
    }
}