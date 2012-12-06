using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.WorkShiftCalculation;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.WorkShiftCalculation
{
	[TestFixture]
	public class WorkShiftValueCalculatorTest
	{
		private MockRepository _mocks;
		private IWorkShiftPeriodValueCalculator _workShiftPeriodValueCalculator;
		private IWorkShiftLengthValueCalculator _workShiftLengthValueCalculator;
		private IWorkShiftValueCalculator _target;
		private DateTimePeriod _period1;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_workShiftPeriodValueCalculator = _mocks.StrictMock<IWorkShiftPeriodValueCalculator>();
			_workShiftLengthValueCalculator = _mocks.StrictMock<IWorkShiftLengthValueCalculator>();
			_target = new WorkShiftValueCalculator(_workShiftPeriodValueCalculator, _workShiftLengthValueCalculator);
		}

		[Test]
		public void ShouldContinueIfLayerNotCorrectActivity()
		{
			DateTime start = new DateTime(2009,02,02,8,0,0,DateTimeKind.Utc);
            DateTime end = new DateTime(2009, 02, 02, 8, 15, 0, DateTimeKind.Utc);

            _period1 = new DateTimePeriod(start,end);
			IActivity activity = new Activity("bo");
			IMainShift mainShift = MainShiftFactory.CreateMainShift(activity, _period1, new ShiftCategory("cat"));
			IVisualLayerCollection visualLayerCollection = mainShift.ProjectionService().CreateProjection();

			double? result =_target.CalculateShiftValue(visualLayerCollection, new Activity("phone"),
			                            new Dictionary<DateTime, ISkillIntervalData>(), WorkShiftLengthHintOption.AverageWorkTime,
			                            false, false, 0.5, 1);
			Assert.IsNull(result);
		}

		[Test]
		public void ShouldHandleLayerStartingInsideInterval()
		{
			DateTime start = new DateTime(2009, 02, 02, 8, 0, 0, DateTimeKind.Utc);
			DateTime end = new DateTime(2009, 02, 02, 8, 30, 0, DateTimeKind.Utc);

			_period1 = new DateTimePeriod(start, end);
			DateTimePeriod period2 =new DateTimePeriod(start.AddMinutes(5), end);
			IActivity activity = new Activity("bo");
			IMainShift mainShift = MainShiftFactory.CreateMainShift(activity, period2, new ShiftCategory("cat"));
			IVisualLayerCollection visualLayerCollection = mainShift.ProjectionService().CreateProjection();

			ISkillIntervalData data = new SkillIntervalData(_period1, 5, -5, 0, null, null);
			Dictionary<DateTime, ISkillIntervalData> dic = new Dictionary<DateTime, ISkillIntervalData>();
			dic.Add(start, data);

			double? result = _target.CalculateShiftValue(visualLayerCollection, activity,
										dic, WorkShiftLengthHintOption.AverageWorkTime,
										false, false, 0.5, 1);
			Assert.IsNull(result);
		}
	}
}