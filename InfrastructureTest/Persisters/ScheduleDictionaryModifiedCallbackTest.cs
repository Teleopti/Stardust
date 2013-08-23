using System.ComponentModel;
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
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
		public void ShouldDoUnsafeSnapshotUpdateOnModifiedOnCallback()
		{
			var modifiedEntity = MockRepository.GenerateMock<IPersistableScheduleData>();
			var scheduleRange = MockRepository.GenerateMock<ScheduleRange>(MockRepository.GenerateMock<IScheduleDictionary>(), MockRepository.GenerateMock<IScheduleParameters>());
			var modifiedEntities = new[] { modifiedEntity };
			var target = new ScheduleDictionaryModifiedCallback();

			target.Callback(scheduleRange, modifiedEntities, new IPersistableScheduleData[] { }, new IPersistableScheduleData[] { });

			scheduleRange.AssertWasCalled(x => x.SolveConflictBecauseOfExternalUpdate(modifiedEntity, true));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
		public void ShouldDoUnsafeSnapshotUpdateOnAddedOnCallback()
		{
			var addedEntity = MockRepository.GenerateMock<IPersistableScheduleData>();
			var addedEntities = new[] { addedEntity };
			var scheduleRange = MockRepository.GenerateMock<ScheduleRange>(MockRepository.GenerateMock<IScheduleDictionary>(), MockRepository.GenerateMock<IScheduleParameters>());
			var target = new ScheduleDictionaryModifiedCallback();

			target.Callback(scheduleRange, new IPersistableScheduleData[] { }, addedEntities, new IPersistableScheduleData[] { });

			scheduleRange.AssertWasCalled(x => x.SolveConflictBecauseOfExternalUpdate(addedEntity, true));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
		public void ShouldTakeSnapshotOfRange()
		{
			var scheduleRange = MockRepository.GenerateMock<IScheduleRange>();
			var target = new ScheduleDictionaryModifiedCallback();
			var entities = new IPersistableScheduleData[] {};

			target.Callback(scheduleRange, entities, entities, entities);

			scheduleRange.AssertWasCalled(x => x.TakeSnapshot());
		}

	}
}