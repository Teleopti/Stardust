using System.Linq;
using System.Web;
using AutoMapper;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.MyTime.Models.StudentAvailability;
using Teleopti.Ccc.Web.Core;
using Teleopti.Ccc.Web.Core.RequestContext;
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

		public StudentAvailabilityDayFormResult Persist(StudentAvailabilityDayForm form)
		{
			var studentAvailabilityDay = _studentAvailabilityDayRepository.Find(form.Date, _loggedOnUser.CurrentUser()).SingleOrDefaultNullSafe();
			if (studentAvailabilityDay != null)
			{
				studentAvailabilityDay = _mapper.Map(form, studentAvailabilityDay);
			}
			else
			{
				studentAvailabilityDay = _mapper.Map<StudentAvailabilityDayForm, IStudentAvailabilityDay>(form);
				_studentAvailabilityDayRepository.Add(studentAvailabilityDay);
			}
			return _mapper.Map<IStudentAvailabilityDay, StudentAvailabilityDayFormResult>(studentAvailabilityDay);
		}

		public StudentAvailabilityDayFormResult Delete(DateOnly date)
		{
			var studentAvailInDatasource = _studentAvailabilityDayRepository.Find(date, _loggedOnUser.CurrentUser());
			if (studentAvailInDatasource.IsEmpty())
				throw new HttpException(404, "StudentAvailability not found");

			_studentAvailabilityDayRepository.Remove( studentAvailInDatasource.Single());
			return new StudentAvailabilityDayFormResult { Date = date.ToFixedClientDateOnlyFormat() };
		}
	}
}