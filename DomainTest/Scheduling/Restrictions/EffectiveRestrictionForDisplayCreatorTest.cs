using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
			var effectiveRestrictionOptions = new EffectiveRestrictionOptions(true, false);

			var scheduleDay = MockRepository.GenerateMock<IScheduleDay>();
			var restrictionCollection = new[] {preference};
			scheduleDay.Stub(x => x.RestrictionCollection()).Return(restrictionCollection);

			var target = new EffectiveRestrictionForDisplayCreator();
			var result = target.GetEffectiveRestrictionForDisplay(scheduleDay, effectiveRestrictionOptions);

			result.ShiftCategory.Should().Be.EqualTo(preference.ShiftCategory);
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
			var effectiveRestrictionOptions = new EffectiveRestrictionOptions(false,true);

			var scheduleDay = MockRepository.GenerateMock<IScheduleDay>();
			var restrictionCollection = new[] { availability };
			scheduleDay.Stub(x => x.RestrictionCollection()).Return(restrictionCollection);

			var target = new EffectiveRestrictionForDisplayCreator();
			var result = target.GetEffectiveRestrictionForDisplay(scheduleDay, effectiveRestrictionOptions);

			result.StartTimeLimitation.Should().Be.EqualTo(availability.StartTimeLimitation);
			result.EndTimeLimitation.Should().Be.EqualTo(availability.EndTimeLimitation);
			result.WorkTimeLimitation.Should().Be.EqualTo(availability.WorkTimeLimitation);

			effectiveRestrictionOptions.Equals(null).Should().Be.False();
			effectiveRestrictionOptions.Equals((object)null).Should().Be.False();
			effectiveRestrictionOptions.Equals(effectiveRestrictionOptions).Should().Be.True();
			effectiveRestrictionOptions.Equals((object)effectiveRestrictionOptions).Should().Be.True();
			effectiveRestrictionOptions.Equals(availability).Should().Be.False();
			(true).Should().Be.True();
			(false).Should().Be.False();
			effectiveRestrictionOptions.GetHashCode().Should().Not.Be.EqualTo(null);
		}
	}
}
