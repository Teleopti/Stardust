using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests.Legacy
{
	public class AbsenceRequestModelMapper
	{
		private readonly IAbsenceRepository _absenceRepository;
		private readonly IPersonRepository _personRepository;

		public AbsenceRequestModelMapper(IAbsenceRepository absenceRepository, IPersonRepository personRepository)
		{
			_absenceRepository = absenceRepository;
			_personRepository = personRepository;
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
			var personRequest = new PersonRequest(_personRepository.Get(source.PersonId)) {Subject = source.Subject};
			
			personRequest.TrySetMessage(source.Message ?? string.Empty);
			personRequest.Request = new AbsenceRequest(_absenceRepository.Load(source.AbsenceId), source.Period)
				{FullDay = source.FullDay};
			return personRequest;
		}
	}
}