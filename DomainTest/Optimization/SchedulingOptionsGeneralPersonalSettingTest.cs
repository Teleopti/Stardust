using System;
using System.Collections.Generic;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;

namespace Teleopti.Ccc.DomainTest.Optimization
{
    [TestFixture]
    public class SchedulingOptionsGeneralPersonalSettingTest
    {
	    [Test]
	    public void ShouldMap()
	    {
		    var scheduleTag = new ScheduleTag();
		    scheduleTag.SetId(Guid.NewGuid());
		    var schedulingOptions = createSchedulingOptions(scheduleTag);

		    var target = new SchedulingOptionsGeneralPersonalSetting();

		    var scheduleTags = new List<IScheduleTag> {scheduleTag};
		    target.MapFrom(schedulingOptions);

		    var targetOptions = new SchedulingOptions();
		    target.MapTo(targetOptions, scheduleTags);

		    targetOptions.TagToUseOnScheduling.Should().Be.SameInstanceAs(scheduleTag);
			targetOptions.UseRotations.Should().Be.True();
			targetOptions.RotationDaysOnly.Should().Be.True();
			targetOptions.UseAvailability.Should().Be.True();
			targetOptions.AvailabilityDaysOnly.Should().Be.True();
			targetOptions.UseStudentAvailability.Should().Be.True();
			targetOptions.UsePreferences.Should().Be.True();
			targetOptions.PreferencesDaysOnly.Should().Be.True();
			targetOptions.UsePreferencesMustHaveOnly.Should().Be.True();
			targetOptions.UseShiftCategoryLimitations.Should().Be.True();
			targetOptions.ShowTroubleshot.Should().Be.True();
	    }

	    private static SchedulingOptions createSchedulingOptions(ScheduleTag scheduleTag)
	    {
		    return new SchedulingOptions
		    {
			    TagToUseOnScheduling = scheduleTag,
			    UseRotations = true,
			    RotationDaysOnly = true,
			    UseAvailability = true,
			    AvailabilityDaysOnly = true,
			    UseStudentAvailability = true,
			    UsePreferences = true,
			    PreferencesDaysOnly = true,
			    UsePreferencesMustHaveOnly = true,
			    UseShiftCategoryLimitations = true,
			    ShowTroubleshot = true
		    };
	    }

	    [Test]
        public void ShouldSetTagToNullScheduleInstanceWhenNoTag()
        {
			var scheduleTag = new ScheduleTag();
			scheduleTag.SetId(Guid.NewGuid());
		    var schedulingOptions = createSchedulingOptions(scheduleTag);

			var target = new SchedulingOptionsGeneralPersonalSetting();

			var scheduleTags = new List<IScheduleTag>();
			target.MapFrom(schedulingOptions);

			var targetOptions = new SchedulingOptions();
			target.MapTo(targetOptions, scheduleTags);

		    targetOptions.TagToUseOnScheduling.Should().Be.EqualTo(NullScheduleTag.Instance);
        }
    }
}
