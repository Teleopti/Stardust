using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Schedule.Common;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Schedule.WeekSchedule;


namespace Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.DataProvider
{
	public class OvertimeAvailabilityPersister : IOvertimeAvailabilityPersister
	{
		private readonly IOvertimeAvailabilityRepository _overtimeAvailabilityRepository;
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly OvertimeAvailabilityInputMapper _inputMapper;
		private readonly OvertimeAvailabilityViewModelMapper _mapper;

		public OvertimeAvailabilityPersister(IOvertimeAvailabilityRepository overtimeAvailabilityRepository, ILoggedOnUser loggedOnUser, OvertimeAvailabilityInputMapper inputMapper, OvertimeAvailabilityViewModelMapper mapper)
		{
			_overtimeAvailabilityRepository = overtimeAvailabilityRepository;
			_loggedOnUser = loggedOnUser;
			_inputMapper = inputMapper;
			_mapper = mapper;
		}

		public OvertimeAvailabilityViewModel Persist(OvertimeAvailabilityInput input)
		{
			var overtimeAvailabilities = _overtimeAvailabilityRepository.Find(input.Date, _loggedOnUser.CurrentUser());
			var overtimeAvailability = overtimeAvailabilities.SingleOrDefaultNullSafe();
			if (overtimeAvailability == null)
			{
				overtimeAvailability = _inputMapper.Map(input);
				_overtimeAvailabilityRepository.Add(overtimeAvailability);
			}
			else
			{
				_overtimeAvailabilityRepository.Remove(overtimeAvailability);
				overtimeAvailability = _inputMapper.Map(input);
				_overtimeAvailabilityRepository.Add(overtimeAvailability);
			}
			return _mapper.Map(overtimeAvailability);
		}

		public OvertimeAvailabilityViewModel Delete(DateOnly date)
		{
			var overtimeAvailabilities = _overtimeAvailabilityRepository.Find(date, _loggedOnUser.CurrentUser());

			if (!overtimeAvailabilities.IsEmpty())
			{
				foreach (var overtimeAvailability in overtimeAvailabilities)
				{
					_overtimeAvailabilityRepository.Remove(overtimeAvailability);
				}
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