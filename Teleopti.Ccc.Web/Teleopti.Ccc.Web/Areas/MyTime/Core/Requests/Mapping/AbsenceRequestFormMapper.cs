using System;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping
{
	public class AbsenceRequestFormMapper
	{
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly IAbsenceRepository _absenceRepository;
		private readonly IUserTimeZone _userTimeZone;

		public AbsenceRequestFormMapper(ILoggedOnUser loggedOnUser, IAbsenceRepository absenceRepository,
			IUserTimeZone userTimeZone)
		{
			_loggedOnUser = loggedOnUser;
			_absenceRepository = absenceRepository;
			_userTimeZone = userTimeZone;
		}

		public IPersonRequest Map(AbsenceRequestForm source, IPersonRequest destination = null)
		{
			destination = destination ?? new PersonRequest(_loggedOnUser.CurrentUser()) {Subject = source.Subject};

			DateTimePeriod period;

			if (source.FullDay)
			{
				var startTime = TimeZoneHelper.ConvertToUtc(new DateTime(2012, 5, 11, 0, 0, 0), _userTimeZone.TimeZone());
				var endTime = TimeZoneHelper.ConvertToUtc(new DateTime(2012, 5, 11, 23, 59, 0), _userTimeZone.TimeZone());
				period = new DateTimePeriod(startTime, endTime);
			}
			else
			{
				period = new DateTimePeriodFormMapper(_userTimeZone).Map(source.Period);
			}

			destination.TrySetMessage(source.Message ?? "");
			destination.Request = new AbsenceRequest(_absenceRepository.Load(source.AbsenceId), period);

			if (source.EntityId != null)
				destination.SetId(source.EntityId);

			destination.Subject = source.Subject;

			return destination;
		}
	}
}