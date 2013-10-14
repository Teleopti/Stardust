using System;
using System.Linq;
using System.ServiceModel;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;
using Teleopti.Ccc.Sdk.Logic.QueryHandler;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.Logic.CommandHandler
{
	public class SetSchedulePeriodWorktimeOverrideCommandHandler : IHandleCommand<SetSchedulePeriodWorktimeOverrideCommandDto>
	{
		private readonly IPersonRepository _personRepository;
		private readonly ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;

		public SetSchedulePeriodWorktimeOverrideCommandHandler(IPersonRepository personRepository, ICurrentUnitOfWorkFactory currentUnitOfWorkFactory)
		{
			_personRepository = personRepository;
			_currentUnitOfWorkFactory = currentUnitOfWorkFactory;
		}

		public void Handle(SetSchedulePeriodWorktimeOverrideCommandDto command)
		{
			using (var uow = _currentUnitOfWorkFactory.LoggedOnUnitOfWorkFactory().CreateAndOpenUnitOfWork())
			{
				var person = _personRepository.Get(command.PersonId);
				if (person == null)
				{
					command.Result = new CommandResultDto();
					return;
				}
				checkIfAuthorized(person);
				var period = person.SchedulePeriod(command.Date.ToDateOnly());
				if (period == null)
				{
					command.Result = new CommandResultDto();
					return;
				}
				period.PeriodTime = TimeSpan.FromMinutes(command.PeriodTimeInMinutes);
				var result = uow.PersistAll();
				command.Result = new CommandResultDto { AffectedItems = result.Count(), AffectedId = period.Id };
			}
		}

		private static void checkIfAuthorized(IPerson person)
		{
			if (!PrincipalAuthorization.Instance().IsPermitted(DefinedRaptorApplicationFunctionPaths.OpenPersonAdminPage, DateOnly.Today, person))
			{
				throw new FaultException("You're not allowed to modify person details.");
			}
		}
	}
}