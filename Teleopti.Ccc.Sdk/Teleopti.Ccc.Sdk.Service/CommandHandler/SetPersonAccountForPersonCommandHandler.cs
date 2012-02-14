using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.Tracking;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.WcfService.CommandHandler
{
    public class SetPersonAccountForPersonCommandHandler : IHandleCommand<SetPersonAccountForPersonCommandDto>
    {
        private readonly IRepositoryFactory _repositoryFactory;
        private readonly IScenarioRepository _scenarioRepository;
        private readonly IPersonRepository _personRepository;
        private readonly IPersonAbsenceAccountRepository _personAbsenceAccountRepository;
        private readonly IAbsenceRepository _absenceRepository;
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;

        public SetPersonAccountForPersonCommandHandler(IRepositoryFactory repositoryFactory, IScenarioRepository scenarioRepository, IPersonRepository personRepository, IPersonAbsenceAccountRepository personAbsenceAccountRepository, IAbsenceRepository absenceRepository, IUnitOfWorkFactory unitOfWorkFactory)
        {
            _repositoryFactory = repositoryFactory;
            _scenarioRepository = scenarioRepository;
            _personRepository = personRepository;
            _personAbsenceAccountRepository = personAbsenceAccountRepository;
            _absenceRepository = absenceRepository;
            _unitOfWorkFactory = unitOfWorkFactory;
        }

        public CommandResultDto Handle(SetPersonAccountForPersonCommandDto command)
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
                if (personAccount == null || !personAccount.StartDate.Equals(dateFrom))
                {
                    personAccount = createPersonAccount(foundAbsence, accounts, dateFrom);
                }

                setPersonAccount(personAccount, command);
                            
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

        private IAccount createPersonAccount(IAbsence foundAbsence, IPersonAccountCollection accounts, DateOnly dateFrom)
        {
            var newAccount = foundAbsence.Tracker.CreatePersonAccount(dateFrom);
            accounts.Add(foundAbsence, newAccount);
            _personAbsenceAccountRepository.AddRange(accounts.PersonAbsenceAccounts());
            return newAccount;
        }

        private void setPersonAccount(IAccount account, SetPersonAccountForPersonCommandDto command)
        {
            if (command.Accrued.HasValue)
                account.Accrued = TimeSpan.FromTicks(command.Accrued.GetValueOrDefault());
            if (command.BalanceIn.HasValue)
                account.BalanceIn = TimeSpan.FromTicks(command.BalanceIn.GetValueOrDefault());
            if (command.Extra.HasValue)
                account.Extra = TimeSpan.FromTicks(command.Extra.GetValueOrDefault());
            if (command.Accrued.HasValue || command.BalanceIn.HasValue || command.Extra.HasValue)
                    refreshAccount(account);
        }

        private void refreshAccount(IAccount account)
        {
            using (var unitOfWork = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
            {
                var refreshService = new TraceableRefreshService(_scenarioRepository.LoadDefaultScenario(),
                                                                 _repositoryFactory);
                refreshService.Refresh(account, unitOfWork);
                unitOfWork.PersistAll();
            }
        }
    }
}