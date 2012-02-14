using AutoMapper;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Models.StudentAvailability;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.StudentAvailability.ViewModelFactory
{
	public class StudentAvailabilityViewModelFactory : IStudentAvailabilityViewModelFactory
	{
		private readonly IMappingEngine _mapper;
		private readonly IStudentAvailabilityProvider _studentAvailabilityProvider;

		public StudentAvailabilityViewModelFactory(IMappingEngine mapper, IStudentAvailabilityProvider studentAvailabilityProvider)
		{
			_mapper = mapper;
			_studentAvailabilityProvider = studentAvailabilityProvider;
		}

		public StudentAvailabilityViewModel CreateViewModel(DateOnly dateInPeriod)
		{
			return _mapper.Map<DateOnly, StudentAvailabilityViewModel>(dateInPeriod);
		}
		
		public StudentAvailabilityDayViewModel CreateDayViewModel(DateOnly date)
		{
			var studentAvailability = _studentAvailabilityProvider.GetStudentAvailabilityForDate(date);
			if (studentAvailability == null)
				return new StudentAvailabilityDayViewModel();
			return _mapper.Map<IStudentAvailabilityRestriction, StudentAvailabilityDayViewModel>(studentAvailability);
		}
	}
}