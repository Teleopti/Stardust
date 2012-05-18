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
		public void ShouldGetSetWarnings()
		{
			Assert.AreEqual(0, _displayRow.Warnings);
			_displayRow.SetWarning(AgentRestrictionDisplayRowColumn.ContractTargetTime, "warning");
			Assert.AreEqual(1, _displayRow.Warnings);
			Assert.AreEqual("warning", _displayRow.Warning(5));
			Assert.AreEqual(null, _displayRow.Warning(1));	
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
				Assert.AreEqual(UserTexts.Resources.Day, _displayRow.PeriodType);
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
				Assert.AreEqual(UserTexts.Resources.Month, _displayRow.PeriodType);
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
				Assert.AreEqual(UserTexts.Resources.Week, _displayRow.PeriodType);
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
		public void ShouldGetSchedulePeriodTargetDaysOff()
		{
			using(_mocks.Record())
			{
				Expect.Call(_matrix.SchedulePeriod).Return(_schedulePeriod);
				Expect.Call(_schedulePeriod.DaysOff()).Return(_daysOff);
			}

			using(_mocks.Playback())
			{
				Assert.AreEqual(_daysOff, _displayRow.TargetDaysOff);	
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
		public void ShouldGetOk()
		{
			Assert.AreEqual(UserTexts.Resources.Yes, _displayRow.Ok);
			_displayRow.SetWarning(AgentRestrictionDisplayRowColumn.Min, "warning");
			Assert.AreEqual(UserTexts.Resources.No, _displayRow.Ok);	
		}
	}
}
