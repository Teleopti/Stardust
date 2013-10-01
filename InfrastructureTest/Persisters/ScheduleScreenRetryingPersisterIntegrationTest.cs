using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Persisters;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.Persisters 
{
	[TestFixture]
	[Category("LongRunning")]
	public class ScheduleScreenRetryingPersisterIntegrationTest : ScheduleScreenPersisterIntegrationTest
	{

		protected override IPersistableScheduleData SetupScheduleData()
		{
			return new Note(Person, FirstDayDateOnly, Scenario, "a test note");
		}

		protected override IEnumerable<IAggregateRoot> TestDataToReassociate()
		{
			return new IAggregateRoot[] { };
		}

		private static T UnitOfWorkAction<T>(Func<IUnitOfWork, T> expressionThatRequiresUnitOfWork)
		{
			using (var unitOfWork = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				return expressionThatRequiresUnitOfWork.Invoke(unitOfWork);
			}
		}

		[Test]
		public void ShouldPersistWithPersonAbsenceAccountConflict()
		{
			var expectedPaaMemoryVersion = 1;
			var expectedPaaDbVersion = 1;
			var expectedSchedDataMemoryVersion = 1;
			var expectedSchedDataDbVersionVersion = 1;

			MakeTarget();
			PersonAbsenceAccount.Version.Should().Be.EqualTo(expectedPaaMemoryVersion);
			UnitOfWorkAction(uow => uow.DatabaseVersion(PersonAbsenceAccount)).Should().Be.EqualTo(expectedPaaDbVersion);
			ScheduleData.Version.Should().Be.EqualTo(expectedSchedDataMemoryVersion);
			UnitOfWorkAction(uow => uow.DatabaseVersion(ScheduleData).Should().Be.EqualTo(expectedSchedDataDbVersionVersion));

			//two db changes to paa in db
			ModifyPersonAbsenceAccountAsAnotherUser();
			ModifyPersonAbsenceAccountAsAnotherUser();
			expectedPaaDbVersion += 2;

			PersonAbsenceAccount.Version.Should().Be.EqualTo(expectedPaaMemoryVersion);
			UnitOfWorkAction(uow => uow.DatabaseVersion(PersonAbsenceAccount)).Should().Be.EqualTo(expectedPaaDbVersion);
			ScheduleData.Version.Should().Be.EqualTo(expectedSchedDataMemoryVersion);
			UnitOfWorkAction(uow => uow.DatabaseVersion(ScheduleData).Should().Be.EqualTo(expectedSchedDataDbVersionVersion));

			//one change to sched data and synchronization db (=setting paa mem object in this case)
			var result = TryPersistScheduleScreen();
			expectedSchedDataDbVersionVersion += 1;
			expectedPaaMemoryVersion = expectedPaaDbVersion;

			result.Saved.Should().Be.True();
			PersonAbsenceAccount.Version.Should().Be.EqualTo(expectedPaaMemoryVersion);
			UnitOfWorkAction(uow => uow.DatabaseVersion(PersonAbsenceAccount)).Should().Be.EqualTo(expectedPaaDbVersion);
			UnitOfWorkAction(uow => uow.DatabaseVersion(ScheduleData).Should().Be.EqualTo(expectedSchedDataDbVersionVersion));
		}

		[Test]
		public void ShouldPersistWithDeletedPersonAbsenceAccount()
		{
			MakeTarget();
			// this will cause a optimistic lock + a unitOfWork refresh fail
			DeleteCurrentPersonAbsenceAccountAsAnotherUser();
			var result = TryPersistScheduleScreen();

			result.Saved.Should().Be.True();
		}

		//private void AddPersonAssignmentAndAbsence()
		//{
		//	var period = new DateTimePeriod(FirstDayDateOnly, FirstDayDateOnly.AddDays(1));
		//	var personAssignment = new PersonAssignment(Person, Scenario, FirstDayDateOnly);
		//	personAssignment.AddMainLayer(Activity, period);
		//	var scheduleDay = ScheduleDictionary[Person].ScheduledDay(FirstDayDateOnly);
		//	scheduleDay.Add(personAssignment);
		//	scheduleDay.CreateAndAddActivity(Activity, period, ShiftCategory);
		//	scheduleDay.CreateAndAddAbsence(new AbsenceLayer(Absence, period));
		//	ScheduleDictionary.Modify(ScheduleModifier.Scheduler, scheduleDay, NewBusinessRuleCollection.Minimum(), new ResourceCalculationOnlyScheduleDayChangeCallback(), new ScheduleTagSetter(NullScheduleTag.Instance));
		//}
		
		[Test]
		public void ShouldRevertVersionNumberWhenTransactionFails()
		{
			var entity = PersonAbsenceAccount as IVersioned;
			var previousVersion = entity.Version;

			ScheduleDictionarySaver = Mocks.StrictMock<IScheduleDictionarySaver>();
			MakeTarget();

			Expect.Call(() => ScheduleDictionarySaver.MarkForPersist(null, null, null)).IgnoreArguments().Throw(new OptimisticLockException());

			Mocks.ReplayAll();

			TryPersistScheduleScreen();

			Mocks.Verify(ScheduleDictionarySaver);

			Assert.That(entity.Version, Is.EqualTo(previousVersion));
		}
	}
}