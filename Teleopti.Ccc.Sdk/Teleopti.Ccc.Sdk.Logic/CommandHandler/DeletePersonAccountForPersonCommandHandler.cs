using System.ServiceModel;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.Tracking;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;
using Teleopti.Ccc.Sdk.Logic.QueryHandler;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.Logic.CommandHandler
{
    public class DeletePersonAccountForPersonCommandHandler : IHandleCommand<DeletePersonAccountForPersonCommandDto>
    {
        private readonly IPersonRepository _personRepository;
        private readonly IPersonAbsenceAccountRepository _personAbsenceAccountRepository;
        private readonly IAbsenceRepository _absenceRepository;
	    private readonly ICurrentScenario _currentScenario;
	    private readonly IRepositoryFactory _repositoryFactory;
	    private readonly ICurrentUnitOfWorkFactory _unitOfWorkFactory;

        public DeletePersonAccountForPersonCommandHandler(IPersonRepository personRepository, IPersonAbsenceAccountRepository personAbsenceAccountRepository, IAbsenceRepository absenceRepository, ICurrentScenario currentScenario, IRepositoryFactory repositoryFactory, ICurrentUnitOfWorkFactory unitOfWorkFactory)
        {
            _personRepository = personRepository;
            _personAbsenceAccountRepository = personAbsenceAccountRepository;
            _absenceRepository = absenceRepository;
	        _currentScenario = currentScenario;
	        _repositoryFactory = repositoryFactory;
	        _unitOfWorkFactory = unitOfWorkFactory;
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public void Handle(DeletePersonAccountForPersonCommandDto command)
		{
			var result = new CommandResultDto {AffectedId = command.PersonId, AffectedItems = 0};
            using (var unitOfWork = _unitOfWorkFactory.LoggedOnUnitOfWorkFactory().CreateAndOpenUnitOfWork())
            {
				var foundPerson = _personRepository.Get(command.PersonId);
                if (foundPerson == null) throw new FaultException("Person does not exist.");
                var foundAbsence = _absenceRepository.Get(command.AbsenceId);
                if (foundAbsence == null) throw new FaultException("Absence does not exist.");
                var dateFrom = command.DateFrom.ToDateOnly();

                checkIfAuthorized(foundPerson, dateFrom);

	            var accounts = _personAbsenceAccountRepository.Find(foundPerson);
                var personAccount = accounts.Find(foundAbsence, dateFrom);
                if (personAccount != null)
                {
                	accounts.Remove(personAccount);
					result.AffectedItems = 1;
                }
				personAccount = accounts.Find(foundAbsence, dateFrom);
				if (personAccount != null)
				{
					var refreshService = new TraceableRefreshService(_currentScenario.Current(), _repositoryFactory);
					refreshService.Refresh(personAccount,unitOfWork);
				}

                unitOfWork.PersistAll();
            }
			command.Result = result;
        }

        private static void checkIfAuthorized(IPerson person, DateOnly dateOnly)
        {
            if (!PrincipalAuthorization.Instance().IsPermitted(DefinedRaptorApplicationFunctionPaths.OpenPersonAdminPage, dateOnly, person))
            {
                throw new FaultException("You're not allowed to modify person accounts for this person.");
            }
        }
    }
}
