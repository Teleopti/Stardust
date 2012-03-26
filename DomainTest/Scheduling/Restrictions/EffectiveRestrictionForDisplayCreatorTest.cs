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
			var effectiveRestrictionOptions = new EffectiveRestrictionOptions(true);

			var scheduleDay = MockRepository.GenerateMock<IScheduleDay>();
			var restrictionCollection = new[] {preference};
			scheduleDay.Stub(x => x.RestrictionCollection()).Return(restrictionCollection);

			var target = new EffectiveRestrictionForDisplayCreator();
			var result = target.GetEffectiveRestrictionForDisplay(scheduleDay, effectiveRestrictionOptions);

			result.ShiftCategory.Should().Be.EqualTo(preference.ShiftCategory);
		}
	}
}
