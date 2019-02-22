using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;
using Teleopti.Ccc.Sdk.Logic.QueryHandler;

namespace Teleopti.Ccc.Sdk.Logic.CommandHandler
{
	public class EndPersonEmploymentCommandHandler : IHandleCommand<EndPersonEmploymentCommandDto>
	{
		private readonly IPersonRepository _personRepository;
		private readonly ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;
		private readonly IPersonLeavingUpdater _personLeavingUpdater;
		private readonly IPersonAccountUpdater _personAccountUpdater;
		private readonly ICurrentAuthorization _currentAuthorization;

		public EndPersonEmploymentCommandHandler(IPersonRepository personRepository, ICurrentUnitOfWorkFactory currentUnitOfWorkFactory, IPersonLeavingUpdater personLeavingUpdater, IPersonAccountUpdater personAccountUpdater, ICurrentAuthorization currentAuthorization)
		{
			_personRepository = personRepository;
			_currentUnitOfWorkFactory = currentUnitOfWorkFactory;
			_personLeavingUpdater = personLeavingUpdater;
			_personAccountUpdater = personAccountUpdater;
			_currentAuthorization = currentAuthorization;
		}

		public void Handle(EndPersonEmploymentCommandDto command)
		{
			using (var uow = _currentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				var person = _personRepository.Load(command.PersonId);
				person.VerifyCanBeModifiedByCurrentUser(_currentAuthorization);
				var updater = command.ClearAfterLeavingDate ? _personLeavingUpdater : new DummyPersonLeavingUpdater();
				person.TerminatePerson(command.Date.ToDateOnly(), _personAccountUpdater, updater);
				uow.PersistAll();
				command.Result = new CommandResultDto { AffectedId = person.Id.GetValueOrDefault(), AffectedItems = 1 };
			}
		}
	}
}