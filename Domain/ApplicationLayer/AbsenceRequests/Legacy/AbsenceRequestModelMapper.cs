using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests.Legacy
{
	public class AbsenceRequestModelMapper
	{
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly IAbsenceRepository _absenceRepository;

		public AbsenceRequestModelMapper(ILoggedOnUser loggedOnUser, IAbsenceRepository absenceRepository)
		{
			_loggedOnUser = loggedOnUser;
			_absenceRepository = absenceRepository;
		}

		public IPersonRequest MapExistingAbsenceRequest(AbsenceRequestModel source, IPersonRequest existingPersonRequest)
		{
			existingPersonRequest.Subject = source.Subject;
			existingPersonRequest.TrySetMessage(source.Message ?? string.Empty);

			var absenceRequest = (AbsenceRequest) existingPersonRequest.Request;
			absenceRequest.SetAbsence(_absenceRepository.Load(source.AbsenceId));
			absenceRequest.SetPeriod(source.Period);

			return existingPersonRequest;
		}

		public IPersonRequest MapNewAbsenceRequest(AbsenceRequestModel source)
		{
			var personRequest = new PersonRequest(_loggedOnUser.CurrentUser()) {Subject = source.Subject};
			
			personRequest.TrySetMessage(source.Message ?? string.Empty);
			personRequest.Request = new AbsenceRequest(_absenceRepository.Load(source.AbsenceId), source.Period)
				{FullDay = source.FullDay};
			return personRequest;
		}
	}
}