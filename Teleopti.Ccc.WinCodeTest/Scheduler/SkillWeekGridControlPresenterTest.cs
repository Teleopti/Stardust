using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.SkillResult;


namespace Teleopti.Ccc.WinCodeTest.Scheduler
{
	[TestFixture, SetCulture("sv-SE")]
	public class SkillWeekGridControlPresenterTest
	{
		private SkillWeekGridControlPresenter _presenter;
		private ISkillWeekGridControl _view;
		private MockRepository _mocks;
		private ISchedulerStateHolder _stateHolder;
		private ISkill _skill;
		private IDateOnlyPeriodAsDateTimePeriod _dateOnlyPeriodAsDateTimePeriod;
		private DateTimePeriod _dateTimePeriod;
		private DateTime _startTime;
		private DateTime _endTime;
		private ISchedulingResultStateHolder _schedulingResultStateHolder;
		private ISkillStaffPeriodHolder _skillStaffPeriodHolder;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_view = _mocks.StrictMock<ISkillWeekGridControl>();
			_stateHolder = _mocks.StrictMock<ISchedulerStateHolder>();
			_skill = _mocks.StrictMock<ISkill>();
			_presenter = new SkillWeekGridControlPresenter(_view);
			_dateOnlyPeriodAsDateTimePeriod = _mocks.StrictMock<IDateOnlyPeriodAsDateTimePeriod>();
			_schedulingResultStateHolder = _mocks.StrictMock<ISchedulingResultStateHolder>();
			_skillStaffPeriodHolder = _mocks.StrictMock<ISkillStaffPeriodHolder>();
		}

		[Test]
		public void ShouldNotCreateDataSourceWhenSkillIsNull()
		{
			var dataSource = _presenter.CreateDataSource(_stateHolder, null);
			Assert.IsNull(dataSource);
		}

		[Test]
		public void ShouldCreateDataSourceOnVirtualSkill()
		{
			_startTime = new DateTime(2012, 9, 3, 0, 0, 0, DateTimeKind.Utc);
			_endTime = new DateTime(2012, 9, 9, 23, 59, 59, DateTimeKind.Utc);
			_dateTimePeriod = new DateTimePeriod(_startTime, _endTime);
			var dateOnlyPeriod = new DateOnlyPeriod(new DateOnly(_startTime), new DateOnly(_endTime));

			using (_mocks.Record())
			{
				Expect.Call(_stateHolder.RequestedPeriod).Return(_dateOnlyPeriodAsDateTimePeriod);
				Expect.Call(_dateOnlyPeriodAsDateTimePeriod.Period()).Return(_dateTimePeriod);
				Expect.Call(_stateHolder.TimeZoneInfo).Return(TeleoptiPrincipal.CurrentPrincipal.Regional.TimeZone).Repeat.AtLeastOnce();
				Expect.Call(_skill.IsVirtual).Return(true);
				Expect.Call(_stateHolder.SchedulingResultState).Return(_schedulingResultStateHolder);
				Expect.Call(_schedulingResultStateHolder.SkillStaffPeriodHolder).Return(_skillStaffPeriodHolder);
				Expect.Call(_skillStaffPeriodHolder.SkillStaffPeriodList(_skill, dateOnlyPeriod.ToDateTimePeriod(TeleoptiPrincipal.CurrentPrincipal.Regional.TimeZone))).IgnoreArguments().Return(new List<ISkillStaffPeriod>());
			}

			using (_mocks.Playback())
			{
				var dataSource = _presenter.CreateDataSource(_stateHolder, _skill);
				Assert.IsNotNull(dataSource);
			}
		}

		[Test]
		public void ShouldDrawWeekGrid()
		{
			_startTime = new DateTime(2012, 9, 3, 0, 0, 0, DateTimeKind.Utc);
			_endTime = new DateTime(2012, 9, 9, 23, 59, 59, DateTimeKind.Utc);
			_dateTimePeriod = new DateTimePeriod(_startTime, _endTime);

			using(_mocks.Record())
			{
				Expect.Call(_stateHolder.RequestedPeriod).Return(_dateOnlyPeriodAsDateTimePeriod);
				Expect.Call(_dateOnlyPeriodAsDateTimePeriod.Period()).Return(_dateTimePeriod);
				Expect.Call(_stateHolder.TimeZoneInfo).Return(TeleoptiPrincipal.CurrentPrincipal.Regional.TimeZone).Repeat.AtLeastOnce();
				Expect.Call(() => _view.CreateGridRows(_skill, null, _stateHolder)).IgnoreArguments();
				Expect.Call(() => _view.SetDataSource(_stateHolder, _skill));
				Expect.Call(() => _view.SetupGrid(1));
			}

			using(_mocks.Playback())
			{
				_presenter.DrawWeekGrid(_stateHolder, _skill);
			}
		}

