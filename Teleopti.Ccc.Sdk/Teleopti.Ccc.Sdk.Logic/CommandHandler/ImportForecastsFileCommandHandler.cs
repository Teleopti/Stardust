using System;
using System.Configuration;
using System.ServiceModel;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Client;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.Logic.CommandHandler
{
	public class ImportForecastsFileCommandHandler : IHandleCommand<ImportForecastsFileCommandDto>
	{
		private readonly IMessagePopulatingServiceBusSender _busSender;
		private readonly ICurrentUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IJobResultRepository _jobResultRepository;
		private readonly IPostHttpRequest _postHttpRequest;
		private readonly IToggleManager _toggleManager;
		private readonly IJsonSerializer _jsonSerializer;
		private readonly ICurrentBusinessUnit _currentBusinessUnit;
		private readonly IStardustSender _stardustSender;

		public ImportForecastsFileCommandHandler(IMessagePopulatingServiceBusSender busSender,
			ICurrentUnitOfWorkFactory unitOfWorkFactory,
			IJobResultRepository jobResultRepository,
			IPostHttpRequest postHttpRequest, IToggleManager toggleManager, IJsonSerializer jsonSerializer,
			ICurrentBusinessUnit currentBusinessUnit, IStardustSender stardustSender)
		{
			_busSender = busSender;
			_unitOfWorkFactory = unitOfWorkFactory;
			_jobResultRepository = jobResultRepository;
			_postHttpRequest = postHttpRequest;
			_toggleManager = toggleManager;
			_jsonSerializer = jsonSerializer;
			_currentBusinessUnit = currentBusinessUnit;
			_stardustSender = stardustSender;
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
			var person = ((IUnsafePerson)TeleoptiPrincipal.CurrentPrincipal).Person;
			using (var unitOfWork = _unitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				var jobResult = new JobResult(JobCategory.ForecastsImport, new DateOnlyPeriod(DateOnly.Today, DateOnly.Today),
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
					ImportMode = (ImportForecastsMode)((int)command.ImportForecastsMode),
					LogOnDatasource = _unitOfWorkFactory.Current().Name,
					LogOnBusinessUnitId = _currentBusinessUnit.Current().Id.GetValueOrDefault()

				};
				_stardustSender.Send(message);
			}
			command.Result = new CommandResultDto { AffectedId = jobResultId, AffectedItems = 1 };
		}
	}
	
}
