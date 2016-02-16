using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Scheduling.Meetings;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.Meetings
{
	[TestFixture]
	public class MeetingChangeTrackerTest
	{
		private IChangeTracker<IMeeting> target;
		private IPerson organizer;
		private IActivity activity;
		private IScenario scenario;
		private IPerson participant;

		[SetUp]
		public void Setup()
		{
			target = new MeetingChangeTracker();
			organizer = PersonFactory.CreatePerson();
			activity = ActivityFactory.CreateActivity("Training");
			scenario = ScenarioFactory.CreateScenarioAggregate();
			participant = PersonFactory.CreatePerson();
		}

		[Test]
		public void ShouldReturnNewParticipants()
		{
			var meeting = new Meeting(organizer, new List<IMeetingPerson>(), "test", "test", "test", activity, scenario);
			meeting.SetId(Guid.NewGuid());
			target.ResetSnapshot();
			target.TakeSnapshot(meeting);
			meeting.AddMeetingPerson(new MeetingPerson(participant,false));

			var result = target.CustomChanges(meeting, DomainUpdateType.Update).Single();
			var detail = result.Root as ICustomChangedEntity;
			detail.Id.Value.Should().Be.EqualTo(meeting.Id.Value);
			detail.MainRoot.Should().Be.EqualTo(participant);
			detail.Period.Should().Be.EqualTo(meeting.MeetingPeriod(meeting.StartDate));
			result.Status.Should().Be.EqualTo(DomainUpdateType.Insert);
		}

		[Test]
		public void ShouldReturnRemovedParticipants()
		{
			var meeting = new Meeting(organizer, new List<IMeetingPerson>(), "test", "test", "test", activity, scenario);
			meeting.SetId(Guid.NewGuid());
			meeting.AddMeetingPerson(new MeetingPerson(participant, false));
			target.ResetSnapshot();
			target.TakeSnapshot(meeting);
			meeting.RemovePerson(participant);
			
			var result = target.CustomChanges(meeting, DomainUpdateType.Update).Single();
			var detail = result.Root as ICustomChangedEntity;
			detail.Id.Value.Should().Be.EqualTo(meeting.Id.Value);
			detail.MainRoot.Should().Be.EqualTo(participant);
			detail.Period.Should().Be.EqualTo(meeting.MeetingPeriod(meeting.StartDate));
			result.Status.Should().Be.EqualTo(DomainUpdateType.Update);
		}

		[Test]
		public void ShouldReturnCurrentParticipantsGivenSubjectWasChanged()
		{
			var meeting = new Meeting(organizer, new List<IMeetingPerson>(), "test", "test", "test", activity, scenario);
			meeting.SetId(Guid.NewGuid());
			meeting.AddMeetingPerson(new MeetingPerson(participant, false));
			target.ResetSnapshot();
			target.TakeSnapshot(meeting);

			meeting.Subject = "test2";

			var result = target.CustomChanges(meeting, DomainUpdateType.Update).Single();
			var detail = result.Root as ICustomChangedEntity;
			detail.Id.Value.Should().Be.EqualTo(meeting.Id.Value);
			detail.MainRoot.Should().Be.EqualTo(participant);
			detail.Period.Should().Be.EqualTo(meeting.MeetingPeriod(meeting.StartDate));
			result.Status.Should().Be.EqualTo(DomainUpdateType.Update);
		}

		[Test]
		public void ShouldReturnAllParticipantsGivenMeetingWasDeleted()
		{
			var meeting = new Meeting(organizer, new List<IMeetingPerson>(), "test", "test", "test", activity, scenario);
			meeting.SetId(Guid.NewGuid());
			meeting.AddMeetingPerson(new MeetingPerson(participant, false));
			meeting.AddMeetingPerson(new MeetingPerson(organizer, false));
			target.ResetSnapshot();
			target.TakeSnapshot(meeting);

			var result = target.CustomChanges(meeting, DomainUpdateType.Delete);
			result.Count().Should().Be.EqualTo(2);
			result.All(r => r.Status == DomainUpdateType.Delete).Should().Be.True();
		}

		[Test]
		public void ShouldNotReturnAnyChangesIfNoPreviousSnapshot()
		{
			var meeting = new Meeting(organizer, new List<IMeetingPerson>(), "test", "test", "test", activity, scenario);
			meeting.SetId(Guid.NewGuid());
			meeting.AddMeetingPerson(new MeetingPerson(participant, false));

			target.CustomChanges(meeting, DomainUpdateType.Insert).Count().Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldReturnAllParticipantsGivenMeetingTimeWasChanged()
		{
			var meeting = new Meeting(organizer, new List<IMeetingPerson>(), "test", "test", "test", activity, scenario);
			meeting.SetId(Guid.NewGuid());
			meeting.AddMeetingPerson(new MeetingPerson(participant, false));
			meeting.AddMeetingPerson(new MeetingPerson(organizer, false));
			target.ResetSnapshot();
			target.TakeSnapshot(meeting);

			meeting.StartDate = meeting.StartDate.AddDays(3);
			meeting.EndDate = meeting.StartDate;

			var result = target.CustomChanges(meeting, DomainUpdateType.Update);
			result.Count().Should().Be.EqualTo(4);
		}

		[Test]
		public void ShouldReturnAllParticipantsGivenMeetingActivityWasChanged()
		{
			var meeting = new Meeting(organizer, new List<IMeetingPerson>(), "test", "test", "test", activity, scenario);
			meeting.SetId(Guid.NewGuid());
			meeting.AddMeetingPerson(new MeetingPerson(participant, false));
			meeting.AddMeetingPerson(new MeetingPerson(organizer, false));
			target.ResetSnapshot();
			target.TakeSnapshot(meeting);

			meeting.Activity = ActivityFactory.CreateActivity("training");

			var result = target.CustomChanges(meeting, DomainUpdateType.Update);
			result.Count().Should().Be.EqualTo(2);
		}
	}
}
