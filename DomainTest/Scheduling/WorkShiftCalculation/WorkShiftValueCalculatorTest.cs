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
		private DateTime _start;
		private DateTime _end;
		private IActivity _phoneActivity;
		private ISkillIntervalData _data;
		private IDictionary<TimeSpan, ISkillIntervalData> _dic;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_workShiftPeriodValueCalculator = _mocks.StrictMock<IWorkShiftPeriodValueCalculator>();
			_workShiftLengthValueCalculator = _mocks.StrictMock<IWorkShiftLengthValueCalculator>();
			_target = new WorkShiftValueCalculator(_workShiftPeriodValueCalculator, _workShiftLengthValueCalculator);
			_start = new DateTime(2009, 02, 02, 8, 0, 0, DateTimeKind.Utc);
			_end = new DateTime(2009, 02, 02, 8, 30, 0, DateTimeKind.Utc);
			_phoneActivity = new Activity("phone");
			_phoneActivity.RequiresSkill = true;
			_period1 = new DateTimePeriod(_start, _end);
			_data = new SkillIntervalData(_period1, 5, -5, 0, null, null);
			_dic = new Dictionary<TimeSpan, ISkillIntervalData>();
			_dic.Add(_start.TimeOfDay, _data);
		}

		[Test]
		public void ShouldHandleClosedDay()
		{
            
			IActivity activity = new Activity("bo");
			IMainShift mainShift = MainShiftFactory.CreateMainShift(activity, _period1, new ShiftCategory("cat"));
			IVisualLayerCollection visualLayerCollection = mainShift.ProjectionService().CreateProjection();

			double? result =_target.CalculateShiftValue(visualLayerCollection, new Activity("phone"),
										new Dictionary<TimeSpan, ISkillIntervalData>(), WorkShiftLengthHintOption.AverageWorkTime,
			                            false, false, 0.5, 1);
			Assert.IsNull(result);
		}

		[Test]
		public void ShouldHandleLayerStartingAndEndingInsideInterval()
		{
			DateTimePeriod period2 =new DateTimePeriod(_start.AddMinutes(5), _end.AddMinutes(-5));
			IMainShift mainShift = MainShiftFactory.CreateMainShift(_phoneActivity, period2, new ShiftCategory("cat"));
			IVisualLayerCollection visualLayerCollection = mainShift.ProjectionService().CreateProjection();

			using (_mocks.Record())
			{
				Expect.Call(_workShiftPeriodValueCalculator.PeriodValue(_data, 20, false, false, 0.5, 1)).Return(50);
				Expect.Call(_workShiftLengthValueCalculator.CalculateShiftValueForPeriod(50, 20,
				                                                                         WorkShiftLengthHintOption.AverageWorkTime))
					.Return(50);
			}

			using (_mocks.Playback())
			{
				double? result = _target.CalculateShiftValue(visualLayerCollection, _phoneActivity,
										_dic, WorkShiftLengthHintOption.AverageWorkTime,
										false, false, 0.5, 1);
				Assert.AreEqual(50, result);
			}
		}

		[Test]
		public void ShouldHandleLayerStartingAndEndingOnExactInterval()
		{
			DateTimePeriod period2 = new DateTimePeriod(_start, _end);
			IMainShift mainShift = MainShiftFactory.CreateMainShift(_phoneActivity, period2, new ShiftCategory("cat"));
			IVisualLayerCollection visualLayerCollection = mainShift.ProjectionService().CreateProjection();

			using (_mocks.Record())
			{
				Expect.Call(_workShiftPeriodValueCalculator.PeriodValue(_data, 30, false, false, 0.5, 1)).Return(50);
				Expect.Call(_workShiftLengthValueCalculator.CalculateShiftValueForPeriod(50, 30,
																						 WorkShiftLengthHintOption.AverageWorkTime))
					.Return(50);
			}

			using (_mocks.Playback())
			{
				double? result = _target.CalculateShiftValue(visualLayerCollection, _phoneActivity,
										_dic, WorkShiftLengthHintOption.AverageWorkTime,
										false, false, 0.5, 1);
				Assert.AreEqual(50, result);
			}
		}

		[Test]
		public void ShouldHandleLayerStartingOutsideOpenHoursAndEndingInsideInterval()
		{
			DateTimePeriod period2 = new DateTimePeriod(_start.AddMinutes(-5), _end);
			IMainShift mainShift = MainShiftFactory.CreateMainShift(_phoneActivity, period2, new ShiftCategory("cat"));
			IVisualLayerCollection visualLayerCollection = mainShift.ProjectionService().CreateProjection();

			using (_mocks.Record())
			{}

			using (_mocks.Playback())
			{
				double? result = _target.CalculateShiftValue(visualLayerCollection, _phoneActivity,
										_dic, WorkShiftLengthHintOption.AverageWorkTime,
										false, false, 0.5, 1);
				Assert.IsNull(result);
			}
		}

		[Test]
		public void ShouldHandleLayerStartingInsideIntervalAndEndingOutsideOpenHours()
		{
			DateTimePeriod period2 = new DateTimePeriod(_start.AddMinutes(5), _end.AddMinutes(5));
			IMainShift mainShift = MainShiftFactory.CreateMainShift(_phoneActivity, period2, new ShiftCategory("cat"));
			IVisualLayerCollection visualLayerCollection = mainShift.ProjectionService().CreateProjection();

			using (_mocks.Record())
			{
				Expect.Call(_workShiftPeriodValueCalculator.PeriodValue(_data, 25, false, false, 0.5, 1)).Return(50);
			}

			using (_mocks.Playback())
			{
				double? result = _target.CalculateShiftValue(visualLayerCollection, _phoneActivity,
										_dic, WorkShiftLengthHintOption.AverageWorkTime,
										false, false, 0.5, 1);
				Assert.IsNull(result);
			}
		}

		[Test]
		public void ShouldHandleTwoShortLayersWithinTheInterval()
		{
			DateTimePeriod period1 = new DateTimePeriod(_start.AddMinutes(1), _end.AddMinutes(-28));
			DateTimePeriod period2 = new DateTimePeriod(_start.AddMinutes(27), _end.AddMinutes(-1));
			IMainShift mainShift = MainShiftFactory.CreateMainShift(_phoneActivity, period1, new ShiftCategory("cat"));
			IMainShiftActivityLayer mainShiftActivityLayer = new MainShiftActivityLayer(_phoneActivity, period2);
			mainShift.LayerCollection.Add(mainShiftActivityLayer);
			IVisualLayerCollection visualLayerCollection = mainShift.ProjectionService().CreateProjection();

			using (_mocks.Record())
			{
				Expect.Call(_workShiftPeriodValueCalculator.PeriodValue(_data, 1, false, false, 0.5, 1)).Return(5);
				Expect.Call(_workShiftPeriodValueCalculator.PeriodValue(_data, 2, false, false, 0.5, 1)).Return(7);
				Expect.Call(_workShiftLengthValueCalculator.CalculateShiftValueForPeriod(12, 3,
																						 WorkShiftLengthHintOption.AverageWorkTime))
					.Return(50);
			}

			using (_mocks.Playback())
			{
				double? result = _target.CalculateShiftValue(visualLayerCollection, _phoneActivity,
										_dic, WorkShiftLengthHintOption.AverageWorkTime,
										false, false, 0.5, 1);
				Assert.AreEqual(50, result);
			}
		}

		[Test]
		public void ShouldHandleLayerStartingInOneIntervalAndEndingInAnother()
		{
			var period1 = new DateTimePeriod(_start.AddMinutes(30), _end.AddMinutes(30));
			ISkillIntervalData data2 = new SkillIntervalData(period1, 5, -5, 0, null, null);
			_dic.Add(period1.StartDateTime.TimeOfDay, data2);

			DateTimePeriod period2 = new DateTimePeriod(_start.AddMinutes(15), _end.AddMinutes(20));
			IMainShift mainShift = MainShiftFactory.CreateMainShift(_phoneActivity, period2, new ShiftCategory("cat"));
			IVisualLayerCollection visualLayerCollection = mainShift.ProjectionService().CreateProjection();

			using (_mocks.Record())
			{
				Expect.Call(_workShiftPeriodValueCalculator.PeriodValue(_data, 15, false, false, 0.5, 1)).Return(50);
				Expect.Call(_workShiftPeriodValueCalculator.PeriodValue(data2, 20, false, false, 0.5, 1)).Return(50);
				Expect.Call(_workShiftLengthValueCalculator.CalculateShiftValueForPeriod(100, 35,
																						 WorkShiftLengthHintOption.AverageWorkTime))
					.Return(100);
			}

			using (_mocks.Playback())
			{
				double? result = _target.CalculateShiftValue(visualLayerCollection, _phoneActivity,
										_dic, WorkShiftLengthHintOption.AverageWorkTime,
										false, false, 0.5, 1);
				Assert.AreEqual(100, result);
			}
		}

		[Test]
		public void ActivityThatNotRequiresSkillCanExistOutsideOpenHours()
		{
			IActivity otherActivity = new Activity("other");
			otherActivity.RequiresSkill = false;
			DateTimePeriod period2 = new DateTimePeriod(_start.AddMinutes(-15), _end.AddMinutes(15));
			IMainShift mainShift = MainShiftFactory.CreateMainShift(otherActivity, period2, new ShiftCategory("cat"));
			DateTimePeriod period1 = new DateTimePeriod(_start.AddMinutes(10), _end.AddMinutes(-10));
			IMainShiftActivityLayer mainShiftActivityLayer = new MainShiftActivityLayer(_phoneActivity, period1);
			mainShift.LayerCollection.Add(mainShiftActivityLayer);
			IVisualLayerCollection visualLayerCollection = mainShift.ProjectionService().CreateProjection();

			using (_mocks.Record())
			{
				Expect.Call(_workShiftPeriodValueCalculator.PeriodValue(_data, 10, false, false, 0.5, 1)).Return(50);
				Expect.Call(_workShiftLengthValueCalculator.CalculateShiftValueForPeriod(50, 10,
																						 WorkShiftLengthHintOption.AverageWorkTime))
					.Return(100);
			}

			using (_mocks.Playback())
			{
				double? result = _target.CalculateShiftValue(visualLayerCollection, _phoneActivity,
										_dic, WorkShiftLengthHintOption.AverageWorkTime,
										false, false, 0.5, 1);
				Assert.AreEqual(100, result);
			}
		}

		[Test]
		public void LayerThatIsNotSkillActivtyShouldCalculateAsZero()
		{
			IActivity otherActivity = new Activity("other");
			otherActivity.RequiresSkill = true;
			DateTimePeriod period2 = new DateTimePeriod(_start.AddMinutes(5), _end.AddMinutes(-5));
			IMainShift mainShift = MainShiftFactory.CreateMainShift(otherActivity, period2, new ShiftCategory("cat"));
			IVisualLayerCollection visualLayerCollection = mainShift.ProjectionService().CreateProjection();

			using (_mocks.Record())
			{
				Expect.Call(_workShiftLengthValueCalculator.CalculateShiftValueForPeriod(0, 0,
																						 WorkShiftLengthHintOption.AverageWorkTime))
					.Return(0);
			}

			using (_mocks.Playback())
			{
				double? result = _target.CalculateShiftValue(visualLayerCollection, _phoneActivity,
										_dic, WorkShiftLengthHintOption.AverageWorkTime,
										false, false, 0.5, 1);
				Assert.AreEqual(0, result);
			}
		}

		[Test]
		public void LayerThatIsNotSkillActivtyShouldCalculateAsZeroEvenIfItIsOutSideOpenHours()
		{
			IActivity otherActivity = new Activity("other");
			otherActivity.RequiresSkill = true;
			DateTimePeriod period2 = new DateTimePeriod(_start.AddMinutes(-5), _end.AddMinutes(-5));
			IMainShift mainShift = MainShiftFactory.CreateMainShift(otherActivity, period2, new ShiftCategory("cat"));
			IVisualLayerCollection visualLayerCollection = mainShift.ProjectionService().CreateProjection();

			using (_mocks.Record())
			{
				Expect.Call(_workShiftLengthValueCalculator.CalculateShiftValueForPeriod(0, 0,
																						 WorkShiftLengthHintOption.AverageWorkTime))
					.Return(0);
			}

			using (_mocks.Playback())
			{
				double? result = _target.CalculateShiftValue(visualLayerCollection, _phoneActivity,
										_dic, WorkShiftLengthHintOption.AverageWorkTime,
										false, false, 0.5, 1);
				Assert.AreEqual(0, result);
			}
		}

		[Test]
		public void ShouldCalculateLayerStartingAfterMidnight()
		{
			_start = new DateTime(2009, 02, 02, 23, 0, 0, DateTimeKind.Utc);
			_end = new DateTime(2009, 02, 03, 0, 0, 0, DateTimeKind.Utc);
			DateTimePeriod period1 = new DateTimePeriod(_start, _end);
			DateTimePeriod period2 = new DateTimePeriod(_start.AddHours(1), _end.AddHours(1));
			IMainShift mainShift = MainShiftFactory.CreateMainShift(_phoneActivity, period1, new ShiftCategory("cat"));
			IMainShiftActivityLayer mainShiftActivityLayer = new MainShiftActivityLayer(_phoneActivity, period2);
			mainShift.LayerCollection.Add(mainShiftActivityLayer);
			IVisualLayerCollection visualLayerCollection = mainShift.ProjectionService().CreateProjection();
			var data1 = new SkillIntervalData(period1, 5, -5, 0, null, null);
			var data2 = new SkillIntervalData(period2, 5, -5, 0, null, null);
			_dic.Clear();
			_dic.Add(new TimeSpan(23, 0 , 0), data1);
			_dic.Add(new TimeSpan(1, 0, 0, 0), data2);

			using (_mocks.Record())
			{
				Expect.Call(_workShiftPeriodValueCalculator.PeriodValue(data1, 60, false, false, 0.5, 1)).Return(5);
				Expect.Call(_workShiftPeriodValueCalculator.PeriodValue(data2, 60, false, false, 0.5, 1)).Return(7);
				Expect.Call(_workShiftLengthValueCalculator.CalculateShiftValueForPeriod(12, 120,
																						 WorkShiftLengthHintOption.AverageWorkTime))
					.Return(50);
			}

			using (_mocks.Playback())
			{
				double? result = _target.CalculateShiftValue(visualLayerCollection, _phoneActivity,
										_dic, WorkShiftLengthHintOption.AverageWorkTime,
										false, false, 0.5, 1);
				Assert.AreEqual(50, result);
			}
		}
	}
}