using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.UserTexts;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling;


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
		private IAgentPreferenceDayCreator _dayCreator;
		private ISchedulingResultStateHolder _schedulingResultStateHolder;
		private IDateOnlyAsDateTimePeriod _dateOnlyAsDateTimePeriod;
		private IScheduleDictionary _scheduleDictionary;
		private IScheduleRange _scheduleRange;
		private IVirtualSchedulePeriod _virtualSchedulePeriod;
		private DateOnlyPeriod _dateOnlyPeriod;

		[SetUp]
		public void Setup()
		{
			_mock = new MockRepository();
			_view = _mock.StrictMock<IAgentPreferenceView>();
			_scheduleDay = _mock.StrictMock<IScheduleDay>();
			_schedulingResultStateHolder = _mock.DynamicMock<ISchedulingResultStateHolder>();
			_presenter = new AgentPreferencePresenter(_view, _scheduleDay, _schedulingResultStateHolder, new DoNothingScheduleDayChangeCallBack());
			_person = _mock.StrictMock<IPerson>();
			_dateOnly = new DateOnly(2013,1,1);
			_preferenceRestriction = new PreferenceRestriction {MustHave = true};
			_preferenceDay = new PreferenceDay(_person, _dateOnly, _preferenceRestriction);
			_dayCreator = _mock.StrictMock<IAgentPreferenceDayCreator>();
			_dateOnlyAsDateTimePeriod = _mock.StrictMock<IDateOnlyAsDateTimePeriod>();
			_scheduleDictionary = _mock.StrictMock<IScheduleDictionary>();
			_scheduleRange = _mock.StrictMock<IScheduleRange>();
			_virtualSchedulePeriod = _mock.StrictMock<IVirtualSchedulePeriod>();
			_dateOnlyPeriod = new DateOnlyPeriod(_dateOnly, _dateOnly);
		}

		[Test]
		public void ShouldInitializePresenter()
		{
			Assert.AreEqual(_view, _presenter.View);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Teleopti.Ccc.WinCode.Scheduling.IAgentPreferenceView.UpdateMustHaveText(System.String)"), Test]
		public void ShouldUpdateView()
		{
			using (_mock.Record())
			{
				updateViewMocks();
			}

			using (_mock.Playback())
			{
				_presenter.UpdateView();
			}
		}

		private void updateViewMocks()
		{
			Expect.Call(_scheduleDay.PersistableScheduleDataCollection())
			      .Return(new ReadOnlyCollection<IPersistableScheduleData>(new List<IPersistableScheduleData> {_preferenceDay}));
			Expect.Call(() => _view.UpdateTimesExtended(null, null, null, null, null, null));
			Expect.Call(() => _view.PopulateShiftCategories());
			Expect.Call(() => _view.PopulateAbsences());
			Expect.Call(() => _view.PopulateDayOffs());
			Expect.Call(() => _view.PopulateActivities());
			Expect.Call(_scheduleDay.Person).Return(_person).Repeat.AtLeastOnce();
			Expect.Call(_scheduleDay.DateOnlyAsPeriod).Return(_dateOnlyAsDateTimePeriod).Repeat.AtLeastOnce();
			Expect.Call(_dateOnlyAsDateTimePeriod.DateOnly).Return(_dateOnly).Repeat.AtLeastOnce();
			Expect.Call(_schedulingResultStateHolder.Schedules).Return(_scheduleDictionary);
            Expect.Call(_scheduleDictionary[_person]).Return(_scheduleRange);
            Expect.Call(_scheduleRange.ScheduledDayCollection(_dateOnlyPeriod)).Return(new[] { _scheduleDay });
			Expect.Call(_scheduleDay.RestrictionCollection()).Return(new List<IRestrictionBase> {_preferenceRestriction});
			Expect.Call(_person.VirtualSchedulePeriod(_dateOnly)).Return(_virtualSchedulePeriod).Repeat.AtLeastOnce();
			Expect.Call(_virtualSchedulePeriod.DateOnlyPeriod).Return(_dateOnlyPeriod);
				Expect.Call(_virtualSchedulePeriod.IsValid).Return(true);
			Expect.Call(_virtualSchedulePeriod.MustHavePreference).Return(1);
			Expect.Call(() => _view.UpdateMustHaveText(Resources.MustHave + " (1/1)"));
			Expect.Call(() => _view.UpdateMustHave(true));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Teleopti.Ccc.WinCode.Scheduling.IAgentPreferenceView.UpdateMustHaveText(System.String)"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
		public void ShouldUpdateAndClearDayOffAndAbsenceWhenShiftCategory()
		{
			var shiftCategory = new ShiftCategory("shiftCategory");
			_preferenceDay.Restriction.ShiftCategory = shiftCategory;

			using (_mock.Record())
			{
				Expect.Call(_scheduleDay.PersistableScheduleDataCollection()).Return(new ReadOnlyCollection<IPersistableScheduleData>(new List<IPersistableScheduleData> { _preferenceDay }));
				Expect.Call(() => _view.UpdateShiftCategory(shiftCategory));
				Expect.Call(() => _view.ClearAbsence());
				Expect.Call(() => _view.ClearDayOff());
				Expect.Call(() => _view.UpdateTimesExtended(null, null, null, null, null, null));
				Expect.Call(() => _view.PopulateShiftCategories());
				Expect.Call(() => _view.PopulateAbsences());
				Expect.Call(() => _view.PopulateDayOffs());
				Expect.Call(() => _view.PopulateActivities());

				Expect.Call(_scheduleDay.Person).Return(_person).Repeat.AtLeastOnce();
				Expect.Call(_scheduleDay.DateOnlyAsPeriod).Return(_dateOnlyAsDateTimePeriod).Repeat.AtLeastOnce();
				Expect.Call(_dateOnlyAsDateTimePeriod.DateOnly).Return(_dateOnly).Repeat.AtLeastOnce();
				Expect.Call(_schedulingResultStateHolder.Schedules).Return(_scheduleDictionary);
                Expect.Call(_scheduleDictionary[_person]).Return(_scheduleRange);
                Expect.Call(_scheduleRange.ScheduledDayCollection(_dateOnlyPeriod)).Return(new[] { _scheduleDay });
				Expect.Call(_scheduleDay.RestrictionCollection()).Return(new List<IRestrictionBase> { _preferenceRestriction });
				Expect.Call(_person.VirtualSchedulePeriod(_dateOnly)).Return(_virtualSchedulePeriod).Repeat.AtLeastOnce();
				Expect.Call(_virtualSchedulePeriod.DateOnlyPeriod).Return(_dateOnlyPeriod);
				Expect.Call(_virtualSchedulePeriod.IsValid).Return(true);
				Expect.Call(_virtualSchedulePeriod.MustHavePreference).Return(1);
				Expect.Call(() => _view.UpdateMustHaveText(Resources.MustHave + " (1/1)"));
				Expect.Call(() => _view.UpdateMustHave(true));
			}

			using (_mock.Playback())
			{
				_presenter.UpdateView();
			}	
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Teleopti.Ccc.WinCode.Scheduling.IAgentPreferenceView.UpdateMustHaveText(System.String)"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity"), Test]
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
				Expect.Call(() => _view.PopulateShiftCategories());
				Expect.Call(() => _view.PopulateAbsences());
				Expect.Call(() => _view.PopulateDayOffs());
				Expect.Call(() => _view.PopulateActivities());

				Expect.Call(_scheduleDay.Person).Return(_person).Repeat.AtLeastOnce();
				Expect.Call(_scheduleDay.DateOnlyAsPeriod).Return(_dateOnlyAsDateTimePeriod).Repeat.AtLeastOnce();
				Expect.Call(_dateOnlyAsDateTimePeriod.DateOnly).Return(_dateOnly).Repeat.AtLeastOnce();
				Expect.Call(_schedulingResultStateHolder.Schedules).Return(_scheduleDictionary);
                Expect.Call(_scheduleDictionary[_person]).Return(_scheduleRange);
                Expect.Call(_scheduleRange.ScheduledDayCollection(_dateOnlyPeriod)).Return(new[] { _scheduleDay });
				Expect.Call(_scheduleDay.RestrictionCollection()).Return(new List<IRestrictionBase> { _preferenceRestriction });
				Expect.Call(_person.VirtualSchedulePeriod(_dateOnly)).Return(_virtualSchedulePeriod).Repeat.AtLeastOnce();
				Expect.Call(_virtualSchedulePeriod.DateOnlyPeriod).Return(_dateOnlyPeriod);
				Expect.Call(_virtualSchedulePeriod.IsValid).Return(true);
				Expect.Call(_virtualSchedulePeriod.MustHavePreference).Return(1);
				Expect.Call(() => _view.UpdateMustHaveText(Resources.MustHave + " (1/1)"));
				Expect.Call(() => _view.UpdateMustHave(true));
			}

			using (_mock.Playback())
			{
				_presenter.UpdateView();
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Teleopti.Ccc.WinCode.Scheduling.IAgentPreferenceView.UpdateMustHaveText(System.String)"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity"), Test]
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
				Expect.Call(() => _view.PopulateShiftCategories());
				Expect.Call(() => _view.PopulateAbsences());
				Expect.Call(() => _view.PopulateDayOffs());
				Expect.Call(() => _view.PopulateActivities());

				Expect.Call(_scheduleDay.Person).Return(_person).Repeat.AtLeastOnce();
				Expect.Call(_scheduleDay.DateOnlyAsPeriod).Return(_dateOnlyAsDateTimePeriod).Repeat.AtLeastOnce();
				Expect.Call(_dateOnlyAsDateTimePeriod.DateOnly).Return(_dateOnly).Repeat.AtLeastOnce();
				Expect.Call(_schedulingResultStateHolder.Schedules).Return(_scheduleDictionary);
                Expect.Call(_scheduleDictionary[_person]).Return(_scheduleRange);
                Expect.Call(_scheduleRange.ScheduledDayCollection(_dateOnlyPeriod)).Return(new[] { _scheduleDay });
				Expect.Call(_scheduleDay.RestrictionCollection()).Return(new List<IRestrictionBase> { _preferenceRestriction });
				Expect.Call(_person.VirtualSchedulePeriod(_dateOnly)).Return(_virtualSchedulePeriod).Repeat.AtLeastOnce();
				Expect.Call(_virtualSchedulePeriod.DateOnlyPeriod).Return(_dateOnlyPeriod);
				Expect.Call(_virtualSchedulePeriod.IsValid).Return(true);
				Expect.Call(_virtualSchedulePeriod.MustHavePreference).Return(1);
				Expect.Call(() => _view.UpdateMustHaveText(Resources.MustHave + " (1/1)"));
				Expect.Call(() => _view.UpdateMustHave(true));
			}

			using (_mock.Playback())
			{
				_presenter.UpdateView();
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Teleopti.Ccc.WinCode.Scheduling.IAgentPreferenceView.UpdateMustHaveText(System.String)"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
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
				Expect.Call(() => _view.PopulateShiftCategories());
				Expect.Call(() => _view.PopulateAbsences());
				Expect.Call(() => _view.PopulateDayOffs());
				Expect.Call(() => _view.PopulateActivities());

				Expect.Call(_scheduleDay.Person).Return(_person).Repeat.AtLeastOnce();
				Expect.Call(_scheduleDay.DateOnlyAsPeriod).Return(_dateOnlyAsDateTimePeriod).Repeat.AtLeastOnce();
				Expect.Call(_dateOnlyAsDateTimePeriod.DateOnly).Return(_dateOnly).Repeat.AtLeastOnce();
				Expect.Call(_schedulingResultStateHolder.Schedules).Return(_scheduleDictionary);
                Expect.Call(_scheduleDictionary[_person]).Return(_scheduleRange);
                Expect.Call(_scheduleRange.ScheduledDayCollection(_dateOnlyPeriod)).Return(new[] { _scheduleDay });
				Expect.Call(_scheduleDay.RestrictionCollection()).Return(new List<IRestrictionBase> { _preferenceRestriction });
				Expect.Call(_person.VirtualSchedulePeriod(_dateOnly)).Return(_virtualSchedulePeriod).Repeat.AtLeastOnce();
				Expect.Call(_virtualSchedulePeriod.DateOnlyPeriod).Return(_dateOnlyPeriod);
				Expect.Call(_virtualSchedulePeriod.IsValid).Return(true);
				Expect.Call(_virtualSchedulePeriod.MustHavePreference).Return(1);
				Expect.Call(() => _view.UpdateMustHaveText(Resources.MustHave + " (1/1)"));
				Expect.Call(() => _view.UpdateMustHave(true));
			}

			using (_mock.Playback())
			{
				_presenter.UpdateView();
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Teleopti.Ccc.WinCode.Scheduling.IAgentPreferenceView.UpdateMustHaveText(System.String)"), Test]
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

				Expect.Call(_scheduleDay.Person).Return(_person).Repeat.AtLeastOnce();
				Expect.Call(_scheduleDay.DateOnlyAsPeriod).Return(_dateOnlyAsDateTimePeriod).Repeat.AtLeastOnce();
				Expect.Call(_dateOnlyAsDateTimePeriod.DateOnly).Return(_dateOnly).Repeat.AtLeastOnce();
				Expect.Call(_schedulingResultStateHolder.Schedules).Return(_scheduleDictionary);
				Expect.Call(_scheduleDictionary[_person]).Return(_scheduleRange);
				Expect.Call(_scheduleRange.ScheduledDayCollection(_dateOnlyPeriod)).Return(new[]{_scheduleDay});
				Expect.Call(_scheduleDay.RestrictionCollection()).Return(new List<IRestrictionBase> { _preferenceRestriction });
				Expect.Call(_person.VirtualSchedulePeriod(_dateOnly)).Return(_virtualSchedulePeriod).Repeat.AtLeastOnce();
				Expect.Call(_virtualSchedulePeriod.DateOnlyPeriod).Return(_dateOnlyPeriod);
				Expect.Call(_virtualSchedulePeriod.IsValid).Return(true);
				Expect.Call(_virtualSchedulePeriod.MustHavePreference).Return(1);
				Expect.Call(() => _view.UpdateMustHaveText(Resources.MustHave + " (1/1)"));
			}

			using (_mock.Playback())
			{
				_presenter.UpdateView();
			}	
		}

		[Test]
		public void ShouldRemoveWhenExistingAndAllEmpty()
		{
			var result = new AgentPreferenceCanCreateResult {Result = false, EmptyError = true};

			var data = new AgentPreferenceData();
			

			using (_mock.Record())
			{
				Expect.Call(_scheduleDay.PersistableScheduleDataCollection()).Return(new ReadOnlyCollection<IPersistableScheduleData>(new List<IPersistableScheduleData> { _preferenceDay }));
				Expect.Call(_dayCreator.CanCreate(data)).Return(result);
			}

			using (_mock.Playback())
			{
				var toExecute = _presenter.CommandToExecute(data, _dayCreator);
				Assert.IsInstanceOf<AgentPreferenceRemoveCommand>(toExecute);
			}
		}

		[Test]
		public void ShouldAddWhenNoExisting()
		{
			var result = new AgentPreferenceCanCreateResult {Result = true};
			var shiftCategory = new ShiftCategory("shiftCategory");

			var data = new AgentPreferenceData
			{
				ShiftCategory = shiftCategory
			};

			using (_mock.Record())
			{
				Expect.Call(_scheduleDay.PersistableScheduleDataCollection()).Return(new ReadOnlyCollection<IPersistableScheduleData>(new List<IPersistableScheduleData> ()));
				Expect.Call(_dayCreator.CanCreate(data)).Return(result);
			}

			using (_mock.Playback())
			{
				var toExecute = _presenter.CommandToExecute(data, _dayCreator);
				Assert.IsInstanceOf<AgentPreferenceAddCommand>(toExecute);
			}
		}

		[Test]
		public void ShouldEditWhenExisting()
		{
			var result = new AgentPreferenceCanCreateResult {Result = true};
			var shiftCategory = new ShiftCategory("shiftCategory");

			var data = new AgentPreferenceData
			{
				ShiftCategory = shiftCategory
			};

			using (_mock.Record())
			{
				Expect.Call(_scheduleDay.PersistableScheduleDataCollection()).Return(new ReadOnlyCollection<IPersistableScheduleData>(new List<IPersistableScheduleData>{_preferenceDay}));
				Expect.Call(_dayCreator.CanCreate(data)).Return(result);
			}

			using (_mock.Playback())
			{
				var toExecute = _presenter.CommandToExecute(data, _dayCreator);
				Assert.IsInstanceOf<AgentPreferenceEditCommand>(toExecute);
			}
		}

		[Test]
		public void ShouldNoneWhenNotValidData()
		{
			var result = new AgentPreferenceCanCreateResult {Result = false, StartTimeMinError = true, StartTimeMaxError = true};

			var data = new AgentPreferenceData
			{
				MinStart = TimeSpan.FromHours(2),
				MaxStart = TimeSpan.FromHours(1)
			};

			using (_mock.Record())
			{
				Expect.Call(_scheduleDay.PersistableScheduleDataCollection()).Return(new ReadOnlyCollection<IPersistableScheduleData>(new List<IPersistableScheduleData>()));
				Expect.Call(_dayCreator.CanCreate(data)).Return(result);
			}

			using (_mock.Playback())
			{
				var toExecute = _presenter.CommandToExecute(data, _dayCreator);
				Assert.IsNull(toExecute);
			}
		}

		[Test]
		public void ShouldGetPreferenceRestriction()
		{
			using (_mock.Record())
			{
				Expect.Call(_scheduleDay.PersistableScheduleDataCollection()).Return(new ReadOnlyCollection<IPersistableScheduleData>(new List<IPersistableScheduleData> { _preferenceDay }));
			}

			using (_mock.Playback())
			{
				var restriction = _presenter.PreferenceRestriction();
				Assert.AreEqual(_preferenceRestriction, restriction);
			}
		}

		[Test]
		public void ShouldHandleNoSchedulePeriodOnMaxMustHave()
		{
			using (_mock.Record())
			{
				Expect.Call(_scheduleDay.Person).Return(_person);
				Expect.Call(_scheduleDay.DateOnlyAsPeriod).Return(_dateOnlyAsDateTimePeriod);
				Expect.Call(_dateOnlyAsDateTimePeriod.DateOnly).Return(_dateOnly);
				Expect.Call(_person.VirtualSchedulePeriod(_dateOnly)).Return(_virtualSchedulePeriod);
				Expect.Call(_virtualSchedulePeriod.IsValid).Return(false);
			}
			
			using (_mock.Playback())
			{
				var result = _presenter.MaxMustHaves();
				Assert.AreEqual(0, result);
			}
		}
		
		[Test]
		public void ShouldRunCommand()
		{
			var command = _mock.StrictMock<IExecutableCommand>();
			using (_mock.Record())
			{
				Expect.Call(command.Execute);
				updateViewMocks();
			}

			using (_mock.Playback())
			{
				_presenter.RunCommand(command);
			}
		}
	}
}
