﻿using System;
using System.ServiceModel;
using Teleopti.Ccc.Domain.ApplicationLayer;
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
    public class SetPersonAccountForPersonCommandHandler : IHandleCommand<SetPersonAccountForPersonCommandDto>
    {
        private readonly IPersonRepository _personRepository;
        private readonly IPersonAbsenceAccountRepository _personAbsenceAccountRepository;
        private readonly IAbsenceRepository _absenceRepository;
        private readonly ICurrentUnitOfWorkFactory _unitOfWorkFactory;
        private readonly ITraceableRefreshService _traceableRefreshService;

        public SetPersonAccountForPersonCommandHandler(IPersonRepository personRepository, IPersonAbsenceAccountRepository personAbsenceAccountRepository, IAbsenceRepository absenceRepository, ICurrentUnitOfWorkFactory unitOfWorkFactory, ITraceableRefreshService traceableRefreshService)
        {
            _personRepository = personRepository;
            _personAbsenceAccountRepository = personAbsenceAccountRepository;
            _absenceRepository = absenceRepository;
            _unitOfWorkFactory = unitOfWorkFactory;
            _traceableRefreshService = traceableRefreshService;
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public void Handle(SetPersonAccountForPersonCommandDto command)
        {
            using (var unitOfWork = _unitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
            {
				var foundPerson = _personRepository.Get(command.PersonId);
                if (foundPerson == null) throw new FaultException("Person does not exist.");
                var foundAbsence = _absenceRepository.Get(command.AbsenceId);
                if (foundAbsence == null) throw new FaultException("Absence does not exist.");
                var dateFrom = command.DateFrom.ToDateOnly();

                checkIfAuthorized(foundPerson, dateFrom);

				var accounts = _personAbsenceAccountRepository.Find(foundPerson);
                var personAccount = accounts.Find(foundAbsence, dateFrom);
                if (personAccount == null || !personAccount.StartDate.Equals(dateFrom))
				{
					var originalAccount = personAccount;
					personAccount = createPersonAccount(foundAbsence, accounts, dateFrom);
					setPersonAccount(personAccount, command);

					if (originalAccount != null) _traceableRefreshService.Refresh(originalAccount);
					_traceableRefreshService.Refresh(personAccount);
                }
                else
                {
					setPersonAccount(personAccount, command);
                }
				          
                unitOfWork.PersistAll();
            }
			command.Result = new CommandResultDto { AffectedId = command.PersonId, AffectedItems = 1 };
        }

        private static void checkIfAuthorized(IPerson person, DateOnly dateOnly)
        {
            if (!PrincipalAuthorization.Current().IsPermitted(DefinedRaptorApplicationFunctionPaths.OpenPersonAdminPage, dateOnly, person))
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
                account.Accrued = TimeSpan.FromTicks(command.Accrued.Value);
            if (command.BalanceIn.HasValue)
                account.BalanceIn = TimeSpan.FromTicks(command.BalanceIn.Value);
            if (command.Extra.HasValue)
                account.Extra = TimeSpan.FromTicks(command.Extra.Value);
        }
    }
}