using System.Web;
using AutoMapper;
using Teleopti.Ccc.Domain.Collection;
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
				_overtimeAvailabilityRepository.Remove(overtimeAvailability);
				overtimeAvailability = _mapper.Map<OvertimeAvailabilityInput, IOvertimeAvailability>(input);
				_overtimeAvailabilityRepository.Add(overtimeAvailability);
			}
			return _mapper.Map<IOvertimeAvailability, OvertimeAvailabilityViewModel>(overtimeAvailability);
		}

		public OvertimeAvailabilityViewModel Delete(DateOnly date)
		{
			var overtimeAvailabilities = _overtimeAvailabilityRepository.Find(date, _loggedOnUser.CurrentUser());
			if (overtimeAvailabilities.IsEmpty())
				throw new HttpException(404, "OvertimeAvailability not found");

			foreach (var overtimeAvailability in overtimeAvailabilities)
			{
				_overtimeAvailabilityRepository.Remove(overtimeAvailability);
			}
			return new OvertimeAvailabilityViewModel
				{
					HasOvertimeAvailability = false,
					StartTime = null,
					EndTime = null,
					EndTimeNextDay = false
				};
		}
	}
}