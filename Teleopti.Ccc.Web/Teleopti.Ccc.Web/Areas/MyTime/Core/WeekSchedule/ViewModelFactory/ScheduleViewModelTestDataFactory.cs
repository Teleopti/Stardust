using System;
using System.Collections.Generic;
using Teleopti.Ccc.Web.Areas.MyTime.Models.MonthSchedule;
using Teleopti.Ccc.Web.Areas.MyTime.Models.WeekSchedule;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.ViewModelFactory
{
	public class ScheduleViewModelTestDataFactory : IScheduleViewModelFactory
	{
		private readonly IList<Func<DateOnly, DayViewModel>> _dayTypeList;
		private readonly IList<Func<PeriodViewModel>> _actCatTypeList;
		private readonly List<Func<NoteViewModel>> _notesTypeList;

		public ScheduleViewModelTestDataFactory()
		{
			_dayTypeList = new List<Func<DateOnly, DayViewModel>>
									{
										createNormalWeekDayModel,
										createDayOffWeekDayModel,
										createShortWeekDayModel,
										createNormalWeekDayModel
									};

			_actCatTypeList = new List<Func<PeriodViewModel>>
										{
											createPhone,
											createBreak,
											createMeeting,
											createLunch,
											createActivityWithLongName
										};

			_notesTypeList = new List<Func<NoteViewModel>>
									{
										createEmptyNote,
										createEmptyNote,
										createEmptyNote,
										createEmptyNote,
										createLongNote,
										createShortNote
									};
		}

		

		private static NoteViewModel createEmptyNote()
		{
			return new NoteViewModel();
		}

		private static NoteViewModel createLongNote()
		{
			return new NoteViewModel
						{
							Message =
								"Suspendisse vehicula, augue a eleifend tincidunt, dui ligula vulputate enim, vitae lobortis eros nulla ut augue. Quisque a suscipit elit. Sed sed turpis lectus. Etiam sed nulla lacus. Fusce et felis sed neque ornare posuere cursus et ipsum. Aenean elementum orci eget nibh adipiscing fermentum feugiat mauris vulputate. Integer luctus risus eu quam vulputate vulputate. Morbi ut lobortis elit. Aenean eget justo orci, vitae cursus mauris. Ut quis mi id magna semper tincidunt. Pellentesque molestie vestibulum adipiscing.",
						};
		}

		private static NoteViewModel createShortNote()
		{
			return new NoteViewModel
						{
							Message =
								"Suspendisse vehicula, augue a eleifend tincidunt, dui ligula vulputate enim, vitae lobortis",
						};
		}

		private static PeriodViewModel createPhone()
		{
			return new PeriodViewModel {Summary = "01:30", TimeSpan = "08:00 - 09:30", Title = "Phone", StyleClassName = "phone_id"};
		}

		private static PeriodViewModel createActivityWithLongName()
		{
			return new PeriodViewModel
						{
							Summary = "01:30",
							TimeSpan = "08:00 - 09:30",
							Title = "The quick brown fox jumps over the lazy dog",
							StyleClassName = "long_id"
						};
		}


		private static PeriodViewModel createBreak()
		{
			return new PeriodViewModel {Summary = "00:15", TimeSpan = "09:30 - 09:45", Title = "Break", StyleClassName = "break_id"};
		}

		private static PeriodViewModel createMeeting()
		{
			return new PeriodViewModel { Summary = "00:15", TimeSpan = "10:30 - 10:45", Title = "Meeting", StyleClassName = "meeting_id", Meeting = new MeetingViewModel { Location = "Room 12249382.3", Title = "augue a eleifend tincidunt" } };

		}

		private static PeriodViewModel createLunch()
		{
			return new PeriodViewModel {Summary = "00:15", TimeSpan = "10:30 - 10:45", Title = "Lunch", StyleClassName = "meeting_id"};
		}

		public WeekScheduleViewModel CreateWeekViewModel(DateOnly dateOnly)
		{
			return createWeekDayModels(dateOnly, 7);
		}

        public MonthScheduleViewModel CreateMonthViewModel(DateOnly dateOnly)
        {
            return new MonthScheduleViewModel();
        }

		private WeekScheduleViewModel createWeekDayModels(DateOnly dateOnly, int count)
		{
			var firstDate = findFirstDateOfWeek(dateOnly);

			var dayList = new List<DayViewModel>(count);
			for (var i = 0; i < count; i++)
			{
				dayList.Add(_dayTypeList[i % _dayTypeList.Count](firstDate));
				firstDate = firstDate.AddDays(1);
			}

			return new WeekScheduleViewModel {Days = dayList};
		}

		private static DateOnly findFirstDateOfWeek(DateOnly dateOnly)
		{
			// Test only
			return dateOnly.AddDays(-1*((int) dateOnly.DayOfWeek) + 1);
		}


		private static PeriodViewModel createLatePeriod()
		{
			return new PeriodViewModel {Summary = "08:00", TimeSpan = "12:30 - 21:30", Title = "Late", StyleClassName = "late_id"};
		}

		private DayViewModel createShortWeekDayModel(DateOnly dateOnly)
		{
			var headerViewModel = createHeaderViewModel(dateOnly);
			var summaryViewModel = createLatePeriod();
			var periodViewModels = new List<PeriodViewModel>();
			var noteViewModel = _notesTypeList[dateOnly.Day % _notesTypeList.Count]();

			for (int i = 0; i < 5; i++)
			{
				periodViewModels.Add(_actCatTypeList[i % _actCatTypeList.Count]());
			}


			return new DayViewModel
			{
				Summary = summaryViewModel,
				Header = headerViewModel,
				Note = noteViewModel,
				Periods =
					periodViewModels
			};
		}

		private DayViewModel createNormalWeekDayModel(DateOnly dateOnly)
		{
			var headerViewModel = createHeaderViewModel(dateOnly);
			var summaryViewModel = createLatePeriod();
			var periodViewModels = new List<PeriodViewModel>();
			var noteViewModel = _notesTypeList[dateOnly.Day % _notesTypeList.Count]();

			for (int i = 0; i < 10; i++)
			{
				periodViewModels.Add(_actCatTypeList[i % _actCatTypeList.Count]());
			}
			

			return new DayViewModel
						{
							Summary = summaryViewModel,
							Header = headerViewModel,
							Note = noteViewModel,
							Periods =
								periodViewModels
						};
		}

		private DayViewModel createDayOffWeekDayModel(DateOnly dateOnly)
		{
			var headerViewModel = createHeaderViewModel(dateOnly);
			return new DayViewModel
						{
							Summary =
								new PeriodViewModel {Summary = "", TimeSpan = "", Title = "Day Off", StyleClassName = "do"}
							,
							Header = headerViewModel,
							Note =
								new NoteViewModel(),
							Periods =
								new List<PeriodViewModel>
								()
						};
		}

		private HeaderViewModel createHeaderViewModel(DateOnly dateOnly)
		{
			 var swedishCulture = System.Globalization.CultureInfo.GetCultureInfo(1053);
			return new HeaderViewModel
						{
							Title = (dateOnly.Date).ToString("dddd",swedishCulture),
								Date = dateOnly.ToShortDateString(swedishCulture),
								DayDescription = dateOnly.Day == 1 ? dateOnly.Date.ToString("MMM", swedishCulture) : String.Empty,
								DayNumber = dateOnly.Day.ToString(swedishCulture)
						};
		}
	}
}