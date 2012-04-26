using System.ServiceModel;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.Logic.CommandHandler
{
    public class DeletePersonAccountForPersonCommandHandler : IHandleCommand<DeletePersonAccountForPersonCommandDto>
    {
        private readonly IPersonRepository _personRepository;
        private readonly IPersonAbsenceAccountRepository _personAbsenceAccountRepository;
        private readonly IAbsenceRepository _absenceRepository;
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;

        public DeletePersonAccountForPersonCommandHandler(IPersonRepository personRepository, IPersonAbsenceAccountRepository personAbsenceAccountRepository, IAbsenceRepository absenceRepository, IUnitOfWorkFactory unitOfWorkFactory)
        {
            _personRepository = personRepository;
            _personAbsenceAccountRepository = personAbsenceAccountRepository;
            _absenceRepository = absenceRepository;
            _unitOfWorkFactory = unitOfWorkFactory;
        }

        public CommandResultDto Handle(DeletePersonAccountForPersonCommandDto command)
        {
            using (var unitOfWork = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
            {
                var foundPerson = _personRepository.Get(command.PersonId);
                if (foundPerson == null) throw new FaultException("Person is not exist.");
                var foundAbsence = _absenceRepository.Get(command.AbsenceId);
                if (foundAbsence == null) throw new FaultException("Absence is not exist.");
                var dateFrom = new DateOnly(command.DateFrom.DateTime);

                checkIfAuthorized(foundPerson, dateFrom);

                var accounts = _personAbsenceAccountRepository.Find(foundPerson);
                var personAccount = accounts.Find(foundAbsence, dateFrom);
                if (personAccount != null)
                    accounts.Remove(personAccount);

                unitOfWork.PersistAll();
            }
            return new CommandResultDto { AffectedId = command.PersonId, AffectedItems = 1 };
        }

        private static void checkIfAuthorized(IPerson person, DateOnly dateOnly)
        {
            if (!TeleoptiPrincipal.Current.PrincipalAuthorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.OpenPersonAdminPage, dateOnly, person))
            {
                throw new FaultException("You're not allowed to modify person accounts for this person.");
            }
        }
    }
}
