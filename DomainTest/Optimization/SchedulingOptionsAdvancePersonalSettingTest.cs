using System;
using System.Collections.Generic;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.DomainTest.Optimization
{
    [TestFixture]
    public class SchedulingOptionsAdvancedPersonalSettingTest
    {
	    [Test]
	    public void ShouldMap()
		{
			var shiftCategory = ShiftCategoryFactory.CreateShiftCategory("Asdf");
			shiftCategory.SetId(Guid.NewGuid());
			var target = new SchedulingOptionsAdvancedPersonalSetting();
		    var options = new SchedulingOptions
		    {
			    ShiftCategory = shiftCategory,
			    UseMinimumStaffing = true,
			    UseMaximumStaffing = true,
			    UseAverageShiftLengths = true,
				RefreshRate = 32,
				ResourceCalculateFrequency = 12
				
		    };

		    target.MapFrom(options);

		    var targetOptions = new SchedulingOptions();
		    target.MapTo(targetOptions, new List<IShiftCategory> {shiftCategory});

		    targetOptions.ShiftCategory.Should().Be.EqualTo(shiftCategory);
		    targetOptions.UseMaximumStaffing.Should().Be.True();
		    targetOptions.UseMinimumStaffing.Should().Be.True();
		    targetOptions.UseAverageShiftLengths.Should().Be.True();
		    targetOptions.RefreshRate.Should().Be.EqualTo(32);
		    targetOptions.ResourceCalculateFrequency.Should().Be.EqualTo(12);
		}
    }
}
