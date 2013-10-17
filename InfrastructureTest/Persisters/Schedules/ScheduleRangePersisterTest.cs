using System.Collections.Generic;
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
	public class ScheduleRangePersisterTest
	{
		private IScheduleRangePersister sut;
		private IUnitOfWorkFactory uowFactory;
		private IDifferenceCollectionService<IPersistableScheduleData> diffSvc;
		private IScheduleRangeConflictCollector scheduleRangeConflictCollector;
		private IScheduleRangeSaver scheduleRangeSaver;
		private IScheduleRange range;

		[SetUp]
		public void Setup()
		{
			uowFactory = MockRepository.GenerateMock<IUnitOfWorkFactory>();
			diffSvc = MockRepository.GenerateMock<IDifferenceCollectionService<IPersistableScheduleData>>();
			scheduleRangeConflictCollector = MockRepository.GenerateMock<IScheduleRangeConflictCollector>();
			scheduleRangeSaver = MockRepository.GenerateMock<IScheduleRangeSaver>();
			sut = new ScheduleRangePersister(uowFactory, diffSvc, scheduleRangeConflictCollector, scheduleRangeSaver);
			range = MockRepository.GenerateMock<IScheduleRange, IUnvalidatedScheduleRangeUpdate>();
		}

		[Test]
		public void NoChangesShouldResultInNoConflicts()
		{
			range.Expect(x => x.DifferenceSinceSnapshot(diffSvc))
					 .Return(new DifferenceCollection<IPersistableScheduleData>());
			sut.Persist(range).Should().Be.Empty();
		}

		[Test]
		public void NoChangesShouldNotTouchDatabase()
		{
			range.Expect(x => x.DifferenceSinceSnapshot(diffSvc))
					 .Return(new DifferenceCollection<IPersistableScheduleData>());
			sut.Persist(range);
			uowFactory.AssertWasNotCalled(x => x.CreateAndOpenUnitOfWork());
		}

		[Test]
		public void ShouldReturnConflicts()
		{
			expectUowCreation();
			var conflicts = new List<PersistConflict> {new PersistConflict(), new PersistConflict()};
			scheduleRangeConflictCollector.Expect(x => x.GetConflicts(range))
			                              .Return(conflicts);
			range.Expect(x => x.DifferenceSinceSnapshot(diffSvc)).Return(new DifferenceCollection<IPersistableScheduleData>{new DifferenceCollectionItem<IPersistableScheduleData>()});
			sut.Persist(range).Should().Have.SameValuesAs(conflicts);
		}

		[Test]
		public void ShouldSaveWhenNoConflicts()
		{
			var diff = new DifferenceCollection<IPersistableScheduleData>{new DifferenceCollectionItem<IPersistableScheduleData>()};
			range.Expect(x => x.DifferenceSinceSnapshot(diffSvc)).Return(diff);
			expectUowCreation();
			sut.Persist(range);
			scheduleRangeSaver.AssertWasCalled(x => x.SaveChanges(diff, (IUnvalidatedScheduleRangeUpdate)range));
		}

		[Test]
		public void ShouldCommitAndDisposeWhenNoConflicts()
		{
			var uow = expectUowCreation();
			range.Expect(x => x.DifferenceSinceSnapshot(diffSvc)).Return(new DifferenceCollection<IPersistableScheduleData> { new DifferenceCollectionItem<IPersistableScheduleData>() });
			sut.Persist(range);
			uow.AssertWasCalled(x => x.PersistAll());
			uow.AssertWasCalled(x => x.Dispose());
		}

		private IUnitOfWork expectUowCreation()
		{
			var uow = MockRepository.GenerateMock<IUnitOfWork>();
			uowFactory.Expect(x => x.CreateAndOpenUnitOfWork()).Return(uow);
			return uow;
		}
	}
}