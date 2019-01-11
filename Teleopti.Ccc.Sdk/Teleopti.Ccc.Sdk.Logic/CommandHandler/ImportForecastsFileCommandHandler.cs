using System;
using System.ServiceModel;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;

namespace Teleopti.Ccc.Sdk.Logic.CommandHandler
{
	public class ImportForecastsFileCommandHandler : IHandleCommand<ImportForecastsFileCommandDto>
	{
		private readonly ICurrentUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IJobResultRepository _jobResultRepository;
		private readonly ICurrentBusinessUnit _currentBusinessUnit;
		private readonly IStardustSender _stardustSender;
		private readonly ILoggedOnUser _loggedOnUser;

		public ImportForecastsFileCommandHandler(ICurrentUnitOfWorkFactory unitOfWorkFactory,
			IJobResultRepository jobResultRepository,
			ICurrentBusinessUnit currentBusinessUnit, IStardustSender stardustSender,
			ILoggedOnUser loggedOnUser)
		{
			_unitOfWorkFactory = unitOfWorkFactory;
			_jobResultRepository = jobResultRepository;
			_currentBusinessUnit = currentBusinessUnit;
			_stardustSender = stardustSender;
			_loggedOnUser = loggedOnUser;
		}

		public void Handle(ImportForecastsFileCommandDto command)
		{
			if (command == null)
				throw new FaultException("Command is null.");
			if (!PrincipalAuthorization.Current().IsPermitted(DefinedRaptorApplicationFunctionPaths.ImportForecastFromFile))
			{
				throw new FaultException("You're not authorized to run this command.");
			}
			Guid jobResultId;
			using (var unitOfWork = _unitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				var person = _loggedOnUser.CurrentUser();
				var jobResult = new JobResult(JobCategory.ForecastsImport, DateOnly.Today.ToDateOnlyPeriod(),
														person, DateTime.UtcNow);
				_jobResultRepository.Add(jobResult);
				jobResultId = jobResult.Id.GetValueOrDefault();
				unitOfWork.PersistAll();

				var message = new ImportForecastsFileToSkillEvent
				{
					JobId = jobResultId,
					UploadedFileId = command.UploadedFileId,
					TargetSkillId = command.TargetSkillId,
					OwnerPersonId = person.Id.GetValueOrDefault(Guid.Empty),
					ImportMode = (ImportForecastsMode)(int)command.ImportForecastsMode,
					LogOnDatasource = _unitOfWorkFactory.Current().Name,
					LogOnBusinessUnitId = _currentBusinessUnit.Current().Id.GetValueOrDefault()

				};
				_stardustSender.Send(message);
			}
			command.Result = new CommandResultDto { AffectedId = jobResultId, AffectedItems = 1 };
		}
	}
	
}
