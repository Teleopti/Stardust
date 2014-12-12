using System;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.Restrictions
{
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

			var target = new EffectiveRestrictionForDisplayCreator(new RestrictionRetrievalOperation(),
				new RestrictionCombiner());
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

			var target = new EffectiveRestrictionForDisplayCreator(new RestrictionRetrievalOperation(),
				new RestrictionCombiner());
			var result = target.MakeEffectiveRestriction(scheduleDay, effectiveRestrictionOptions);

			result.StartTimeLimitation.Should().Be.EqualTo(availability.StartTimeLimitation);
			result.EndTimeLimitation.Should().Be.EqualTo(availability.EndTimeLimitation);
			result.WorkTimeLimitation.Should().Be.EqualTo(availability.WorkTimeLimitation);
			result.IsAvailabilityDay.Should().Be.True();
			result.IsRestriction.Should().Be.True();
		}

		[Test]
		public void ShouldCreateEffectiveRestrictionBasedOnStudentAvailability()
		{
			var studentAvailability = new StudentAvailabilityRestriction
			{
				StartTimeLimitation = new StartTimeLimitation(new TimeSpan(8, 0, 0), new TimeSpan(17, 0, 0)),
				EndTimeLimitation = new EndTimeLimitation(new TimeSpan(8, 0, 0), new TimeSpan(17, 0, 0)),
				WorkTimeLimitation = new WorkTimeLimitation(new TimeSpan(6, 0, 0), new TimeSpan(9, 0, 0))
			};
			var effectiveRestrictionOptions = new EffectiveRestrictionOptions { UseStudentAvailability = true };

			var scheduleDay = MockRepository.GenerateMock<IScheduleDay>();
			var restrictionCollection = new[] { studentAvailability };
			scheduleDay.Stub(x => x.RestrictionCollection()).Return(restrictionCollection);

			var target = new EffectiveRestrictionForDisplayCreator(new RestrictionRetrievalOperation(),
				new RestrictionCombiner());
			var result = target.MakeEffectiveRestriction(scheduleDay, effectiveRestrictionOptions);

			result.StartTimeLimitation.Should().Be.EqualTo(studentAvailability.StartTimeLimitation);
			result.EndTimeLimitation.Should().Be.EqualTo(studentAvailability.EndTimeLimitation);
			result.WorkTimeLimitation.Should().Be.EqualTo(studentAvailability.WorkTimeLimitation);
			result.IsAvailabilityDay.Should().Be.True();
			result.IsRestriction.Should().Be.True();
		}
	}
}
