using System.Linq;
using System.Web;
using AutoMapper;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.MyTime.Models.StudentAvailability;
using Teleopti.Ccc.Web.Core;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.StudentAvailability.DataProvider
{
	public class StudentAvailabilityPersister : IStudentAvailabilityPersister
	{
		private readonly IStudentAvailabilityDayRepository _studentAvailabilityDayRepository;
		private readonly IMappingEngine _mapper;
		private readonly ILoggedOnUser _loggedOnUser;

		public StudentAvailabilityPersister(IStudentAvailabilityDayRepository studentAvailabilityDayRepository, IMappingEngine mapper, ILoggedOnUser loggedOnUser)
		{
			_studentAvailabilityDayRepository = studentAvailabilityDayRepository;
			_mapper = mapper;
			_loggedOnUser = loggedOnUser;
		}

		public StudentAvailabilityDayViewModel Persist(StudentAvailabilityDayInput input)
		{
			var studentAvailabilityDay = _studentAvailabilityDayRepository.Find(input.Date, _loggedOnUser.CurrentUser()).SingleOrDefaultNullSafe();
			if (studentAvailabilityDay != null)
			{
				studentAvailabilityDay = _mapper.Map(input, studentAvailabilityDay);
			}
			else
			{
				studentAvailabilityDay = _mapper.Map<StudentAvailabilityDayInput, IStudentAvailabilityDay>(input);
				_studentAvailabilityDayRepository.Add(studentAvailabilityDay);
			}
			return _mapper.Map<IStudentAvailabilityDay, StudentAvailabilityDayViewModel>(studentAvailabilityDay);
		}

		public StudentAvailabilityDayViewModel Delete(DateOnly date)
		{
			var studentAvailInDatasource = _studentAvailabilityDayRepository.Find(date, _loggedOnUser.CurrentUser());
			if (studentAvailInDatasource.IsEmpty())
				throw new HttpException(404, "StudentAvailability not found");

			_studentAvailabilityDayRepository.Remove( studentAvailInDatasource.Single());
			return new StudentAvailabilityDayViewModel { Date = date.ToFixedClientDateOnlyFormat() };
		}
	}
}