using System;
using System.Collections.Generic;
using NUnit.Framework;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.TestCommon.FakeData;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.UndoRedo;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.ClipBoard;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.AgentRestrictions;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.RestrictionSummary;
using Teleopti.Ccc.TestCommon;


namespace Teleopti.Ccc.WinCodeTest.Scheduler.AgentRestrictions
{
	[TestFixture]
	public class AgentRestrictionsDetailPresenterTest : IDisposable
	{
		private AgentRestrictionsDetailPresenter _presenter;
		private IAgentRestrictionsDetailView _view;
		private IAgentRestrictionsDetailModel _model;
		private ISchedulerStateHolder _schedulerStateHolder;
	    private ISchedulingResultStateHolder _schedulingResultStateHolder;
		private IGridlockManager _gridlockManager;
		private ClipHandler<IScheduleDay> _clipHandler;
		private IOverriddenBusinessRulesHolder _overriddenBusinessRulesHolder;
		private IScheduleDayChangeCallback _scheduleDayChangeCallback;
		private GridStyleInfo _info;
		private IPreferenceCellData _preferenceCellData;

		private DateTime _dateTime;
		private IScenario _scenario;
		private DateTimePeriod _dateTimePeriod;
		private IPerson _person;

		[SetUp]
		public void Setup()
		{
			_view = MockRepository.GenerateMock<IAgentRestrictionsDetailView>();
			_person = PersonFactory.CreatePerson("Jens").WithId();
			_scenario = ScenarioFactory.CreateScenarioAggregate();

			_dateTime = new DateTime(2012, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			_dateTimePeriod = new DateTimePeriod(_dateTime, _dateTime.AddDays(30));
			_model = new AgentRestrictionsDetailModel(_dateTimePeriod);
			_schedulingResultStateHolder = new SchedulingResultStateHolder();
			_schedulerStateHolder = new SchedulerStateHolder(_scenario,
				new DateOnlyPeriodAsDateTimePeriod(new DateOnlyPeriod(2012, 1, 1, 2012, 1, 1), TimeZoneInfo.Utc), new[] {_person},
				MockRepository.GenerateMock<IDisableDeletedFilter>(), _schedulingResultStateHolder,
				new FakeTimeZoneGuard(TimeZoneInfo.Utc));

		    _gridlockManager = new GridlockManager();
			_clipHandler = new ClipHandler<IScheduleDay>();
			_overriddenBusinessRulesHolder = new OverriddenBusinessRulesHolder();
			_scheduleDayChangeCallback = new DoNothingScheduleDayChangeCallBack();
			_presenter = new AgentRestrictionsDetailPresenter(_view, _model, _schedulerStateHolder, _gridlockManager, _clipHandler, SchedulePartFilter.None, _overriddenBusinessRulesHolder, _scheduleDayChangeCallback, NullScheduleTag.Instance, new UndoRedoContainer());
			_info = new GridStyleInfo();
			_preferenceCellData = new PreferenceCellData();
			
			_preferenceCellData.SchedulePart = ScheduleDayFactory.Create(new DateOnly(2012, 1, 1),_person,_scenario);
			_schedulingResultStateHolder.Schedules = _preferenceCellData.SchedulePart.Owner;
		}

		[Test]
		public void ShouldHaveSevenCols()
		{
			Assert.AreEqual(7, _presenter.ColCount);	
		}

		[Test]
		public void ShouldReturnRowCount()
		{
			var detailData = _model.DetailData();
			detailData.Add(0, null);
			detailData.Add(1, null);

			//because of constructor in SchedulePresenterBase
			var period = _schedulerStateHolder.RequestedPeriod;
			Assert.AreEqual(1, _presenter.RowCount);
		}

		[Test]
		public void ShouldQueryCellInfo()
		{
			var e = new GridQueryCellInfoEventArgs(-1, -1, _info);
			_presenter.QueryCellInfo(null, e);
			Assert.AreEqual(string.Empty, e.Style.CellValue.ToString());

			e = new GridQueryCellInfoEventArgs(0, 0, _info);
			_presenter.QueryCellInfo(null, e);
			Assert.AreEqual(string.Empty, e.Style.CellValue.ToString());
		}

		[Test]
		public void ShouldQueryCellInfoWeekday()
		{
			_preferenceCellData.TheDate = new DateOnly(2012,1,1);
			_model.DetailData().Add(0, _preferenceCellData);
			var e = new GridQueryCellInfoEventArgs(0, 1, _info);
			_presenter.QueryCellInfo(null, e);
			var expectedDay =
				TeleoptiPrincipalForLegacy.CurrentPrincipal.Regional.Culture.DateTimeFormat.GetDayName(
					TeleoptiPrincipalForLegacy.CurrentPrincipal.Regional.Culture.Calendar.GetDayOfWeek(new DateTime(2012, 1, 1)));
			//because of constructor in SchedulePresenterBase
			var period = _schedulerStateHolder.RequestedPeriod;
			Assert.AreEqual(expectedDay, e.Style.CellValue.ToString());
		}

		[Test]
		public void VerifyQueryCellInfoWeekHeaderWhenDayNotScheduled()
		{
			var startTimeLimitation = new StartTimeLimitation(new TimeSpan(7, 0, 0), null);
			var endTimeLimitation = new EndTimeLimitation(new TimeSpan(0, 20, 0), null);
			var workTimeLimitation = new WorkTimeLimitation(new TimeSpan(8, 0, 0), new TimeSpan(12, 0, 0));
			var shiftCategory = ShiftCategoryFactory.CreateShiftCategory("Natt");
			var effectiveRestriction = new EffectiveRestriction(startTimeLimitation, endTimeLimitation, workTimeLimitation,
				shiftCategory, null, null, new List<IActivityRestriction>());
			_preferenceCellData.EffectiveRestriction = effectiveRestriction;

			_preferenceCellData.SchedulingOption = new RestrictionSchedulingOptions();
			_preferenceCellData.SchedulingOption.UseScheduling = true;
			_model.DetailData().Add(0, _preferenceCellData);

			//because of constructor in SchedulePresenterBase
			var period = _schedulerStateHolder.RequestedPeriod;

			var e = new GridQueryCellInfoEventArgs(1, 0, _info);
			_presenter.QueryCellInfo(null, e);

			var weekHeaderCellData = _presenter.OnQueryWeekHeader(1);
			Assert.IsNotNull(weekHeaderCellData);

			_preferenceCellData.EffectiveRestriction = null;
			weekHeaderCellData = _presenter.OnQueryWeekHeader(1);
			Assert.IsNotNull(weekHeaderCellData);
		}

		[Test]
		public void VerifyQueryCellInfoWeekHeaderWhenDayScheduled()
		{
			var preferenceCellData = new PreferenceCellData();
			var startTimeLimitation = new StartTimeLimitation(new TimeSpan(7, 0, 0), null);
			var endTimeLimitation = new EndTimeLimitation(new TimeSpan(0, 20, 0), null);
			var workTimeLimitation = new WorkTimeLimitation(new TimeSpan(8, 0, 0), new TimeSpan(12, 0, 0));
			var shiftCategory = ShiftCategoryFactory.CreateShiftCategory("Natt");
			var effectiveRestriction = new EffectiveRestriction(startTimeLimitation, endTimeLimitation, workTimeLimitation,
				shiftCategory, null, null, new List<IActivityRestriction>());

			_preferenceCellData.EffectiveRestriction = effectiveRestriction;
			_preferenceCellData.SchedulingOption = new RestrictionSchedulingOptions();
			_preferenceCellData.SchedulingOption.UseScheduling = true;
			var detailData = _model.DetailData();
			detailData.Clear();
			detailData.Add(0, _preferenceCellData);

			_preferenceCellData.SchedulePart.AddMainShift(PersonAssignmentFactory.CreateAssignmentWithMainShift(_person, _scenario, new DateTimePeriod(2012, 1, 1, 8, 2012, 1, 1, 16)));

			//because of constructor in SchedulePresenterBase
			var period = _schedulerStateHolder.RequestedPeriod;

			var e = new GridQueryCellInfoEventArgs(1, 0, _info);
			_presenter.QueryCellInfo(null, e);

			var weekHeaderCellData = _presenter.OnQueryWeekHeader(1);
			Assert.IsNotNull(weekHeaderCellData);

			preferenceCellData.EffectiveRestriction = null;
			weekHeaderCellData = _presenter.OnQueryWeekHeader(1);
			Assert.IsNotNull(weekHeaderCellData);
		}

		[Test]
		public void ShouldDisplayCorrectWeekNumberInBeginningOf2016()
		{
			var startTimeLimitation = new StartTimeLimitation(new TimeSpan(7, 0, 0), null);
			var endTimeLimitation = new EndTimeLimitation(new TimeSpan(0, 20, 0), null);
			var workTimeLimitation = new WorkTimeLimitation(new TimeSpan(8, 0, 0), new TimeSpan(12, 0, 0));
			var shiftCategory = ShiftCategoryFactory.CreateShiftCategory("Natt");
			var effectiveRestriction = new EffectiveRestriction(startTimeLimitation, endTimeLimitation, workTimeLimitation,
				shiftCategory, null, null, new List<IActivityRestriction>());
			_preferenceCellData.SchedulingOption = new RestrictionSchedulingOptions { UseScheduling = true };
			_preferenceCellData.EffectiveRestriction = effectiveRestriction;

			var detailData = _model.DetailData();
			detailData.Clear();
			detailData.Add(0, _preferenceCellData);

			_preferenceCellData.TheDate = new DateOnly(2016, 1, 1);

			//because of constructor in SchedulePresenterBase
			var period = _schedulerStateHolder.RequestedPeriod;

			var weekHeaderCellData = _presenter.OnQueryWeekHeader(1);
			weekHeaderCellData.WeekNumber.Should().Be.EqualTo(53);
		}

		[Test]
		public void ShouldAlertWhenBelowMin()
		{
			var startTimeLimitation = new StartTimeLimitation(new TimeSpan(7, 0, 0), null);
			var endTimeLimitation = new EndTimeLimitation(new TimeSpan(0, 20, 0), null);
			var workTimeLimitation = new WorkTimeLimitation(new TimeSpan(8, 0, 0), new TimeSpan(8, 0, 0));
			var shiftCategory = ShiftCategoryFactory.CreateShiftCategory("Natt");
			var effectiveRestriction = new EffectiveRestriction(startTimeLimitation, endTimeLimitation, workTimeLimitation,
				shiftCategory, null, null, new List<IActivityRestriction>());
			_preferenceCellData.EffectiveRestriction = effectiveRestriction;

			_preferenceCellData.SchedulingOption = new RestrictionSchedulingOptions {UseScheduling = true};
			_model.DetailData().Add(0, _preferenceCellData);
			_preferenceCellData.WeeklyMax = TimeSpan.FromHours(10);
			_preferenceCellData.WeeklyMin = TimeSpan.FromHours(9);

			_preferenceCellData.SchedulePart.AddMainShift(PersonAssignmentFactory.CreateAssignmentWithMainShift(_person, _scenario, new DateTimePeriod(2012, 1, 1, 8, 2012, 1, 1, 16)));

			//because of constructor in SchedulePresenterBase
			var period = _schedulerStateHolder.RequestedPeriod;
			var weekHeaderCellData = _presenter.OnQueryWeekHeader(1);
			Assert.IsNotNull(weekHeaderCellData);
			Assert.IsNotNull(weekHeaderCellData);
			Assert.IsTrue(weekHeaderCellData.Alert);
		}

		[Test]
		public void ShouldAlertWhenAboveMax()
		{
			var startTimeLimitation = new StartTimeLimitation(new TimeSpan(7, 0, 0), null);
			var endTimeLimitation = new EndTimeLimitation(new TimeSpan(0, 20, 0), null);
			var workTimeLimitation = new WorkTimeLimitation(new TimeSpan(8, 0, 0), new TimeSpan(8, 0, 0));
			var shiftCategory = ShiftCategoryFactory.CreateShiftCategory("Natt");
			var effectiveRestriction = new EffectiveRestriction(startTimeLimitation, endTimeLimitation, workTimeLimitation,
				shiftCategory, null, null, new List<IActivityRestriction>());
			_preferenceCellData.EffectiveRestriction = effectiveRestriction;

			_preferenceCellData.SchedulingOption = new RestrictionSchedulingOptions {UseScheduling = true};
			_model.DetailData().Add(0, _preferenceCellData);
			_preferenceCellData.WeeklyMax = TimeSpan.FromHours(7);
			_preferenceCellData.WeeklyMin = TimeSpan.FromHours(6);
			
			//because of constructor in SchedulePresenterBase
			var period = _schedulerStateHolder.RequestedPeriod;
			var weekHeaderCellData = _presenter.OnQueryWeekHeader(1);
			Assert.IsNotNull(weekHeaderCellData);
			Assert.IsTrue(weekHeaderCellData.Alert);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				_info.Dispose();
			}
		}
	}
}
