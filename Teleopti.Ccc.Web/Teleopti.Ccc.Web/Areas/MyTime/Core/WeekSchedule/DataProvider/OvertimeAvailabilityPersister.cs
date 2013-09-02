using AutoMapper;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.MyTime.Models.WeekSchedule;
using Teleopti.Ccc.Web.Core;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.DataProvider
{
	public class OvertimeAvailabilityPersister : IOvertimeAvailabilityPersister
	{
		private readonly IOvertimeAvailabilityRepository _overtimeAvailabilityRepository;
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly IMappingEngine _mapper;

		public OvertimeAvailabilityPersister(IOvertimeAvailabilityRepository overtimeAvailabilityRepository, ILoggedOnUser loggedOnUser,IMappingEngine mapper)
		{
			_overtimeAvailabilityRepository = overtimeAvailabilityRepository;
			_loggedOnUser = loggedOnUser;
			_mapper = mapper;
		}

		public OvertimeAvailabilityViewModel Persist(OvertimeAvailabilityInput input)
		{
			var overtimeAvailabilities = _overtimeAvailabilityRepository.Find(input.Date, _loggedOnUser.CurrentUser());
			var overtimeAvailability = overtimeAvailabilities.SingleOrDefaultNullSafe();
			if (overtimeAvailability == null)
			{
				overtimeAvailability = _mapper.Map<OvertimeAvailabilityInput, IOvertimeAvailability>(input);
				_overtimeAvailabilityRepository.Add(overtimeAvailability);
			}
			else
			{
				_mapper.Map(input, overtimeAvailability);
			}
			return _mapper.Map<IOvertimeAvailability, OvertimeAvailabilityViewModel>(overtimeAvailability);
		}
	}
}