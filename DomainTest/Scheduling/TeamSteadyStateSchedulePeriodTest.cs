using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling
{
	[TestFixture]
	public class TeamSteadyStateSchedulePeriodTest
	{
		private TeamSteadyStateSchedulePeriod _target;
		private MockRepository _mocks;
		private IVirtualSchedulePeriod _schedulePeriodTarget;
		private IVirtualSchedulePeriod _schedulePeriod;
		private ISchedulePeriodTargetTimeCalculator _targetTimeCalculator;
		private IScheduleMatrixPro _scheduleMatrixTarget;
		private IScheduleMatrixPro _scheduleMatrix;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_schedulePeriodTarget = _mocks.StrictMock<IVirtualSchedulePeriod>();
			_schedulePeriod = _mocks.StrictMock<IVirtualSchedulePeriod>();
			_targetTimeCalculator = _mocks.StrictMock<ISchedulePeriodTargetTimeCalculator>();
			_scheduleMatrixTarget = _mocks.StrictMock<IScheduleMatrixPro>();
			_scheduleMatrix = _mocks.StrictMock<IScheduleMatrixPro>();
			_target = new TeamSteadyStateSchedulePeriod(_schedulePeriodTarget, _targetTimeCalculator, _scheduleMatrixTarget);
			
		}

		[Test]
		public void ShouldNotEqualOnDifferentPeriod()
		{
			using(_mocks.Record())
			{
				Expect.Call(_schedulePeriodTarget.DateOnlyPeriod).Return(new DateOnlyPeriod(new DateOnly(2012, 2, 2), new DateOnly(2012, 2, 10)));
				Expect.Call(_schedulePeriod.DateOnlyPeriod).Return(new DateOnlyPeriod(new DateOnly(2012, 5, 5), new DateOnly(2012, 7, 10)));	
			}
			
			using(_mocks.Playback())
			{
				Assert.IsFalse(_target.SchedulePeriodEquals(_schedulePeriod, _scheduleMatrix));
			}
		}

		[Test]
		public void ShouldNotEqualOnDifferentType()
		{
			using(_mocks.Record())
			{
				Expect.Call(_schedulePeriodTarget.DateOnlyPeriod).Return(new DateOnlyPeriod(new DateOnly(2012, 2, 2), new DateOnly(2012, 2, 10)));
				Expect.Call(_schedulePeriod.DateOnlyPeriod).Return(new DateOnlyPeriod(new DateOnly(2012, 2, 2), new DateOnly(2012, 2, 10)));
				Expect.Call(_schedulePeriodTarget.PeriodType).Return(SchedulePeriodType.Day);
				Expect.Call(_schedulePeriod.PeriodType).Return(SchedulePeriodType.Month);
			}

			using(_mocks.Playback())
			{
				Assert.IsFalse(_target.SchedulePeriodEquals(_schedulePeriod, _scheduleMatrix));	
			}	
		}

		[Test]
		public void ShouldNotEqualOnDifferentDaysOff()
		{
			using (_mocks.Record())
			{
				Expect.Call(_schedulePeriodTarget.DateOnlyPeriod).Return(new DateOnlyPeriod(new DateOnly(2012, 2, 2), new DateOnly(2012, 2, 10)));
				Expect.Call(_schedulePeriod.DateOnlyPeriod).Return(new DateOnlyPeriod(new DateOnly(2012, 2, 2), new DateOnly(2012, 2, 10)));
				Expect.Call(_schedulePeriodTarget.PeriodType).Return(SchedulePeriodType.Day);
				Expect.Call(_schedulePeriod.PeriodType).Return(SchedulePeriodType.Day);
				Expect.Call(_schedulePeriodTarget.DaysOff()).Return(8);
				Expect.Call(_schedulePeriod.DaysOff()).Return(2);
			}

			using (_mocks.Playback())
			{
				Assert.IsFalse(_target.SchedulePeriodEquals(_schedulePeriod, _scheduleMatrix));
			}		
		}

		[Test]
		public void ShouldNotEqualOnDifferentPeriodTime()
		{
			using (_mocks.Record())
			{
				Expect.Call(_schedulePeriodTarget.DateOnlyPeriod).Return(new DateOnlyPeriod(new DateOnly(2012, 2, 2), new DateOnly(2012, 2, 10)));
				Expect.Call(_schedulePeriod.DateOnlyPeriod).Return(new DateOnlyPeriod(new DateOnly(2012, 2, 2), new DateOnly(2012, 2, 10)));
				Expect.Call(_schedulePeriodTarget.PeriodType).Return(SchedulePeriodType.Day);
				Expect.Call(_schedulePeriod.PeriodType).Return(SchedulePeriodType.Day);
				Expect.Call(_schedulePeriodTarget.DaysOff()).Return(8);
				Expect.Call(_schedulePeriod.DaysOff()).Return(8);
				Expect.Call(_schedulePeriodTarget.PeriodTarget()).Return(TimeSpan.FromHours(160));
				Expect.Call(_schedulePeriod.PeriodTarget()).Return(TimeSpan.FromHours(140));
			}

			using (_mocks.Playback())
			{
				Assert.IsFalse(_target.SchedulePeriodEquals(_schedulePeriod, _scheduleMatrix));
			}		
		}

		[Test]
		public void ShouldNotEqualOnDifferentHoursPerDay()
		{
			using (_mocks.Record())
			{
				Expect.Call(_schedulePeriodTarget.DateOnlyPeriod).Return(new DateOnlyPeriod(new DateOnly(2012, 2, 2), new DateOnly(2012, 2, 10)));
				Expect.Call(_schedulePeriod.DateOnlyPeriod).Return(new DateOnlyPeriod(new DateOnly(2012, 2, 2), new DateOnly(2012, 2, 10)));
				Expect.Call(_schedulePeriodTarget.PeriodType).Return(SchedulePeriodType.Day);
				Expect.Call(_schedulePeriod.PeriodType).Return(SchedulePeriodType.Day);
				Expect.Call(_schedulePeriodTarget.DaysOff()).Return(8);
				Expect.Call(_schedulePeriod.DaysOff()).Return(8);
				Expect.Call(_schedulePeriodTarget.PeriodTarget()).Return(TimeSpan.FromHours(160));
				Expect.Call(_schedulePeriod.PeriodTarget()).Return(TimeSpan.FromHours(160));
				Expect.Call(_schedulePeriodTarget.AverageWorkTimePerDay).Return(TimeSpan.FromHours(8));
				Expect.Call(_schedulePeriod.AverageWorkTimePerDay).Return(TimeSpan.FromHours(6));
			}

			using (_mocks.Playback())
			{
				Assert.IsFalse(_target.SchedulePeriodEquals(_schedulePeriod, _scheduleMatrix));
			}			
		}

		[Test]
		public void ShouldNotEqualOnDifferentMinMaxWorkTime()
		{
			using (_mocks.Record())
			{
				Expect.Call(_schedulePeriodTarget.DateOnlyPeriod).Return(new DateOnlyPeriod(new DateOnly(2012, 2, 2), new DateOnly(2012, 2, 10)));
				Expect.Call(_schedulePeriod.DateOnlyPeriod).Return(new DateOnlyPeriod(new DateOnly(2012, 2, 2), new DateOnly(2012, 2, 10)));
				Expect.Call(_schedulePeriodTarget.PeriodType).Return(SchedulePeriodType.Day);
				Expect.Call(_schedulePeriod.PeriodType).Return(SchedulePeriodType.Day);
				Expect.Call(_schedulePeriodTarget.DaysOff()).Return(8);
				Expect.Call(_schedulePeriod.DaysOff()).Return(8);
				Expect.Call(_schedulePeriodTarget.PeriodTarget()).Return(TimeSpan.FromHours(160));
				Expect.Call(_schedulePeriod.PeriodTarget()).Return(TimeSpan.FromHours(160));
				Expect.Call(_schedulePeriodTarget.AverageWorkTimePerDay).Return(TimeSpan.FromHours(8));
				Expect.Call(_schedulePeriod.AverageWorkTimePerDay).Return(TimeSpan.FromHours(8));
				Expect.Call(_targetTimeCalculator.TargetWithTolerance(_scheduleMatrixTarget)).Return(new TimePeriod(TimeSpan.FromHours(140), TimeSpan.FromHours(170)));
				Expect.Call(_targetTimeCalculator.TargetWithTolerance(_scheduleMatrix)).Return(new TimePeriod(TimeSpan.FromHours(160), TimeSpan.FromHours(160)));
			}

			using (_mocks.Playback())
			{
				Assert.IsFalse(_target.SchedulePeriodEquals(_schedulePeriod, _scheduleMatrix));
			}	
		}

		[Test]
		public void ShouldEqualOnSameTargetValues()
		{
			using (_mocks.Record())
			{
				Expect.Call(_schedulePeriodTarget.DateOnlyPeriod).Return(new DateOnlyPeriod(new DateOnly(2012, 2, 2), new DateOnly(2012, 2, 10)));
				Expect.Call(_schedulePeriod.DateOnlyPeriod).Return(new DateOnlyPeriod(new DateOnly(2012, 2, 2), new DateOnly(2012, 2, 10)));
				Expect.Call(_schedulePeriodTarget.PeriodType).Return(SchedulePeriodType.Day);
				Expect.Call(_schedulePeriod.PeriodType).Return(SchedulePeriodType.Day);
				Expect.Call(_schedulePeriodTarget.DaysOff()).Return(8);
				Expect.Call(_schedulePeriod.DaysOff()).Return(8);
				Expect.Call(_schedulePeriodTarget.PeriodTarget()).Return(TimeSpan.FromHours(160));
				Expect.Call(_schedulePeriod.PeriodTarget()).Return(TimeSpan.FromHours(160));
				Expect.Call(_schedulePeriodTarget.AverageWorkTimePerDay).Return(TimeSpan.FromHours(8));
				Expect.Call(_schedulePeriod.AverageWorkTimePerDay).Return(TimeSpan.FromHours(8));
				Expect.Call(_targetTimeCalculator.TargetWithTolerance(_scheduleMatrixTarget)).Return(new TimePeriod(TimeSpan.FromHours(140), TimeSpan.FromHours(170)));
				Expect.Call(_targetTimeCalculator.TargetWithTolerance(_scheduleMatrix)).Return(new TimePeriod(TimeSpan.FromHours(140), TimeSpan.FromHours(170)));
			}

			using (_mocks.Playback())
			{
				Assert.IsTrue(_target.SchedulePeriodEquals(_schedulePeriod, _scheduleMatrix));
			}	
		}

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void ShouldThrowExceptionOnNullSchedulePeriod()
		{
			_target.SchedulePeriodEquals(null, _scheduleMatrix);
		}

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void ShouldThrowExceptionOnNullVirtualPeriod()
		{
			_target.SchedulePeriodEquals(_schedulePeriod, null);
		}
	}
}
