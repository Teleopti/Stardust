using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock
{
	[TestFixture]
	public class WorkShiftFromEditableShiftTest
	{
		private IWorkShiftFromEditableShift _target;
		private TimeZoneInfo _timeZoneInfo;

		[SetUp]
		public void Setup()
		{
			_target = new WorkShiftFromEditableShift();
			_timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
		}

		[Test]
		public void ShouldConvertProperly()
		{
			var expectedPeriod = new DateTimePeriod(new DateTime(1800, 1, 1, 9, 0, 0, DateTimeKind.Utc), new DateTime(1800, 1, 2, 3, 0, 0, DateTimeKind.Utc));

			var equator = new ScheduleDayEquator(new EditableShiftMapper());
			var shiftToConvert = EditableShiftFactory.CreateEditorShiftWithThreeActivityLayers(); 
			var workShift = _target.Convert(shiftToConvert, new DateOnly(2007, 1, 1), _timeZoneInfo);
			Assert.AreEqual(expectedPeriod, workShift.Projection.Period());

			workShift = _target.Convert(shiftToConvert, new DateOnly(2007, 1, 2), _timeZoneInfo);
			Assert.AreEqual(expectedPeriod, workShift.Projection.Period());

			workShift = _target.Convert(shiftToConvert, new DateOnly(2006, 12, 31), _timeZoneInfo);
			Assert.AreEqual(expectedPeriod, workShift.Projection.Period());

			var convertedBack = workShift.ToEditorShift(new DateOnlyAsDateTimePeriod(new DateOnly(2007, 1, 1), _timeZoneInfo),_timeZoneInfo);
			Assert.AreSame(shiftToConvert.ShiftCategory, convertedBack.ShiftCategory);
			Assert.IsTrue(equator.MainShiftEquals(shiftToConvert, convertedBack));
		}
	}
}