using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Infrastructure.Persisters.Schedules;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Persisters.Schedules
{
	[TestFixture]
	public class ScheduleDictionaryPersisterTest
	{
		//just some mock tests to see that schedulerangepersister is called correctly (which is integration tested)
		
		[Test]
		public void ShouldPersistEveryScheduleRange()
		{
			var sr1 = MockRepository.GenerateMock<IScheduleRange>();
			var sr2 = MockRepository.GenerateMock<IScheduleRange>();
			var persister = MockRepository.GenerateMock<IScheduleRangePersister>();
			var dic = createScheduleDictionaryWith(sr1, sr2);
			var target = new ScheduleDictionaryPersister(persister);
			target.Persist(dic);

			persister.AssertWasCalled(x => x.Persist(sr1));
			persister.AssertWasCalled(x => x.Persist(sr2));
		}

		[Test]
		public void ShouldCollectAllConflicts()
		{
			var persister = MockRepository.GenerateMock<IScheduleRangePersister>();
			var sr1 = MockRepository.GenerateMock<IScheduleRange>();
			var sr2 = MockRepository.GenerateMock<IScheduleRange>();
			persister.Expect(x => x.Persist(sr1)).Return(new List<PersistConflict> {createConflict(), createConflict()});
			persister.Expect(x => x.Persist(sr2)).Return(new List<PersistConflict> {createConflict()});
			var dic = createScheduleDictionaryWith(sr1, sr2);
			
			var target = new ScheduleDictionaryPersister(persister);
			var res = target.Persist(dic);

			res.Count().Should().Be.EqualTo(3);
		}

		private PersistConflict createConflict()
		{
			return new PersistConflict(new DifferenceCollectionItem<IPersistableScheduleData>(), new PersonAssignment(new Person(), new Scenario("d"), new DateOnly()));
		}

		private static IScheduleDictionary createScheduleDictionaryWith(params IScheduleRange[] ranges)
		{
			var ret = MockRepository.GenerateMock<IScheduleDictionary>();
			ret.Expect(x => x.Values).Return(ranges);
			return ret;
		}
	}
}