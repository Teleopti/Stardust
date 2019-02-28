using System.Linq;
using System.Web;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.MyTime.Core.StudentAvailability.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Models.StudentAvailability;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.StudentAvailability.DataProvider
{
	public class StudentAvailabilityPersister : IStudentAvailabilityPersister
	{
		private readonly IStudentAvailabilityDayRepository _studentAvailabilityDayRepository;
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly StudentAvailabilityDayFormMapper _formMapper;
		private readonly StudentAvailabilityDayViewModelMapper _modelMapper;

		public StudentAvailabilityPersister(IStudentAvailabilityDayRepository studentAvailabilityDayRepository, ILoggedOnUser loggedOnUser, StudentAvailabilityDayFormMapper formMapper, StudentAvailabilityDayViewModelMapper modelMapper)
		{
			_studentAvailabilityDayRepository = studentAvailabilityDayRepository;
			_loggedOnUser = loggedOnUser;
			_formMapper = formMapper;
			_modelMapper = modelMapper;
		}

		public StudentAvailabilityDayViewModel Persist(StudentAvailabilityDayInput input)
		{
			var studentAvailabilityDay = _studentAvailabilityDayRepository.Find(input.Date, _loggedOnUser.CurrentUser()).SingleOrDefaultNullSafe();
			if (studentAvailabilityDay != null)
			{
				studentAvailabilityDay = _formMapper.Map(input, studentAvailabilityDay);
			}
			else
			{
				studentAvailabilityDay = _formMapper.Map(input);
				_studentAvailabilityDayRepository.Add(studentAvailabilityDay);
			}
			return _modelMapper.Map(studentAvailabilityDay, null);
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