		[Test]
		public void ShouldCreateDataSourceOnNonVirtualSkill()
		{
			_startTime = new DateTime(2012, 9, 3, 0, 0, 0, DateTimeKind.Utc);
			_endTime = new DateTime(2012, 9, 9, 23, 59, 59, DateTimeKind.Utc);
			_dateTimePeriod = new DateTimePeriod(_startTime, _endTime);
			var dateOnlyPeriod = new DateOnlyPeriod(new DateOnly(_startTime), new DateOnly(_endTime));

			using (_mocks.Record())
			{
				Expect.Call(_stateHolder.RequestedPeriod).Return(_dateOnlyPeriodAsDateTimePeriod);
				Expect.Call(_dateOnlyPeriodAsDateTimePeriod.Period()).Return(_dateTimePeriod);
				Expect.Call(_stateHolder.TimeZoneInfo).Return(TeleoptiPrincipal.CurrentPrincipal.Regional.TimeZone).Repeat.AtLeastOnce();
				Expect.Call(_skill.IsVirtual).Return(false);
				Expect.Call(_stateHolder.SchedulingResultState).Return(_schedulingResultStateHolder);
				Expect.Call(_schedulingResultStateHolder.SkillStaffPeriodHolder).Return(_skillStaffPeriodHolder);
				Expect.Call(_skillStaffPeriodHolder.SkillStaffPeriodList(new List<ISkill> { _skill }, dateOnlyPeriod.ToDateTimePeriod(TeleoptiPrincipal.CurrentPrincipal.Regional.TimeZone))).IgnoreArguments().Return(new List<ISkillStaffPeriod>());
			}

			using (_mocks.Playback())
			{
				var dataSource = _presenter.CreateDataSource(_stateHolder, _skill);
				Assert.IsNotNull(dataSource);
			}
		}

		[Test]
		public void ShouldSetDates()
		{
			_startTime = new DateTime(2012,9, 3, 0, 0, 0,DateTimeKind.Utc);
			_endTime = new DateTime(2012, 9, 9, 23, 59, 59, DateTimeKind.Utc);
			_dateTimePeriod = new DateTimePeriod(_startTime, _endTime);

			using(_mocks.Record())
			{
				Expect.Call(_stateHolder.RequestedPeriod).Return(_dateOnlyPeriodAsDateTimePeriod);
				Expect.Call(_dateOnlyPeriodAsDateTimePeriod.Period()).Return(_dateTimePeriod);
				Expect.Call(_stateHolder.TimeZoneInfo).Return(TeleoptiPrincipal.CurrentPrincipal.Regional.TimeZone).Repeat.AtLeastOnce();	
			}

			using(_mocks.Playback())
			{
				_presenter.SetDates(_stateHolder);

				Assert.AreEqual(7, _presenter.Dates.Count);
				Assert.AreEqual(new DateOnly(2012, 9, 3), _presenter.Dates[0]);
				Assert.AreEqual(new DateOnly(2012, 9, 9), _presenter.Dates[6]);
			}
		}

		[Test]
		public void ShouldSetWeeks()
		{
			_startTime = new DateTime(2012, 9, 3, 0, 0, 0, DateTimeKind.Utc);
			_endTime = new DateTime(2012, 9, 16, 23, 59, 59, DateTimeKind.Utc);
			_dateTimePeriod = new DateTimePeriod(_startTime, _endTime);

			using(_mocks.Record())
			{
				Expect.Call(_stateHolder.RequestedPeriod).Return(_dateOnlyPeriodAsDateTimePeriod);
				Expect.Call(_dateOnlyPeriodAsDateTimePeriod.Period()).Return(_dateTimePeriod);
				Expect.Call(_stateHolder.TimeZoneInfo).Return(TeleoptiPrincipal.CurrentPrincipal.Regional.TimeZone).Repeat.AtLeastOnce();	
			}

			using(_mocks.Playback())
			{
				_presenter.SetDates(_stateHolder);
				_presenter.SetWeeks();

				Assert.AreEqual(2, _presenter.Weeks.Count);
			}		
		}

		[Test]
		public void ShouldOnlySetFullWeeks()
		{
			_startTime = new DateTime(2012, 9, 3, 0, 0, 0, DateTimeKind.Utc);
			_endTime = new DateTime(2012, 9, 4, 0, 0, 0, DateTimeKind.Utc);
			_dateTimePeriod = new DateTimePeriod(_startTime, _endTime);

			using (_mocks.Record())
			{
				Expect.Call(_stateHolder.RequestedPeriod).Return(_dateOnlyPeriodAsDateTimePeriod);
				Expect.Call(_dateOnlyPeriodAsDateTimePeriod.Period()).Return(_dateTimePeriod);
				Expect.Call(_stateHolder.TimeZoneInfo).Return(TeleoptiPrincipal.CurrentPrincipal.Regional.TimeZone).Repeat.AtLeastOnce();
			}

			using (_mocks.Playback())
			{
				_presenter.SetDates(_stateHolder);
				_presenter.SetWeeks();

				Assert.AreEqual(0, _presenter.Weeks.Count);
			}	
		}
	}
}
