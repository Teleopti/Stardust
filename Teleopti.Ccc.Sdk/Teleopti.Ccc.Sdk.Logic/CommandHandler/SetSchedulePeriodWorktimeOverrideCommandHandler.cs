using System;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;
using Teleopti.Ccc.Sdk.Logic.QueryHandler;

namespace Teleopti.Ccc.Sdk.Logic.CommandHandler
{
	public class SetSchedulePeriodWorktimeOverrideCommandHandler : IHandleCommand<SetSchedulePeriodWorktimeOverrideCommandDto>
	{
		private readonly IPersonRepository _personRepository;
		private readonly ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;
		private readonly ICurrentAuthorization _currentAuthorization;

		public SetSchedulePeriodWorktimeOverrideCommandHandler(IPersonRepository personRepository, ICurrentUnitOfWorkFactory currentUnitOfWorkFactory, ICurrentAuthorization currentAuthorization)
		{
			_personRepository = personRepository;
			_currentUnitOfWorkFactory = currentUnitOfWorkFactory;
			_currentAuthorization = currentAuthorization;
		}

		public void Handle(SetSchedulePeriodWorktimeOverrideCommandDto command)
		{
			using (var uow = _currentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				var person = _personRepository.Get(command.PersonId);
				if (person == null)
				{
					command.Result = new CommandResultDto();
					return;
				}
				person.VerifyCanBeModifiedByCurrentUser(_currentAuthorization);
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
	}
}