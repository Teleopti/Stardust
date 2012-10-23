using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Scheduling.Meetings;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.TestCommon;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.Restrictions
{
	[TestFixture]
	public class WorkTimeMinMaxRestrictionCreatorTest
	{
		[Test]
		public static void ShouldReturnEffectiveRestriction()
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
		public static void ShouldReturnCombinedRestrictionIfDayHasMeetings()
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
}