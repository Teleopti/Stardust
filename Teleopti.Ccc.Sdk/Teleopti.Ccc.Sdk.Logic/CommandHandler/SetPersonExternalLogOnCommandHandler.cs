using System;
using System.Linq;
using System.ServiceModel;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;
using Teleopti.Ccc.Sdk.Logic.QueryHandler;

namespace Teleopti.Ccc.Sdk.Logic.CommandHandler
{
	public class SetPersonExternalLogOnCommandHandler : IHandleCommand<SetPersonExternalLogOnCommandDto>
	{
		private readonly IPersonRepository _personRepository;
		private readonly ICurrentUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IExternalLogOnRepository _externalLogOnRepository;
		private readonly ICurrentAuthorization _currentAuthorization;

		public SetPersonExternalLogOnCommandHandler(IPersonRepository personRepository, ICurrentUnitOfWorkFactory unitOfWorkFactory, IExternalLogOnRepository externalLogOnRepository, ICurrentAuthorization currentAuthorization)
		{
			_personRepository = personRepository;
			_unitOfWorkFactory = unitOfWorkFactory;
			_externalLogOnRepository = externalLogOnRepository;
			_currentAuthorization = currentAuthorization;
		}

		public void Handle(SetPersonExternalLogOnCommandDto command)
		{
			using (var uow = _unitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				var person = _personRepository.Get(command.PersonId);
				person.VerifyCanBeModifiedByCurrentUser(_currentAuthorization);

				var existingPersonPeriod =
					person.PersonPeriodCollection.FirstOrDefault(pp => pp.StartDate == command.PeriodStartDate.ToDateOnly());

				var affectedId = existingPersonPeriod != null ? 
					updateExistingPersonPeriod(command, person, existingPersonPeriod, uow) : 
					createNewPersonPeriod(command, person, uow);

				command.Result = new CommandResultDto { AffectedId = affectedId, AffectedItems = 1 };
			}
		}

		private Guid? updateExistingPersonPeriod(SetPersonExternalLogOnCommandDto command, IPerson person, IPersonPeriod existPersonPeriod, IUnitOfWork uow)
		{
			updateExternalLogon(command, person, existPersonPeriod);
			uow.PersistAll();
			return existPersonPeriod.Id;
		}

		private void updateExternalLogon(SetPersonExternalLogOnCommandDto command, IPerson person,
			IPersonPeriod existPersonPeriod)
		{
			foreach (var externalLogonDto in command.ExternalLogOn)
			{
				var externalLogon = _externalLogOnRepository.Get(externalLogonDto.Id.GetValueOrDefault());
				person.AddExternalLogOn(externalLogon, existPersonPeriod);
			}
		}

		private Guid? createNewPersonPeriod(SetPersonExternalLogOnCommandDto command, IPerson person, IUnitOfWork uow)
		{
			var lastPersonPeriod = person.PersonPeriodCollection.OrderBy(pp => pp.Period.StartDate).LastOrDefault();
			IPersonPeriod newPersonPeriod;
			if (lastPersonPeriod != null)
			{
				newPersonPeriod = new PersonPeriod(command.PeriodStartDate.ToDateOnly(), lastPersonPeriod.PersonContract, lastPersonPeriod.Team);
				person.AddPersonPeriod(newPersonPeriod);

				lastPersonPeriod.PersonSkillCollection.ForEach(x => person.AddSkill(x, newPersonPeriod));

				newPersonPeriod.Note = lastPersonPeriod.Note;
				newPersonPeriod.BudgetGroup = lastPersonPeriod.BudgetGroup;
				newPersonPeriod.RuleSetBag = lastPersonPeriod.RuleSetBag;
				updateExternalLogon(command, person, newPersonPeriod);
			}
			else
			{
				throw new FaultException($"The specified person {person.Name} has no existing person periods to clone.");
			}

			uow.PersistAll();
			return newPersonPeriod.Id;
		}
	}
}