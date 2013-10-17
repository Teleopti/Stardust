using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Infrastructure.Persisters.Schedules;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.Persisters.Schedules
{
	[TestFixture]
	public class NoChangesTest
	{
		private IScheduleRange rangeWithNoChanges;
		private IDifferenceCollectionService<IPersistableScheduleData> diffSvc;

		[SetUp]
		public void Setup()
		{
			diffSvc = MockRepository.GenerateMock<IDifferenceCollectionService<IPersistableScheduleData>>();
			rangeWithNoChanges = MockRepository.GenerateMock<IScheduleRange, IUnvalidatedScheduleRangeUpdate>();
			rangeWithNoChanges.Expect(x => x.DifferenceSinceSnapshot(diffSvc))
				 .Return(new DifferenceCollection<IPersistableScheduleData>());
		}

		[Test]
		public void NoChangesShouldResultInNoConflicts()
		{
			var sut = new ScheduleRangePersister(null, diffSvc, null, null);		
			sut.Persist(rangeWithNoChanges).Should().Be.Empty();
		}

		[Test]
		public void NoChangesShouldNotTouchDatabase()
		{
			var uowFactory = MockRepository.GenerateMock<IUnitOfWorkFactory>();
			var sut = new ScheduleRangePersister(uowFactory, diffSvc, null, null);
			sut.Persist(rangeWithNoChanges);
			uowFactory.AssertWasNotCalled(x => x.CreateAndOpenUnitOfWork());
		} 
	}
}