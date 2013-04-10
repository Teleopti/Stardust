using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.WinCode.Scheduling;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Scheduler
{
	[TestFixture]
	public class AgentPreferencePresenterTest
	{
		private AgentPreferencePresenter _presenter;
		private IAgentPreferenceView _view;
		private MockRepository _mock;
		private IScheduleDay _scheduleDay;
		private IPreferenceDay _preferenceDay;
		private IPerson _person;
		private DateOnly _dateOnly;
		private IPreferenceRestriction _preferenceRestriction;

		[SetUp]
		public void Setup()
		{
			_mock = new MockRepository();
			_view = _mock.StrictMock<IAgentPreferenceView>();
			_scheduleDay = _mock.StrictMock<IScheduleDay>();
			_presenter = new AgentPreferencePresenter(_view, _scheduleDay);
			_person = new Person();
			_dateOnly = new DateOnly(2013,1,1);
			_preferenceRestriction = new PreferenceRestriction();
			_preferenceDay = new PreferenceDay(_person, _dateOnly, _preferenceRestriction);

		}

		[Test]
		public void ShouldInitializePresenter()
		{
			Assert.AreEqual(_view, _presenter.View);
			Assert.AreEqual(_scheduleDay, _presenter.ScheduleDay);
		}

		[Test]
		public void ShouldUpdateView()
		{
			using (_mock.Record())
			{
				Expect.Call(_scheduleDay.PersistableScheduleDataCollection()).Return(new ReadOnlyCollection<IPersistableScheduleData>(new List<IPersistableScheduleData> { _preferenceDay }));
				Expect.Call(() => _view.UpdateTimesExtended(null, null, null, null, null, null));
				Expect.Call(() => _view.UpdateMustHave(false));
				Expect.Call(() => _view.PopulateShiftCategories());
				Expect.Call(() => _view.PopulateAbsences());
				Expect.Call(() => _view.PopulateDayOffs());
				Expect.Call(() => _view.PopulateActivities());
			}

			using (_mock.Playback())
			{
				_presenter.UpdateView();
			}
		}

		[Test]
		public void ShouldUpdateAndClearDayOffAndAbsenceWhenShiftCategory()
		{
			var shiftCategory = new ShiftCategory("shiftCategory");
			_preferenceDay.Restriction.ShiftCategory = shiftCategory;

			using (_mock.Record())
			{
				Expect.Call(_scheduleDay.PersistableScheduleDataCollection()).Return(new ReadOnlyCollection<IPersistableScheduleData>(new List<IPersistableScheduleData> { _preferenceDay }));
				Expect.Call(() => _view.UpdateShiftCategory(shiftCategory));
				Expect.Call(() => _view.UpdateShiftCategoryExtended(shiftCategory));
				Expect.Call(() => _view.ClearAbsence());
				Expect.Call(() => _view.ClearDayOff());
				Expect.Call(() => _view.UpdateTimesExtended(null, null, null, null, null, null));
				Expect.Call(() => _view.UpdateMustHave(false));
				Expect.Call(() => _view.PopulateShiftCategories());
				Expect.Call(() => _view.PopulateAbsences());
				Expect.Call(() => _view.PopulateDayOffs());
				Expect.Call(() => _view.PopulateActivities());
			}

			using (_mock.Playback())
			{
				_presenter.UpdateView();
			}	
		}

		[Test]
		public void ShouldUpdateAndClearAllElseWhenAbsence()
		{
			var absence = new Absence();
			_preferenceDay.Restriction.Absence = absence;

			using (_mock.Record())
			{
				Expect.Call(_scheduleDay.PersistableScheduleDataCollection()).Return(new ReadOnlyCollection<IPersistableScheduleData>(new List<IPersistableScheduleData> { _preferenceDay }));
				Expect.Call(() => _view.UpdateAbsence(absence));
				Expect.Call(() => _view.ClearShiftCategory());
				Expect.Call(() => _view.ClearShiftCategoryExtended());
				Expect.Call(() => _view.ClearDayOff());
				Expect.Call(() => _view.ClearActivity());
				Expect.Call(() => _view.UpdateTimesExtended(null, null, null, null, null, null));
				Expect.Call(() => _view.UpdateMustHave(false));
				Expect.Call(() => _view.PopulateShiftCategories());
				Expect.Call(() => _view.PopulateAbsences());
				Expect.Call(() => _view.PopulateDayOffs());
				Expect.Call(() => _view.PopulateActivities());
			}

			using (_mock.Playback())
			{
				_presenter.UpdateView();
			}
		}

		[Test]
		public void ShouldUpdateAndClearAllElseWhenDayOff()
		{
			var dayOffTemplate = new DayOffTemplate(new Description("dayOff"));
			_preferenceDay.Restriction.DayOffTemplate = dayOffTemplate;

			using (_mock.Record())
			{
				Expect.Call(_scheduleDay.PersistableScheduleDataCollection()).Return(new ReadOnlyCollection<IPersistableScheduleData>(new List<IPersistableScheduleData> { _preferenceDay }));
				Expect.Call(() => _view.UpdateDayOff(dayOffTemplate));
				Expect.Call(() => _view.ClearShiftCategory());
				Expect.Call(() => _view.ClearShiftCategoryExtended());
				Expect.Call(() => _view.ClearAbsence());
				Expect.Call(() => _view.ClearActivity());
				Expect.Call(() => _view.UpdateTimesExtended(null, null, null, null, null, null));
				Expect.Call(() => _view.UpdateMustHave(false));
				Expect.Call(() => _view.PopulateShiftCategories());
				Expect.Call(() => _view.PopulateAbsences());
				Expect.Call(() => _view.PopulateDayOffs());
				Expect.Call(() => _view.PopulateActivities());
			}

			using (_mock.Playback())
			{
				_presenter.UpdateView();
			}
		}

		[Test]
		public void ShouldClearDayOffAndAbsenceWhenActivity()
		{
			var activity = new Activity("activity");
			var activityRestriction = new ActivityRestriction(activity);
			_preferenceDay.Restriction.AddActivityRestriction(activityRestriction);

			using (_mock.Record())
			{
				Expect.Call(_scheduleDay.PersistableScheduleDataCollection()).Return(new ReadOnlyCollection<IPersistableScheduleData>(new List<IPersistableScheduleData> { _preferenceDay }));
				Expect.Call(() => _view.UpdateActivity(activity));
				Expect.Call(() => _view.ClearAbsence());
				Expect.Call(() => _view.ClearDayOff());
				Expect.Call(() => _view.UpdateActivityTimes(null, null, null, null, null, null));
				Expect.Call(() => _view.UpdateTimesExtended(null, null, null, null, null, null));
				Expect.Call(() => _view.UpdateMustHave(false));
				Expect.Call(() => _view.PopulateShiftCategories());
				Expect.Call(() => _view.PopulateAbsences());
				Expect.Call(() => _view.PopulateDayOffs());
				Expect.Call(() => _view.PopulateActivities());
			}

			using (_mock.Playback())
			{
				_presenter.UpdateView();
			}
		}

		[Test]
		public void ShouldClearAllWhenNoRestriction()
		{
			using (_mock.Record())
			{
				Expect.Call(_scheduleDay.PersistableScheduleDataCollection()).Return(new ReadOnlyCollection<IPersistableScheduleData>(new List<IPersistableScheduleData>()));
				Expect.Call(() => _view.ClearShiftCategory());
				Expect.Call(() => _view.ClearShiftCategoryExtended());
				Expect.Call(() => _view.ClearAbsence());
				Expect.Call(() => _view.ClearDayOff());
				Expect.Call(() => _view.ClearActivity());
				Expect.Call(() => _view.PopulateShiftCategories());
				Expect.Call(() => _view.PopulateAbsences());
				Expect.Call(() => _view.PopulateDayOffs());
				Expect.Call(() => _view.PopulateActivities());
			}

			using (_mock.Playback())
			{
				_presenter.UpdateView();
			}	
		}
	}
}
