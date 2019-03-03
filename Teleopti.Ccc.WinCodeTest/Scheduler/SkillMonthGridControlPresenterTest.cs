using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.SkillResult;
using Teleopti.Ccc.TestCommon;


namespace Teleopti.Ccc.WinCodeTest.Scheduler
{
	[TestFixture]
	public class SkillMonthGridControlPresenterTest
	{
		private SkillMonthGridControlPresenter _presenter;
		private ISkillMonthGridControl _view;
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
			_view = _mocks.StrictMock<ISkillMonthGridControl>();
			_stateHolder = _mocks.StrictMock<ISchedulerStateHolder>();
			_skill = _mocks.StrictMock<ISkill>();
			_presenter = new SkillMonthGridControlPresenter(_view);
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
			_startTime = new DateTime(2012, 9, 1, 0, 0, 0, DateTimeKind.Utc);
			_endTime = new DateTime(2012, 9, 30, 23, 59, 59, DateTimeKind.Utc);
			_dateTimePeriod = new DateTimePeriod(_startTime, _endTime);
			var dateOnlyPeriod = new DateOnlyPeriod(new DateOnly(_startTime), new DateOnly(_endTime));

			using (_mocks.Record())
			{
				Expect.Call(_stateHolder.RequestedPeriod).Return(_dateOnlyPeriodAsDateTimePeriod);
				Expect.Call(_dateOnlyPeriodAsDateTimePeriod.Period()).Return(_dateTimePeriod);
				Expect.Call(_skill.IsVirtual).Return(true);
				Expect.Call(_stateHolder.SchedulingResultState).Return(_schedulingResultStateHolder);
				Expect.Call(_schedulingResultStateHolder.SkillStaffPeriodHolder).Return(_skillStaffPeriodHolder);
				Expect.Call(_skillStaffPeriodHolder.SkillStaffPeriodList(_skill, dateOnlyPeriod.ToDateTimePeriod(TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal.Regional.TimeZone))).IgnoreArguments().Return(new List<ISkillStaffPeriod>());
				Expect.Call(_view.TimeZoneGuard).Return(new FakeTimeZoneGuard()).Repeat.Any();
			}

			using (_mocks.Playback())
			{
				var dataSource = _presenter.CreateDataSource(_stateHolder, _skill);
				Assert.IsNotNull(dataSource);
			}
		}

		[Test]
		public void ShouldCreateDataSourceOnNonVirtualSkill()
		{
			_startTime = new DateTime(2012, 9, 1, 0, 0, 0, DateTimeKind.Utc);
			_endTime = new DateTime(2012, 9, 30, 23, 59, 59, DateTimeKind.Utc);
			_dateTimePeriod = new DateTimePeriod(_startTime, _endTime);
			var dateOnlyPeriod = new DateOnlyPeriod(new DateOnly(_startTime), new DateOnly(_endTime));
		
			using (_mocks.Record())
			{
				Expect.Call(_stateHolder.RequestedPeriod).Return(_dateOnlyPeriodAsDateTimePeriod);
				Expect.Call(_dateOnlyPeriodAsDateTimePeriod.Period()).Return(_dateTimePeriod);
				Expect.Call(_skill.IsVirtual).Return(false);
				Expect.Call(_stateHolder.SchedulingResultState).Return(_schedulingResultStateHolder);
				Expect.Call(_schedulingResultStateHolder.SkillStaffPeriodHolder).Return(_skillStaffPeriodHolder);
				Expect.Call(_skillStaffPeriodHolder.SkillStaffPeriodList(new List<ISkill> { _skill }, dateOnlyPeriod.ToDateTimePeriod(TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal.Regional.TimeZone))).IgnoreArguments().Return(new List<ISkillStaffPeriod>());
				Expect.Call(_view.TimeZoneGuard).Return(new FakeTimeZoneGuard()).Repeat.Any();
			}

			using (_mocks.Playback())
			{
				var dataSource = _presenter.CreateDataSource(_stateHolder, _skill);
				Assert.IsNotNull(dataSource);
			}
		}

