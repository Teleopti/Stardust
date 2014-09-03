using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;


namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock
{
	[TestFixture]
	public class WorkShiftFromEditableShiftTest
	{
		private IWorkShiftFromEditableShift _target;

		[SetUp]
		public void Setup()
		{
			_target = new WorkShiftFromEditableShift();
		}

		[Test]
		public void ShouldConvertProperly()
		{
			var equator = new ScheduleDayEquator(new EditableShiftMapper());
			var shiftToConvert = EditableShiftFactory.CreateEditorShiftWithThreeActivityLayers(); //will create nightshift starting 2007, 1, 1
			var workShift = _target.Convert(shiftToConvert, new DateOnly(2007, 1, 1));
			var convertedBack = workShift.ToEditorShift(new DateOnly(2007, 1, 1).Date, TimeZoneInfo.Utc);
			Assert.AreSame(shiftToConvert.ShiftCategory, convertedBack.ShiftCategory);
			Assert.IsTrue(equator.MainShiftEquals(shiftToConvert, convertedBack));
		}

	}
}