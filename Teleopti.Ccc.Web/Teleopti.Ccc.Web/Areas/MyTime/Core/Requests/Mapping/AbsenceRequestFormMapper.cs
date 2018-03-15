using System;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
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

		public IPersonRequest MapExistingAbsenceRequest(AbsenceRequestForm source, IPersonRequest existingPersonRequest)
		{
			var period = new DateTimePeriodFormMapper(_userTimeZone).Map(source.Period);

			existingPersonRequest.Subject = source.Subject;
			existingPersonRequest.TrySetMessage(source.Message ?? string.Empty);

			var absenceRequest = (AbsenceRequest) existingPersonRequest.Request;
			absenceRequest.SetAbsence(_absenceRepository.Load(source.AbsenceId));
			absenceRequest.SetPeriod(period);

			return existingPersonRequest;
		}

		public IPersonRequest MapNewAbsenceRequest(AbsenceRequestForm source)
		{
			var personRequest = new PersonRequest(_loggedOnUser.CurrentUser()) {Subject = source.Subject};
			var period = new DateTimePeriodFormMapper(_userTimeZone).Map(source.Period);

			personRequest.TrySetMessage(source.Message ?? string.Empty);
			personRequest.Request = new AbsenceRequest(_absenceRepository.Load(source.AbsenceId), period);

			return personRequest;
		}
	}
}