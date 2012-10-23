using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Meetings;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.TestCommon;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.Restrictions
{
	[TestFixture]
	public class WorkTimeMinMaxRestrictionCreatorTest
	{
		[Test]
		public void ShouldReturnEffectiveRestriction()
		{
			var effectiveRestrictionForDisplayCreator = MockRepository.GenerateMock<IEffectiveRestrictionForDisplayCreator>();
			var scheduleDay = new StubFactory().ScheduleDayStub();
			var effectiveRestriction = MockRepository.GenerateMock<IEffectiveRestriction>();
			var target = new WorkTimeMinMaxRestrictionCreator(effectiveRestrictionForDisplayCreator);

			scheduleDay.Stub(x => x.PersonMeetingCollection()).Return(new ReadOnlyCollection<IPersonMeeting>(new IPersonMeeting[] {}));
			effectiveRestrictionForDisplayCreator.Stub(x => x.MakeEffectiveRestriction(scheduleDay, EffectiveRestrictionOptions.UseAll())).Return(effectiveRestriction);

			var result = target.MakeWorkTimeMinMaxRestriction(scheduleDay, EffectiveRestrictionOptions.UseAll());

			result.Restriction.Should().Be(effectiveRestriction);
		}

		[Test]
		public void ShouldReturnCombinedRestrictionIfDayHasMeetings()
		{
			var effectiveRestrictionForDisplayCreator = MockRepository.GenerateMock<IEffectiveRestrictionForDisplayCreator>();
			var scheduleDay = new StubFactory().ScheduleDayStub();
			var effectiveRestriction = MockRepository.GenerateMock<IEffectiveRestriction>();
			var target = new WorkTimeMinMaxRestrictionCreator(effectiveRestrictionForDisplayCreator);

			scheduleDay.Stub(x => x.PersonMeetingCollection()).Return(new ReadOnlyCollection<IPersonMeeting>(new[] {new PersonMeeting(null, null, new DateTimePeriod())}));
			effectiveRestrictionForDisplayCreator.Stub(x => x.MakeEffectiveRestriction(scheduleDay, EffectiveRestrictionOptions.UseAll())).Return(effectiveRestriction);

			var result = target.MakeWorkTimeMinMaxRestriction(scheduleDay, EffectiveRestrictionOptions.UseAll());

			result.Restriction.Should().Be.OfType<CombinedRestriction>();
			var combined = result.Restriction as CombinedRestriction;
			combined.One.Should().Be(effectiveRestriction);
			combined.Two.Should().Be.OfType<MeetingRestriction>();
		}
	}

	[TestFixture]
	public class EffectiveRestrictionForDisplayCreatorTest
	{
		[Test]
		public void ShouldCreateEffectiveRestrictionBasedOnPreference()
		{
			var preference = new PreferenceRestriction {ShiftCategory = new ShiftCategory("AM")};
			var effectiveRestrictionOptions = new EffectiveRestrictionOptions {UsePreference = true};

			var scheduleDay = MockRepository.GenerateMock<IScheduleDay>();
			var restrictionCollection = new[] {preference};
			scheduleDay.Stub(x => x.RestrictionCollection()).Return(restrictionCollection);

			var target = new EffectiveRestrictionForDisplayCreator(new RestrictionRetrievalOperation(), new RestrictionCombiner(), new MeetingRestrictionCombiner(new RestrictionCombiner()), new PersonalShiftRestrictionCombiner(new RestrictionCombiner()));
			var result = target.MakeEffectiveRestriction(scheduleDay, effectiveRestrictionOptions);

			result.ShiftCategory.Should().Be.EqualTo(preference.ShiftCategory);
			result.IsPreferenceDay.Should().Be.True();
			result.IsRestriction.Should().Be.True();
		}

		[Test]
		public void ShouldCreateEffectiveRestrictionBasedOnAvailability()
		{
			var availability = new AvailabilityRestriction
			                   	{
									StartTimeLimitation = new StartTimeLimitation(new TimeSpan(8, 0, 0), new TimeSpan(17, 0, 0)),
									EndTimeLimitation = new EndTimeLimitation(new TimeSpan(8, 0, 0), new TimeSpan(17, 0, 0)),
			                   		WorkTimeLimitation = new WorkTimeLimitation(new TimeSpan(6,0,0), new TimeSpan(9,0,0))
			                   	};
			var effectiveRestrictionOptions = new EffectiveRestrictionOptions {UseAvailability = true};

			var scheduleDay = MockRepository.GenerateMock<IScheduleDay>();
			var restrictionCollection = new[] { availability };
			scheduleDay.Stub(x => x.RestrictionCollection()).Return(restrictionCollection);

			var target = new EffectiveRestrictionForDisplayCreator(new RestrictionRetrievalOperation(), new RestrictionCombiner(), new MeetingRestrictionCombiner(new RestrictionCombiner()), new PersonalShiftRestrictionCombiner(new RestrictionCombiner()));
			var result = target.MakeEffectiveRestriction(scheduleDay, effectiveRestrictionOptions);

			result.StartTimeLimitation.Should().Be.EqualTo(availability.StartTimeLimitation);
			result.EndTimeLimitation.Should().Be.EqualTo(availability.EndTimeLimitation);
			result.WorkTimeLimitation.Should().Be.EqualTo(availability.WorkTimeLimitation);
			result.IsAvailabilityDay.Should().Be.True();
			result.IsRestriction.Should().Be.True();
		}
	}
}
