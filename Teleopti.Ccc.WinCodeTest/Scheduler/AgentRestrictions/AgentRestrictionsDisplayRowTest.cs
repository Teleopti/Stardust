using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.WinCode.Scheduling.AgentRestrictions;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Scheduler.AgentRestrictions
{
	[TestFixture]
	public class AgentRestrictionsDisplayRowTest
	{
		private IAgentDisplayData _dataTarget;
		private AgentRestrictionsDisplayRow _displayRow;
		private MockRepository _mocks;
		private IScheduleMatrixPro _matrix;
		private IVirtualSchedulePeriod _schedulePeriod;
		private DateOnlyPeriod _dateOnlyPeriod;
		private TimeSpan _timeSpan;
		private int _daysOff;
		private IContract _contract;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_matrix = _mocks.StrictMock<IScheduleMatrixPro>();
			_dataTarget = new AgentRestrictionsDisplayRow(_matrix);
			_displayRow = new AgentRestrictionsDisplayRow(_matrix);
			_schedulePeriod = _mocks.StrictMock<IVirtualSchedulePeriod>();
			_dateOnlyPeriod = new DateOnlyPeriod(2011, 1, 1, 2011, 1, 30);
			_timeSpan = new TimeSpan(100 ,0 ,0);
			_daysOff = 5;
			_contract = _mocks.StrictMock<IContract>();
		}

		[Test]
		public void VerifyDefaultProperties()
		{
			Assert.AreSame(_matrix, _dataTarget.Matrix);
			Assert.AreEqual(AgentRestrictionDisplayRowState.NotAvailable, _displayRow.State);	
		}

		[Test]
		public void ShouldGetSetState()
		{
			_displayRow.State = AgentRestrictionDisplayRowState.Loading;
			Assert.AreEqual(AgentRestrictionDisplayRowState.Loading, _displayRow.State);
		}

		[Test]
		public void ShouldGetSetAgentName()
		{
			_displayRow.AgentName = "AgentName";
			Assert.AreEqual("AgentName", _displayRow.AgentName);
		}

		[Test]
		public void ShouldSetWarnings()
		{
			using(_mocks.Record())
			{
				Expect.Call(_matrix.SchedulePeriod).Return(_schedulePeriod).Repeat.AtLeastOnce();
				Expect.Call(_schedulePeriod.DaysOff()).Return(11);
				Expect.Call(_schedulePeriod.Contract).Return(_contract);
				Expect.Call(_contract.EmploymentType).Return(EmploymentType.FixedStaffNormalWorkTime);
			}

			using(_mocks.Playback())
			{
				Assert.AreEqual(0, _displayRow.Warnings);

				_displayRow.ContractTargetTime = TimeSpan.FromMinutes(10);
				_displayRow.ContractCurrentTime = TimeSpan.FromMinutes(11);
				_displayRow.CurrentDaysOff = 12;
				_displayRow.NoWorkshiftFound = true;

				((IAgentDisplayData) _displayRow).MinimumPossibleTime = TimeSpan.FromMinutes(17);
				((IAgentDisplayData)_displayRow).MaximumPossibleTime = TimeSpan.FromMinutes(14);
				_displayRow.MinMaxTime = new TimePeriod(TimeSpan.FromMinutes(15), TimeSpan.FromMinutes(16));

				_displayRow.SetWarnings();

				Assert.AreEqual(UserTexts.Resources.No, _displayRow.Ok);
				Assert.AreEqual(5, _displayRow.Warnings);
				Assert.IsNotNull(_displayRow.Warning((int)AgentRestrictionDisplayRowColumn.ContractTime));
				Assert.IsNotNull(_displayRow.Warning((int)AgentRestrictionDisplayRowColumn.DaysOffSchedule));
				Assert.IsNotNull(_displayRow.Warning((int)AgentRestrictionDisplayRowColumn.Min));
				Assert.IsNotNull(_displayRow.Warning((int)AgentRestrictionDisplayRowColumn.Max));
				Assert.IsNull(_displayRow.Warning((int)AgentRestrictionDisplayRowColumn.Type));
				Assert.IsNotNull(_displayRow.Warning((int)AgentRestrictionDisplayRowColumn.Ok));
			}	
		}

		[Test]
		public void ShouldGetPeriodTypeDay()
		{
			using(_mocks.Record())
			{
				Expect.Call(_matrix.SchedulePeriod).Return(_schedulePeriod);
				Expect.Call(_schedulePeriod.PeriodType).Return(SchedulePeriodType.Day);
			}

			using(_mocks.Playback())
			{
				Assert.AreEqual(UserTexts.Resources.SchedulePeriodTypeDay, _displayRow.PeriodType);
			}
		}

		[Test]
		public void ShouldGetPeriodTypeMonth()
		{
			using (_mocks.Record())
			{
				Expect.Call(_matrix.SchedulePeriod).Return(_schedulePeriod);
				Expect.Call(_schedulePeriod.PeriodType).Return(SchedulePeriodType.Month);
			}

			using (_mocks.Playback())
			{
				Assert.AreEqual(UserTexts.Resources.SchedulePeriodTypeMonth, _displayRow.PeriodType);
			}
		}

		[Test]
		public void ShouldGetPeriodTypeWeek()
		{
			using (_mocks.Record())
			{
				Expect.Call(_matrix.SchedulePeriod).Return(_schedulePeriod);
				Expect.Call(_schedulePeriod.PeriodType).Return(SchedulePeriodType.Week);
			}

			using (_mocks.Playback())
			{
				Assert.AreEqual(UserTexts.Resources.SchedulePeriodTypeWeek, _displayRow.PeriodType);
			}
		}

		[Test]
		public void ShouldGetPeriodTypeChineseMonth()
		{
			using (_mocks.Record())
			{
				Expect.Call(_matrix.SchedulePeriod).Return(_schedulePeriod);
				Expect.Call(_schedulePeriod.PeriodType).Return(SchedulePeriodType.ChineseMonth);
			}

			using (_mocks.Playback())
			{
				Assert.AreEqual(UserTexts.Resources.SchedulePeriodTypeChineseMonth, _displayRow.PeriodType);
			}
		}

		[Test]
		public void ShouldGetPeriodFrom()
		{
			using(_mocks.Record())
			{
				Expect.Call(_matrix.SchedulePeriod).Return(_schedulePeriod);
				Expect.Call(_schedulePeriod.DateOnlyPeriod).Return(_dateOnlyPeriod);
			}

			using(_mocks.Playback())
			{
				Assert.AreEqual(_dateOnlyPeriod.StartDate.ToShortDateString(TeleoptiPrincipal.Current.Regional.Culture), _displayRow.StartDate);		
			}
		}

		[Test]
		public void ShouldGetPeriodTo()
		{
			using (_mocks.Record())
			{
				Expect.Call(_matrix.SchedulePeriod).Return(_schedulePeriod);
				Expect.Call(_schedulePeriod.DateOnlyPeriod).Return(_dateOnlyPeriod);
			}

			using (_mocks.Playback())
			{
				Assert.AreEqual(_dateOnlyPeriod.EndDate.ToShortDateString(TeleoptiPrincipal.Current.Regional.Culture), _displayRow.EndDate);
			}
		}

		[Test]
		public void ShouldGetSetContractTargetTime()
		{
			_displayRow.ContractTargetTime = _timeSpan;
			Assert.AreEqual(_timeSpan, _displayRow.ContractTargetTime);
		}

		[Test]
		public void ShouldGetContractTargetTimeWithTolerance()
		{
			var minMax = new TimePeriod(TimeSpan.FromHours(10), TimeSpan.FromHours(20));
			_displayRow.MinMaxTime = minMax;
			_displayRow.ContractTargetTime = _timeSpan;
			
			var expectedString = TimeHelper.GetLongHourMinuteTimeString(_timeSpan, TeleoptiPrincipal.Current.Regional.Culture) +
					" (" + TimeHelper.GetLongHourMinuteTimeString(minMax.StartTime, TeleoptiPrincipal.Current.Regional.Culture) +
					" - " + TimeHelper.GetLongHourMinuteTimeString(minMax.EndTime, TeleoptiPrincipal.Current.Regional.Culture) +
					")";

			Assert.AreEqual(expectedString, _displayRow.ContractTargetTimeWithTolerance);	
		}

		[Test]
		public void ShouldGetContractTargetTimeHourlyEmployees()
		{
			_displayRow.ContractTargetTime = _timeSpan;

			var expectedString = TimeHelper.GetLongHourMinuteTimeString(_timeSpan, TeleoptiPrincipal.Current.Regional.Culture);

			Assert.AreEqual(expectedString, _displayRow.ContractTargetTimeHourlyEmployees);	
		}

		[Test]
		public void ShouldGetSchedulePeriodTargetDaysOffForAllButHourlyEmployees()
		{
			using(_mocks.Record())
			{
				Expect.Call(_matrix.SchedulePeriod).Return(_schedulePeriod).Repeat.AtLeastOnce();
				Expect.Call(_schedulePeriod.DaysOff()).Return(_daysOff);
				Expect.Call(_schedulePeriod.Contract).Return(_contract);
				Expect.Call(_contract.EmploymentType).Return(EmploymentType.FixedStaffNormalWorkTime);
			}

			using(_mocks.Playback())
			{
				Assert.AreEqual(_daysOff, _displayRow.TargetDaysOff);	
			}
		}

		[Test]
		public void ShouldGetSchedulePeriodTargetDaysOffForHourlyEmployees()
		{
			using (_mocks.Record())
			{
				Expect.Call(_matrix.SchedulePeriod).Return(_schedulePeriod);
				Expect.Call(_schedulePeriod.Contract).Return(_contract);
				Expect.Call(_contract.EmploymentType).Return(EmploymentType.HourlyStaff);
			}

			using (_mocks.Playback())
			{
				Assert.AreEqual(0, _displayRow.TargetDaysOff);
			}
		}

		[Test]
		public void ShouldGetSchedulePeriodTargetDaysOffWithTolerance()
		{
			var expectedString = _daysOff + " (" + (_daysOff - 1) +
							   " - " + (_daysOff + 1) +
							   ")";

			using(_mocks.Record())
			{
				Expect.Call(_matrix.SchedulePeriod).Return(_schedulePeriod).Repeat.AtLeastOnce();
				Expect.Call(_schedulePeriod.DaysOff()).Return(_daysOff).Repeat.AtLeastOnce();
				Expect.Call(_schedulePeriod.Contract).Return(_contract).Repeat.AtLeastOnce();
				Expect.Call(_contract.NegativeDayOffTolerance).Return(1);
				Expect.Call(_contract.PositiveDayOffTolerance).Return(1);
				Expect.Call(_contract.EmploymentType).Return(EmploymentType.FixedStaffNormalWorkTime).Repeat.AtLeastOnce();
			}

			using(_mocks.Playback())
			{
				Assert.AreEqual(expectedString, _displayRow.TargetDaysOffWithTolerance);		
			}	
		}

		[Test]
		public void ShouldGetSetCurrentContractTime()
		{
			_displayRow.ContractCurrentTime = _timeSpan;
			Assert.AreEqual(_timeSpan, _displayRow.ContractCurrentTime);
		}

		[Test]
		public void ShouldGetSetCurrentDaysOff()
		{
			_displayRow.CurrentDaysOff = _daysOff;
			Assert.AreEqual(_daysOff, _displayRow.CurrentDaysOff);
		}

		[Test]
		public void ShouldGetSetNoShiftsFound()
		{
			_displayRow.NoWorkshiftFound = true;
			Assert.IsTrue(_displayRow.NoWorkshiftFound);
		}

		//[Test]
		//public void ShouldGetSetThreadIndex()
		//{
		//    _displayRow.ThreadIndex = 1;
		//    Assert.AreEqual(1, _displayRow.ThreadIndex);
		//}
	}
}
