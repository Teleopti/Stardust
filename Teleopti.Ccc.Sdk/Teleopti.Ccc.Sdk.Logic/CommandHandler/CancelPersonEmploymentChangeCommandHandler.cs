using System.Linq;
using System.ServiceModel;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;

namespace Teleopti.Ccc.Sdk.Logic.CommandHandler
{
    public class CancelPersonEmploymentChangeCommandHandler : IHandleCommand<CancelPersonEmploymentChangeCommandDto>
    {
        private readonly ICurrentUnitOfWorkFactory _unitOfWorkFactory;
        private readonly IPersonRepository _personRepository;
		private readonly ICurrentAuthorization _currentAuthorization;

		public CancelPersonEmploymentChangeCommandHandler(ICurrentUnitOfWorkFactory unitOfWorkFactory, IPersonRepository personRepository, ICurrentAuthorization currentAuthorization)
        {
            _unitOfWorkFactory = unitOfWorkFactory;
            _personRepository = personRepository;
			_currentAuthorization = currentAuthorization;
		}

		public void Handle(CancelPersonEmploymentChangeCommandDto command)
        {
			var affectedItems = 0;
			var dateToRemoveAfter = new DateOnly(command.Date.DateTime);

			using (var uow = _unitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				var person = _personRepository.Get(command.PersonId);
				if (person == null)
				{
					throw new FaultException($"Person with {command.PersonId} not found.");
				}

				person.VerifyCanBeModifiedByCurrentUser(_currentAuthorization);

				if (dateToRemoveAfter > person.TerminalDate)
					throw new FaultException("You cannot change person employment after the persons leaving date.");

				var existingPersonPeriods =
					person.PersonPeriodCollection?
						.Where(pp => pp.StartDate > dateToRemoveAfter.AddDays(-1)).ToList().Count ?? 0;

				if (existingPersonPeriods > 0)
				{
					person.RemoveAllPeriodsAfter(dateToRemoveAfter.AddDays(-1));
					affectedItems = existingPersonPeriods - 
									person.PersonPeriodCollection?
										.Where(pp => pp.StartDate > dateToRemoveAfter.AddDays(-1)).ToList().Count ?? 0;
				}

				uow.PersistAll();
			}

			command.Result = new CommandResultDto { AffectedId = command.PersonId, AffectedItems = affectedItems };
        }
    }
}
