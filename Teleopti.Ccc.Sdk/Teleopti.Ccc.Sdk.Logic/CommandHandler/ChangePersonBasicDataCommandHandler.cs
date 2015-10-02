using System.Globalization;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.Logic.CommandHandler
{
	public class ChangePersonBasicDataCommandHandler : IHandleCommand<ChangePersonBasicDataCommandDto>
	{
		private readonly IPersonRepository _personRepository;
		private readonly ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;
		private readonly IWorkflowControlSetRepository _workflowControlSetRepository;

		public ChangePersonBasicDataCommandHandler(IPersonRepository personRepository, ICurrentUnitOfWorkFactory currentUnitOfWorkFactory, IWorkflowControlSetRepository workflowControlSetRepository)
		{
			_personRepository = personRepository;
			_currentUnitOfWorkFactory = currentUnitOfWorkFactory;
			_workflowControlSetRepository = workflowControlSetRepository;
		}

		public void Handle(ChangePersonBasicDataCommandDto command)
		{
			using (var uow = _currentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				var person = _personRepository.Load(command.PersonId);
				if (command.CultureLanguageId.HasValue)
					person.PermissionInformation.SetCulture(new CultureInfo(command.CultureLanguageId.Value));
				if (command.UICultureLanguageId.HasValue)
					person.PermissionInformation.SetUICulture(new CultureInfo(command.UICultureLanguageId.Value));
				if (command.WorkWeekStart.HasValue)
					person.FirstDayOfWeek = command.WorkWeekStart.Value;
				if (command.WorkflowControlSetId.HasValue)
				{
					var workflowControlSet = _workflowControlSetRepository.Get(command.WorkflowControlSetId.Value);
					person.WorkflowControlSet = workflowControlSet;
				}

				if (command.FirstName != null || command.LastName != null)
					person.Name = new Name(command.FirstName, command.LastName);
				if (command.Email != null)
					person.Email = command.Email;
				if (command.EmploymentNumber != null)
					person.EmploymentNumber = command.EmploymentNumber;
				if (command.Note != null)
					person.Note = command.Note;
				if (command.IsDeleted)
					((IDeleteTag)person).SetDeleted();

				uow.PersistAll();
				command.Result = new CommandResultDto { AffectedId = person.Id.GetValueOrDefault(), AffectedItems = 1 };
			}
			
		}
	}
}