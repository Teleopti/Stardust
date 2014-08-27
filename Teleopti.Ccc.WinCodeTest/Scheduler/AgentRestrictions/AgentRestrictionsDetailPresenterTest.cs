using System;
using System.Collections.Generic;
using NUnit.Framework;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCode.Common.Clipboard;
using Teleopti.Ccc.WinCode.Scheduling;
using Teleopti.Ccc.WinCode.Scheduling.AgentRestrictions;
using Rhino.Mocks;
using Teleopti.Ccc.WinCode.Scheduling.RestrictionSummary;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Scheduler.AgentRestrictions
{
	[TestFixture]
	public class AgentRestrictionsDetailPresenterTest : IDisposable
	{
		private AgentRestrictionsDetailPresenter _presenter;
		private IAgentRestrictionsDetailView _view;
		private IAgentRestrictionsDetailModel _model;
		private MockRepository _mocks;
		private ISchedulerStateHolder _schedulerStateHolder;
	    private ISchedulingResultStateHolder _schedulingResultStateHolder;
		private IGridlockManager _gridlockManager;
		private ClipHandler<IScheduleDay> _clipHandler;
		private IOverriddenBusinessRulesHolder _overriddenBusinessRulesHolder;
		private IScheduleDayChangeCallback _scheduleDayChangeCallback;
		private GridStyleInfo _info;
		private Dictionary<int, IPreferenceCellData> _detailData;
		private IPreferenceCellData _preferenceCellData;

		private DateTime _dateTime;
		private IScheduleDictionary _scheduleDictionary;
		private IScenario _scenario;
		private IScheduleDateTimePeriod _scheduleDateTimePeriod;
		private DateTimePeriod _dateTimePeriod;
		private IDictionary<IPerson, IScheduleRange> _dictionary;
		private IPerson _person;
		private IScheduleRange _range;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_view = _mocks.StrictMock<IAgentRestrictionsDetailView>();
			_model = _mocks.StrictMock<IAgentRestrictionsDetailModel>();
			_schedulerStateHolder = _mocks.StrictMock<ISchedulerStateHolder>();
		    _schedulingResultStateHolder = _mocks.StrictMock<ISchedulingResultStateHolder>();
			_gridlockManager = _mocks.StrictMock<IGridlockManager>();
			_clipHandler = new ClipHandler<IScheduleDay>();
			_overriddenBusinessRulesHolder = _mocks.StrictMock<IOverriddenBusinessRulesHolder>();
			_scheduleDayChangeCallback = _mocks.DynamicMock<IScheduleDayChangeCallback>();
			_presenter = new AgentRestrictionsDetailPresenter(_view, _model, _schedulerStateHolder, _gridlockManager, _clipHandler, SchedulePartFilter.None, _overriddenBusinessRulesHolder, _scheduleDayChangeCallback, NullScheduleTag.Instance);
			_info = new GridStyleInfo();
			_detailData = new Dictionary<int, IPreferenceCellData>();
			_preferenceCellData = _mocks.StrictMock<IPreferenceCellData>();

			_range = _mocks.StrictMock<IScheduleRange>();
			_person = PersonFactory.CreatePerson("Jens");
			_dictionary = new Dictionary<IPerson, IScheduleRange> { { _person, _range } };
			_dateTime = new DateTime(2012, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			_dateTimePeriod = new DateTimePeriod(_dateTime, _dateTime.AddDays(30));
			_scenario = ScenarioFactory.CreateScenarioAggregate();
			_scheduleDateTimePeriod = new ScheduleDateTimePeriod(_dateTimePeriod);
			_scheduleDictionary = new ScheduleDictionaryForTest(_scenario, _scheduleDateTimePeriod, _dictionary);
		}

		[Test]
		public void ShouldHaveSevenCols()
		{
			Assert.AreEqual(7, _presenter.ColCount);	
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "period"), Test]
		public void ShouldReturnRowCount()
		{
			_detailData.Add(0, null);
			_detailData.Add(1, null);

			using(_mocks.Record())
			{
				Expect.Call(_model.DetailData()).Return(_detailData);
			}

			using(_mocks.Playback())
			{
				//because of constructor in SchedulePresenterBase
				var period = _schedulerStateHolder.RequestedPeriod;
				Assert.AreEqual(1, _presenter.RowCount);	
			}	
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

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "period"), Test]
		public void ShouldQueryCellInfoScheduleDay()
		{
			var part = ExtractedSchedule.CreateScheduleDay(_scheduleDictionary, _person, new DateOnly(_dateTime));

			using(_mocks.Record())
			{
				Expect.Call(_model.DetailData()).Return(_detailData);
				Expect.Call(_preferenceCellData.TheDate).Return(new DateOnly(2012, 1, 1)).Repeat.AtLeastOnce();
				Expect.Call(_preferenceCellData.SchedulePart).Return(part).Repeat.AtLeastOnce();
				Expect.Call(_preferenceCellData.ViolatesNightlyRest).Return(true);
				Expect.Call(_preferenceCellData.NoShiftsCanBeFound).Return(true);
			}

			using(_mocks.Playback())
			{
				//because of constructor in SchedulePresenterBase
				var period = _schedulerStateHolder.RequestedPeriod;
				_detailData.Add(0, _preferenceCellData);
				var e = new GridQueryCellInfoEventArgs(1, 1, _info);
				_presenter.QueryCellInfo(null, e);	
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "period"), Test]
		public void ShouldQueryCellInfoWeekday()
		{
			using (_mocks.Record())
			{
				Expect.Call(_model.DetailData()).Return(_detailData);
				Expect.Call(_preferenceCellData.TheDate).Return(new DateOnly(2012, 1, 1));
			}

			using (_mocks.Playback())
			{
				_detailData.Add(0, _preferenceCellData);
				var e = new GridQueryCellInfoEventArgs(0, 1, _info);
				_presenter.QueryCellInfo(null, e);
				var expectedDay = TeleoptiPrincipal.Current.Regional.Culture.DateTimeFormat.GetDayName(TeleoptiPrincipal.Current.Regional.Culture.Calendar.GetDayOfWeek(new DateOnly(2012, 1, 1)));
				//because of constructor in SchedulePresenterBase
				var period = _schedulerStateHolder.RequestedPeriod;
				Assert.AreEqual(expectedDay, e.Style.CellValue.ToString());
			}	
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "period"), Test]
		public void VerifyQueryCellInfoWeekHeaderWhenDayNotScheduled()
		{
			var preferenceCellData = new PreferenceCellData();
			var startTimeLimitation = new StartTimeLimitation(new TimeSpan(7, 0, 0), null);
			var endTimeLimitation = new EndTimeLimitation(new TimeSpan(0, 20, 0), null);
			var workTimeLimitation = new WorkTimeLimitation(new TimeSpan(8, 0, 0), new TimeSpan(12, 0, 0));
			var shiftCategory = ShiftCategoryFactory.CreateShiftCategory("Natt");
			var effectiveRestriction = new EffectiveRestriction(startTimeLimitation, endTimeLimitation,workTimeLimitation, shiftCategory,null, null, new List<IActivityRestriction>());
			preferenceCellData.EffectiveRestriction = effectiveRestriction;
			
			var schedulePart = _mocks.StrictMock<IScheduleDay>();
			var projectionService = _mocks.StrictMock<IProjectionService>();
			var visualLayerCollection = _mocks.StrictMock<IVisualLayerCollection>();

			preferenceCellData.SchedulePart = schedulePart;
			
			preferenceCellData.SchedulingOption = new RestrictionSchedulingOptions();
			preferenceCellData.SchedulingOption.UseScheduling = true;
			_detailData.Add(0, preferenceCellData);

			using(_mocks.Record())
			{
				Expect.Call(_model.DetailData()).Return(_detailData).Repeat.AtLeastOnce();

				Expect.Call(schedulePart.ProjectionService()).Return(projectionService).Repeat.AtLeastOnce();
				Expect.Call(projectionService.CreateProjection()).Return(visualLayerCollection).Repeat.AtLeastOnce();
				Expect.Call(visualLayerCollection.HasLayers).Return(false).Repeat.AtLeastOnce(); // this line indicates that the day is not scheduled
			    Expect.Call(_schedulerStateHolder.SchedulingResultState).Return(_schedulingResultStateHolder);
			    Expect.Call(_schedulingResultStateHolder.UseMinWeekWorkTime).Return(true);
			}

			using(_mocks.Playback())
			{
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
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "period"), Test]
		public void VerifyQueryCellInfoWeekHeaderWhenDayScheduled()
		{
			var preferenceCellData = new PreferenceCellData();
			var startTimeLimitation = new StartTimeLimitation(new TimeSpan(7, 0, 0), null);
			var endTimeLimitation = new EndTimeLimitation(new TimeSpan(0, 20, 0), null);
			var workTimeLimitation = new WorkTimeLimitation(new TimeSpan(8, 0, 0), new TimeSpan(12, 0, 0));
			var shiftCategory = ShiftCategoryFactory.CreateShiftCategory("Natt");
			var effectiveRestriction = new EffectiveRestriction(startTimeLimitation, endTimeLimitation, workTimeLimitation, shiftCategory, null, null, new List<IActivityRestriction>());
			preferenceCellData.EffectiveRestriction = effectiveRestriction;

			var schedulePart = _mocks.StrictMock<IScheduleDay>();
			var projectionService = _mocks.StrictMock<IProjectionService>();
			var visualLayerCollection = _mocks.StrictMock<IVisualLayerCollection>();

			preferenceCellData.SchedulePart = schedulePart;

			preferenceCellData.SchedulingOption = new RestrictionSchedulingOptions();
			preferenceCellData.SchedulingOption.UseScheduling = true;
			_detailData.Clear();
			_detailData.Add(0, preferenceCellData);

			using (_mocks.Record())
			{
				Expect.Call(_model.DetailData()).Return(_detailData).Repeat.AtLeastOnce();

				Expect.Call(schedulePart.ProjectionService()).Return(projectionService).Repeat.AtLeastOnce();
				Expect.Call(projectionService.CreateProjection()).Return(visualLayerCollection).Repeat.AtLeastOnce();
				Expect.Call(visualLayerCollection.HasLayers).Return(true).Repeat.AtLeastOnce(); // this line indicates that the day is scheduled
				Expect.Call(visualLayerCollection.ContractTime()).Return(new TimeSpan(0, 8, 0, 0)).Repeat.AtLeastOnce();
			}

			using (_mocks.Playback())
			{
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

		}

        [Test]
        public void ShouldAlertWhenBelowMin()
        {
            var preferenceCellData = new PreferenceCellData();
            var startTimeLimitation = new StartTimeLimitation(new TimeSpan(7, 0, 0), null);
            var endTimeLimitation = new EndTimeLimitation(new TimeSpan(0, 20, 0), null);
            var workTimeLimitation = new WorkTimeLimitation(new TimeSpan(8, 0, 0), new TimeSpan(8, 0, 0));
            var shiftCategory = ShiftCategoryFactory.CreateShiftCategory("Natt");
            var effectiveRestriction = new EffectiveRestriction(startTimeLimitation, endTimeLimitation, workTimeLimitation, shiftCategory, null, null, new List<IActivityRestriction>());
            preferenceCellData.EffectiveRestriction = effectiveRestriction;

            var schedulePart = _mocks.StrictMock<IScheduleDay>();
            var projectionService = _mocks.StrictMock<IProjectionService>();
            var visualLayerCollection = _mocks.StrictMock<IVisualLayerCollection>();

            preferenceCellData.SchedulePart = schedulePart;

            preferenceCellData.SchedulingOption = new RestrictionSchedulingOptions {UseScheduling = true};
            _detailData.Add(0, preferenceCellData);
            preferenceCellData.WeeklyMax = TimeSpan.FromHours(10);
            preferenceCellData.WeeklyMin = TimeSpan.FromHours(9);

            using (_mocks.Record())
            {
                Expect.Call(_model.DetailData()).Return(_detailData).Repeat.AtLeastOnce();

                Expect.Call(schedulePart.ProjectionService()).Return(projectionService).Repeat.AtLeastOnce();
                Expect.Call(projectionService.CreateProjection()).Return(visualLayerCollection).Repeat.AtLeastOnce();
                Expect.Call(visualLayerCollection.HasLayers).Return(false).Repeat.AtLeastOnce(); // this line indicates that the day is not scheduled
                Expect.Call(_schedulerStateHolder.SchedulingResultState).Return(_schedulingResultStateHolder).Repeat.AtLeastOnce();
                Expect.Call(_schedulingResultStateHolder.UseMinWeekWorkTime).Return(true).Repeat.AtLeastOnce();
            }

            using (_mocks.Playback())
            {
                //because of constructor in SchedulePresenterBase
                var period = _schedulerStateHolder.RequestedPeriod;
                var weekHeaderCellData = _presenter.OnQueryWeekHeader(1);
                Assert.IsNotNull(weekHeaderCellData);
                Assert.IsNotNull(weekHeaderCellData);
                Assert.IsTrue(weekHeaderCellData.Alert);
            }
        }

        [Test]
        public void ShouldAlertWhenAboveMax()
        {
            var preferenceCellData = new PreferenceCellData();
            var startTimeLimitation = new StartTimeLimitation(new TimeSpan(7, 0, 0), null);
            var endTimeLimitation = new EndTimeLimitation(new TimeSpan(0, 20, 0), null);
            var workTimeLimitation = new WorkTimeLimitation(new TimeSpan(8, 0, 0), new TimeSpan(8, 0, 0));
            var shiftCategory = ShiftCategoryFactory.CreateShiftCategory("Natt");
            var effectiveRestriction = new EffectiveRestriction(startTimeLimitation, endTimeLimitation, workTimeLimitation, shiftCategory, null, null, new List<IActivityRestriction>());
            preferenceCellData.EffectiveRestriction = effectiveRestriction;

            var schedulePart = _mocks.StrictMock<IScheduleDay>();
            var projectionService = _mocks.StrictMock<IProjectionService>();
            var visualLayerCollection = _mocks.StrictMock<IVisualLayerCollection>();

            preferenceCellData.SchedulePart = schedulePart;

            preferenceCellData.SchedulingOption = new RestrictionSchedulingOptions { UseScheduling = true };
            _detailData.Add(0, preferenceCellData);
            preferenceCellData.WeeklyMax = TimeSpan.FromHours(7);
            preferenceCellData.WeeklyMin = TimeSpan.FromHours(6);

            using (_mocks.Record())
            {
                Expect.Call(_model.DetailData()).Return(_detailData).Repeat.AtLeastOnce();
                Expect.Call(schedulePart.ProjectionService()).Return(projectionService).Repeat.AtLeastOnce();
                Expect.Call(projectionService.CreateProjection()).Return(visualLayerCollection).Repeat.AtLeastOnce();
                Expect.Call(visualLayerCollection.HasLayers).Return(false).Repeat.AtLeastOnce(); // this line indicates that the day is not scheduled
            }

            using (_mocks.Playback())
            {
                //because of constructor in SchedulePresenterBase
                var period = _schedulerStateHolder.RequestedPeriod;
                var weekHeaderCellData = _presenter.OnQueryWeekHeader(1);
                Assert.IsNotNull(weekHeaderCellData);
                Assert.IsTrue(weekHeaderCellData.Alert);
            }
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
