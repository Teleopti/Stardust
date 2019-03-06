using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.StudentAvailability.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Models.StudentAvailability;
using Teleopti.Ccc.Web.Areas.SystemSetting.BankHolidayCalendar.Core.DataProvider;


namespace Teleopti.Ccc.Web.Areas.MyTime.Core.StudentAvailability.ViewModelFactory
{
	public class StudentAvailabilityViewModelFactory : IStudentAvailabilityViewModelFactory
	{
		private readonly StudentAvailabilityViewModelMapper _mapper;
		private readonly StudentAvailabilityDayFeedbackViewModelMapper _feedbackMapper;
		private readonly StudentAvailabilityDayViewModelMapper _dayMapper;
		private readonly IStudentAvailabilityProvider _studentAvailabilityProvider;
		private readonly IBankHolidayCalendarProvider _bankHolidayCalendarProvider;

		public StudentAvailabilityViewModelFactory(StudentAvailabilityViewModelMapper mapper, StudentAvailabilityDayFeedbackViewModelMapper feedbackMapper, 
			StudentAvailabilityDayViewModelMapper dayMapper, IStudentAvailabilityProvider studentAvailabilityProvider, 
			IBankHolidayCalendarProvider bankHolidayCalendarProvider)
		{
			_mapper = mapper;
			_feedbackMapper = feedbackMapper;
			_dayMapper = dayMapper;
			_studentAvailabilityProvider = studentAvailabilityProvider;
			_bankHolidayCalendarProvider = bankHolidayCalendarProvider;
		}

		public StudentAvailabilityViewModel CreateViewModel(DateOnly dateInPeriod)
		{
			return _mapper.Map(dateInPeriod);
		}

		public StudentAvailabilityDayFeedbackViewModel CreateDayFeedbackViewModel(DateOnly date)
		{
			return _feedbackMapper.Map(date);
		}

		public StudentAvailabilityDayViewModel CreateDayViewModel(DateOnly date)
		{
			var studentAvailability = _studentAvailabilityProvider.GetStudentAvailabilityDayForDate(date);
			if (studentAvailability == null) return null;

			var calendarDate = _bankHolidayCalendarProvider.GetMySiteBankHolidayDates(date.ToDateOnlyPeriod()).ToList();
			return _dayMapper.Map(studentAvailability, calendarDate.FirstOrDefault());
		}

		public IEnumerable<StudentAvailabilityDayViewModel> CreateStudentAvailabilityAndSchedulesViewModels(
			DateOnly from, DateOnly to)
		{
			var period = new DateOnlyPeriod(from, to);
			var studentAvailabilityDays = _studentAvailabilityProvider.GetStudentAvailabilityDayForPeriod(period);
			var calendarDates = _bankHolidayCalendarProvider.GetMySiteBankHolidayDates(period).ToList();

			return  (from availableDay in studentAvailabilityDays
				let bankHolidayDate = calendarDates.FirstOrDefault(cd => cd.Date == availableDay.RestrictionDate)
				select _dayMapper.Map(availableDay, bankHolidayDate)).ToArray();
		}
	}
}