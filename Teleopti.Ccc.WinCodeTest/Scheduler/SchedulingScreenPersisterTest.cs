using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Persisters;
using Teleopti.Ccc.Infrastructure.Persisters.Account;
using Teleopti.Ccc.Infrastructure.Persisters.Requests;
using Teleopti.Ccc.Infrastructure.Persisters.WriteProtection;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling;

namespace Teleopti.Ccc.WinCodeTest.Scheduler
{
	[TestFixture]
	public class SchedulingScreenPersisterTest
	{
		//just mock tests to see that integration tested services are all called.

		private ISchedulingScreenPersister target;
		private IScheduleDictionaryPersister scheduleDictionaryPersister;
		private IRequestPersister requestPersister;
		private IWriteProtectionPersister writeProtectionPersister;
		private IWorkflowControlSetPublishDatePersister _workflowControlSetPublishDatePersister;

		[SetUp]
		public void Setup()
		{
			scheduleDictionaryPersister = MockRepository.GenerateMock<IScheduleDictionaryPersister>();
			requestPersister = MockRepository.GenerateMock<IRequestPersister>();
			writeProtectionPersister = MockRepository.GenerateMock<IWriteProtectionPersister>();
			_workflowControlSetPublishDatePersister = MockRepository.GenerateMock<IWorkflowControlSetPublishDatePersister>();
			target = new SchedulingScreenPersister(scheduleDictionaryPersister, requestPersister, writeProtectionPersister, _workflowControlSetPublishDatePersister);
		}

		[Test]
		public void ShouldPersistRequests()
		{
			IEnumerable<PersistConflict> foo;
			var requests = new List<IPersonRequest>();
			target.TryPersist(null, requests, null, null, out foo);

			requestPersister.AssertWasCalled(x => x.Persist(requests));
		}

		[Test]
		public void ShouldPersistWriteProtections()
		{
			IEnumerable<PersistConflict> foo;
			var writeProtections = new List<IPersonWriteProtectionInfo>();
			target.TryPersist(null, null, writeProtections, null, out foo);

			writeProtectionPersister.AssertWasCalled(x => x.Persist(writeProtections));
		}

		[Test]
		public void ShouldPersistWorkflowControlSetPublishDate()
		{
			IEnumerable<PersistConflict> foo;
			var workflowControlSets = new List<IWorkflowControlSet>();
			target.TryPersist(null, null, null, workflowControlSets, out foo);

			_workflowControlSetPublishDatePersister.AssertWasCalled(x => x.Persist(workflowControlSets));	
		}

		[Test]
		public void ShouldPersistSchedules()
		{
			IEnumerable<PersistConflict> conflicts;
			var dic = MockRepository.GenerateMock<IScheduleDictionary>();
			target.TryPersist(dic, null, null, null, out conflicts);

			scheduleDictionaryPersister.AssertWasCalled(x => x.Persist(dic));
		}

		[Test]
		public void ShouldReturnOkIfNoConflicts()
		{
			IEnumerable<PersistConflict> conflicts;
			var dic = MockRepository.GenerateMock<IScheduleDictionary>();
			scheduleDictionaryPersister.Expect(x => x.Persist(dic)).Return(new List<PersistConflict>());
			var res = target.TryPersist(dic, null, null, null, out conflicts);

			res.Should().Be.True();
			conflicts.Should().Be.Empty();
		}

		[Test]
		public void ShouldReturnConflictsIfConflicts()
		{
			IEnumerable<PersistConflict> returningConflicts;
			var dic = MockRepository.GenerateMock<IScheduleDictionary>();
			var conflicts = new[]
				{
					new PersistConflict(new DifferenceCollectionItem<IPersistableScheduleData>(), MockRepository.GenerateMock<IPersonAssignment>()),
					new PersistConflict(new DifferenceCollectionItem<IPersistableScheduleData>(), MockRepository.GenerateMock<IPersonAssignment>()),
					new PersistConflict(new DifferenceCollectionItem<IPersistableScheduleData>(), MockRepository.GenerateMock<IPersonAssignment>())
				};
			scheduleDictionaryPersister.Expect(x => x.Persist(dic)).Return(conflicts);
			var res = target.TryPersist(dic, null, null, null, out returningConflicts);

			res.Should().Be.False();
			conflicts.Should().Have.SameValuesAs(conflicts);
		}
	}
}