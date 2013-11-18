using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.Persisters.Account;
using Teleopti.Ccc.Infrastructure.Persisters.Requests;
using Teleopti.Ccc.Infrastructure.Persisters.Schedules;
using Teleopti.Ccc.Infrastructure.Persisters.WriteProtection;
using Teleopti.Ccc.WinCode.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Scheduler
{
	[TestFixture]
	public class SchedulingScreenPersisterTest
	{
		//just mock tests to see that integration tested services are all called.

		private ISchedulingScreenPersister target;
		private IScheduleDictionaryPersister scheduleDictionaryPersister;
		private IPersonAccountPersister personAccountPersister;
		private IRequestPersister requestPersister;
		private IWriteProtectionPersister writeProtectionPersister;

		[SetUp]
		public void Setup()
		{
			personAccountPersister = MockRepository.GenerateMock<IPersonAccountPersister>();
			scheduleDictionaryPersister = MockRepository.GenerateMock<IScheduleDictionaryPersister>();
			requestPersister = MockRepository.GenerateMock<IRequestPersister>();
			writeProtectionPersister = MockRepository.GenerateMock<IWriteProtectionPersister>();
			target = new SchedulingScreenPersister(scheduleDictionaryPersister, personAccountPersister, requestPersister, writeProtectionPersister);
		}

		[Test]
		public void ShouldPersistAccounts()
		{
			IEnumerable<PersistConflict> foo;
			var accounts = new List<IPersonAbsenceAccount>();
			target.TryPersist(null, accounts, null, null, out foo);

			personAccountPersister.AssertWasCalled(x => x.Persist(accounts));
		}

		[Test]
		public void ShouldPersistRequests()
		{
			IEnumerable<PersistConflict> foo;
			var requests = new List<IPersonRequest>();
			target.TryPersist(null, null, requests, null, out foo);

			requestPersister.AssertWasCalled(x => x.Persist(requests));
		}

		[Test]
		public void ShouldPersistWriteProtections()
		{
			IEnumerable<PersistConflict> foo;
			var writeProtections = new List<IPersonWriteProtectionInfo>();
			target.TryPersist(null, null, null, writeProtections, out foo);

			writeProtectionPersister.AssertWasCalled(x => x.Persist(writeProtections));
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
					new PersistConflict(new DifferenceCollectionItem<INonversionedPersistableScheduleData>(), MockRepository.GenerateMock<IPersonAssignment>()),
					new PersistConflict(new DifferenceCollectionItem<INonversionedPersistableScheduleData>(), MockRepository.GenerateMock<IPersonAssignment>()),
					new PersistConflict(new DifferenceCollectionItem<INonversionedPersistableScheduleData>(), MockRepository.GenerateMock<IPersonAssignment>())
				};
			scheduleDictionaryPersister.Expect(x => x.Persist(dic)).Return(conflicts);
			var res = target.TryPersist(dic, null, null, null, out returningConflicts);

			res.Should().Be.False();
			conflicts.Should().Have.SameValuesAs(conflicts);
		}
	}
}