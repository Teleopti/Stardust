using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Infrastructure.Persisters;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Persisters
{
	[TestFixture]
	public class ScheduleDictionaryModifiedCallbackTest
	{
		public void ShouldDoUnsafeSnapshotUpdateOnModifiedOnCallback()
		{
			var modifiedEntity = MockRepository.GenerateMock<IPersistableScheduleData>();
			var scheduleRange = MockRepository.GenerateMock<ScheduleRange>(MockRepository.GenerateMock<IScheduleDictionary>(), MockRepository.GenerateMock<IScheduleParameters>());
			var modifiedEntities = new[] { modifiedEntity };
			var target = new ScheduleDictionaryModifiedCallback();

			target.Callback(scheduleRange, modifiedEntities, new IPersistableScheduleData[] { }, new IPersistableScheduleData[] { });

			scheduleRange.AssertWasCalled(x => x.UnsafeSnapshotUpdate(modifiedEntity, true));
		}

		public void ShouldDoUnsafeSnapshotUpdateOnAddedOnCallback()
		{
			var addedEntity = MockRepository.GenerateMock<IPersistableScheduleData>();
			var addedEntities = new[] { addedEntity };
			var scheduleRange = MockRepository.GenerateMock<ScheduleRange>(MockRepository.GenerateMock<IScheduleDictionary>(), MockRepository.GenerateMock<IScheduleParameters>());
			var target = new ScheduleDictionaryModifiedCallback();

			target.Callback(scheduleRange, new IPersistableScheduleData[] { }, addedEntities, new IPersistableScheduleData[] { });

			scheduleRange.AssertWasCalled(x => x.UnsafeSnapshotUpdate(addedEntity, true));
		}
	}
}