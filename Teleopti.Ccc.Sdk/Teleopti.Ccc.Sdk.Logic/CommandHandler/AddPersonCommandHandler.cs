using System;
using System.Globalization;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;

namespace Teleopti.Ccc.Sdk.Logic.CommandHandler
{
	public class AddPersonCommandHandler : IHandleCommand<AddPersonCommandDto>
	{
		private readonly IPersonRepository _personRepository;
		private readonly ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;
		private readonly ITenantDataManagerClient _tenantDataManager;
		private readonly IWorkflowControlSetRepository _workflowControlSetRepository;

		public AddPersonCommandHandler(IPersonRepository personRepository, ICurrentUnitOfWorkFactory currentUnitOfWorkFactory, ITenantDataManagerClient tenantDataManager, IWorkflowControlSetRepository workflowControlSetRepository)
		{
			_personRepository = personRepository;
			_currentUnitOfWorkFactory = currentUnitOfWorkFactory;
			_tenantDataManager = tenantDataManager;
			_workflowControlSetRepository = workflowControlSetRepository;
		}

		public void Handle(AddPersonCommandDto command)
		{
			if (string.IsNullOrEmpty(command.FirstName) && string.IsNullOrEmpty(command.LastName))
				throw new ArgumentException("Both first and last name cannot be empty");
			if (string.IsNullOrEmpty(command.TimeZoneId))
				throw new ArgumentException("Timezone cannot be empty");
			if (!string.IsNullOrEmpty(command.ApplicationLogonName) && command.ApplicationLogOnPassword == null)
				throw new ArgumentException("Password cannot be null");
			using (var uow = _currentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				var person = new Person
				{
					Email = command.Email
				};
				person.SetName(new Name(command.FirstName, command.LastName));
				person.SetEmploymentNumber(command.EmploymentNumber);
				person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.FindSystemTimeZoneById(command.TimeZoneId));
				if (command.CultureLanguageId.HasValue)
					person.PermissionInformation.SetCulture(new CultureInfo(command.CultureLanguageId.Value));
				if(command.UICultureLanguageId.HasValue)
					person.PermissionInformation.SetUICulture(new CultureInfo(command.UICultureLanguageId.Value));
				if (command.WorkWeekStart.HasValue)
					person.FirstDayOfWeek = command.WorkWeekStart.Value;
				if (command.WorkflowControlSetId.HasValue)
				{
					var workflowControlSet = _workflowControlSetRepository.Get(command.WorkflowControlSetId.Value);
					person.WorkflowControlSet = workflowControlSet;
				}
				if (!string.IsNullOrEmpty(command.Note))
					person.Note = command.Note;
				if (command.IsDeleted)
					((IDeleteTag)person).SetDeleted();

				_personRepository.Add(person);
				uow.Flush();

				var saveTenantDataResult = _tenantDataManager.SaveTenantData(new TenantAuthenticationData
				{
					ApplicationLogonName = command.ApplicationLogonName,
					Password = command.ApplicationLogOnPassword,
					Identity = command.Identity,
					PersonId = person.Id.GetValueOrDefault()
				});
				if (saveTenantDataResult.Success)
				{
					uow.PersistAll();
					command.Result = new CommandResultDto { AffectedId = person.Id.GetValueOrDefault(), AffectedItems = 1 };
				}
				else
				{
					command.Result = new CommandResultDto { AffectedItems = 0 };
				}
			}
		}
	}
}