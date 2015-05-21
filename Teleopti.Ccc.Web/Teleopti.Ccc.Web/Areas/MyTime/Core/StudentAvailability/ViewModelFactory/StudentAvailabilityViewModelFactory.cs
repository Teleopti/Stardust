﻿using AutoMapper;
using System.Collections.Generic;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Models.StudentAvailability;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.StudentAvailability.ViewModelFactory
{
	public class StudentAvailabilityViewModelFactory : IStudentAvailabilityViewModelFactory
	{
		private readonly IMappingEngine _mapper;
		private readonly IStudentAvailabilityProvider _studentAvailabilityProvider;
		private readonly IScheduleProvider _scheduleProvider;

		public StudentAvailabilityViewModelFactory(IMappingEngine mapper, IStudentAvailabilityProvider studentAvailabilityProvider, IScheduleProvider scheduleProvider)
		{
			_mapper = mapper;
			_studentAvailabilityProvider = studentAvailabilityProvider;
			_scheduleProvider = scheduleProvider;
		}

		public StudentAvailabilityViewModel CreateViewModel(DateOnly dateInPeriod)
		{
			return _mapper.Map<DateOnly, StudentAvailabilityViewModel>(dateInPeriod);
		}

		public StudentAvailabilityDayFeedbackViewModel CreateDayFeedbackViewModel(DateOnly date)
		{
			return _mapper.Map<DateOnly, StudentAvailabilityDayFeedbackViewModel>(date);
		}

		public StudentAvailabilityDayViewModel CreateDayViewModel(DateOnly date)
		{
			var studentAvailability = _studentAvailabilityProvider.GetStudentAvailabilityDayForDate(date);
			if (studentAvailability == null) return null;
			return _mapper.Map<IStudentAvailabilityDay, StudentAvailabilityDayViewModel>(studentAvailability);
		}

		public IEnumerable<StudentAvailabilityDayViewModel> CreateStudentAvailabilityAndSchedulesViewModels(
			DateOnly from, DateOnly to)
		{
			var period = new DateOnlyPeriod(from, to);
			var scheduleDays = _scheduleProvider.GetScheduleForStudentAvailability(period);
			return _mapper.Map<IEnumerable<IScheduleDay>, IEnumerable<StudentAvailabilityDayViewModel>>(scheduleDays);
		}
	}
}