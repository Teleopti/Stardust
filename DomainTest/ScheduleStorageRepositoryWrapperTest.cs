using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;


namespace Teleopti.Ccc.DomainTest
{
	[DomainTest]
	public class ScheduleStorageRepositoryWrapperTest
	{
		public IScheduleStorageRepositoryWrapper Target;
		public FakePersonAssignmentRepository PersonAssignmentRepository;
		public FakePersonAbsenceRepository PersonAbsenceRepository;
		public FakePreferenceDayRepository PreferenceDayRepository;
		public FakeNoteRepository NoteRepository;
		public FakeStudentAvailabilityDayRepository StudentAvailabilityDayRepository;
		public FakePublicNoteRepository PublicNoteRepository;
		public FakeAgentDayScheduleTagRepository AgentDayScheduleTagRepository;
		public FakeOvertimeAvailabilityRepository OvertimeAvailabilityRepository;

		[Test]
		public void ShouldBeAbleToHandlePersonAssignmentViaRepository()
		{
			var item = new PersonAssignment(new Person(), new Scenario("-"), new DateOnly()).WithId();
			var repository = PersonAssignmentRepository;

			Target.Add(item);
			repository.LoadAll().Count().Should().Be.EqualTo(1);
			var result = Target.LoadScheduleDataAggregate(item.GetType(), item.Id.GetValueOrDefault());
			Assert.AreSame(item, result);
			var result2 = Target.Get(item.GetType(), item.Id.GetValueOrDefault());
			Assert.AreSame(item, result2);
			Target.Remove(item);
			repository.LoadAll().Count().Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldBeAbleToHandlePersonAbsenceViaRepository()
		{
			var item = new PersonAbsence(new Person(), new Scenario("-"), new AbsenceLayer(new Absence(), new DateTimePeriod())).WithId();
			var repository = PersonAbsenceRepository;

			Target.Add(item);
			repository.LoadAll().Count().Should().Be.EqualTo(1);
			var result = Target.LoadScheduleDataAggregate(item.GetType(), item.Id.GetValueOrDefault());
			Assert.AreSame(item, result);
			var result2 = Target.Get(item.GetType(), item.Id.GetValueOrDefault());
			Assert.AreSame(item, result2);
			Target.Remove(item);
			repository.LoadAll().Count().Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldBeAbleToHandlePreferenceDayViaRepository()
		{
			var item = new PreferenceDay(new Person(), new DateOnly(), new PreferenceRestriction()).WithId();
			var repository = PreferenceDayRepository;

			Target.Add(item);
			repository.LoadAll().Count().Should().Be.EqualTo(1);
			var result = Target.LoadScheduleDataAggregate(item.GetType(), item.Id.GetValueOrDefault());
			Assert.AreSame(item, result);
			var result2 = Target.Get(item.GetType(), item.Id.GetValueOrDefault());
			Assert.AreSame(item, result2);
			Target.Remove(item);
			repository.LoadAll().Count().Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldBeAbleToHandleNoteViaRepository()
		{
			var item = new Note(new Person(), new DateOnly(), new Scenario("-"), "asd").WithId();
			var repository = NoteRepository;

			Target.Add(item);
			repository.LoadAll().Count().Should().Be.EqualTo(1);
			var result = Target.LoadScheduleDataAggregate(item.GetType(), item.Id.GetValueOrDefault());
			Assert.AreSame(item, result);
			var result2 = Target.Get(item.GetType(), item.Id.GetValueOrDefault());
			Assert.AreSame(item, result2);
			Target.Remove(item);
			repository.LoadAll().Count().Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldBeAbleToHandleStudentAvailabilityDayViaRepository()
		{
			var item = new StudentAvailabilityDay(new Person(), new DateOnly(), new List<IStudentAvailabilityRestriction>()).WithId();
			var repository = StudentAvailabilityDayRepository;

			Target.Add(item);
			repository.LoadAll().Count().Should().Be.EqualTo(1);
			var result = Target.LoadScheduleDataAggregate(item.GetType(), item.Id.GetValueOrDefault());
			Assert.AreSame(item, result);
			var result2 = Target.Get(item.GetType(), item.Id.GetValueOrDefault());
			Assert.AreSame(item, result2);
			Target.Remove(item);
			repository.LoadAll().Count().Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldBeAbleToHandlePublicNoteViaRepository()
		{
			var item = new PublicNote(new Person(), new DateOnly(), new Scenario("-"), "asd").WithId();
			
			Target.Add(item);
			PublicNoteRepository.LoadAll().Count().Should().Be.EqualTo(1);
			var result = Target.LoadScheduleDataAggregate(item.GetType(), item.Id.GetValueOrDefault());
			Assert.AreSame(item, result);
			var result2 = Target.Get(item.GetType(), item.Id.GetValueOrDefault());
			Assert.AreSame(item, result2);
			Target.Remove(item);
			PublicNoteRepository.LoadAll().Count().Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldBeAbleToHandleAgentDayScheduleTagViaRepository()
		{
			var item = new AgentDayScheduleTag(new Person(), new DateOnly(), new Scenario("-"), new ScheduleTag()).WithId();
			var repository = AgentDayScheduleTagRepository;

			Target.Add(item);
			repository.LoadAll().Count().Should().Be.EqualTo(1);
			var result = Target.LoadScheduleDataAggregate(item.GetType(), item.Id.GetValueOrDefault());
			Assert.AreSame(item, result);
			var result2 = Target.Get(item.GetType(), item.Id.GetValueOrDefault());
			Assert.AreSame(item, result2);
			Target.Remove(item);
			repository.LoadAll().Count().Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldBeAbleToHandleOvertimeAvailabilityViaRepository()
		{
			var item = new OvertimeAvailability(new Person(), new DateOnly(), new TimeSpan(), new TimeSpan()).WithId();
			var repository = OvertimeAvailabilityRepository;

			Target.Add(item);
			repository.LoadAll().Count().Should().Be.EqualTo(1);
			var result = Target.LoadScheduleDataAggregate(item.GetType(), item.Id.GetValueOrDefault());
			Assert.AreSame(item, result);
			var result2 = Target.Get(item.GetType(), item.Id.GetValueOrDefault());
			Assert.AreSame(item, result2);
			Target.Remove(item);
			repository.LoadAll().Count().Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldIgnoreNullValues()
		{
			Assert.DoesNotThrow(() => Target.Add(null));
			Assert.DoesNotThrow(() => Target.Remove(null));
		}
	}
}