		[Test]
		public void ShouldDrawMonthGrid()
		{
			_startTime = new DateTime(2012, 9, 1, 0, 0, 0, DateTimeKind.Utc);
			_endTime = new DateTime(2012, 9, 30, 23, 59, 59, DateTimeKind.Utc);
			_dateTimePeriod = new DateTimePeriod(_startTime, _endTime);

			using (_mocks.Record())
			{
				Expect.Call(_stateHolder.RequestedPeriod).Return(_dateOnlyPeriodAsDateTimePeriod);
				Expect.Call(_dateOnlyPeriodAsDateTimePeriod.Period()).Return(_dateTimePeriod);
				Expect.Call(() => _view.CreateGridRows(_skill, null, _stateHolder)).IgnoreArguments();
				Expect.Call(() => _view.SetDataSource(_stateHolder, _skill));
				Expect.Call(() => _view.SetupGrid(1));
				Expect.Call(_view.TimeZoneGuard).Return(new FakeTimeZoneGuard()).Repeat.Any();
			}

			using (_mocks.Playback())
			{
				_presenter.DrawMonthGrid(_stateHolder, _skill);
			}
		}

		[Test]
		public void ShouldSetDates()
		{
			_startTime = new DateTime(2012, 9, 1, 0, 0, 0, DateTimeKind.Utc);
			_endTime = new DateTime(2012, 9, 30, 23, 59, 59, DateTimeKind.Utc);
			_dateTimePeriod = new DateTimePeriod(_startTime, _endTime);

			using (_mocks.Record())
			{
				Expect.Call(_stateHolder.RequestedPeriod).Return(_dateOnlyPeriodAsDateTimePeriod);
				Expect.Call(_dateOnlyPeriodAsDateTimePeriod.Period()).Return(_dateTimePeriod);
				Expect.Call(_view.TimeZoneGuard).Return(new FakeTimeZoneGuard()).Repeat.Any();
			}

			using (_mocks.Playback())
			{
				_presenter.SetDates(_stateHolder);

				Assert.AreEqual(30, _presenter.Dates.Count);
				Assert.AreEqual(new DateOnly(2012, 9, 1), _presenter.Dates[0]);
				Assert.AreEqual(new DateOnly(2012, 9, 30), _presenter.Dates[29]);
			}
		}

		[Test]
		public void ShouldSetMonths()
		{
			_startTime = new DateTime(2012, 9, 1, 0, 0, 0, DateTimeKind.Utc);
			_endTime = new DateTime(2012, 9, 30, 23, 59, 59, DateTimeKind.Utc);
			_dateTimePeriod = new DateTimePeriod(_startTime, _endTime);

			using (_mocks.Record())
			{
				Expect.Call(_stateHolder.RequestedPeriod).Return(_dateOnlyPeriodAsDateTimePeriod);
				Expect.Call(_dateOnlyPeriodAsDateTimePeriod.Period()).Return(_dateTimePeriod);
				Expect.Call(_view.TimeZoneGuard).Return(new FakeTimeZoneGuard()).Repeat.Any();
			}

			using (_mocks.Playback())
			{
				_presenter.SetDates(_stateHolder);
				_presenter.SetMonths();

				Assert.AreEqual(1, _presenter.Months.Count);
			}
		}

		[Test]
		public void ShouldOnlySetFullMonths()
		{
			_startTime = new DateTime(2012, 9, 1, 0, 0, 0, DateTimeKind.Utc);
			_endTime = new DateTime(2012, 9, 25, 0, 0, 0, DateTimeKind.Utc);
			_dateTimePeriod = new DateTimePeriod(_startTime, _endTime);

			using (_mocks.Record())
			{
				Expect.Call(_stateHolder.RequestedPeriod).Return(_dateOnlyPeriodAsDateTimePeriod);
				Expect.Call(_dateOnlyPeriodAsDateTimePeriod.Period()).Return(_dateTimePeriod);
				Expect.Call(_view.TimeZoneGuard).Return(new FakeTimeZoneGuard()).Repeat.Any();
			}

			using (_mocks.Playback())
			{
				_presenter.SetDates(_stateHolder);
				_presenter.SetMonths();

				Assert.AreEqual(0, _presenter.Months.Count);
			}
		}
	}
}
