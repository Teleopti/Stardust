using System;
using System.Configuration;
using System.ServiceModel;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Forecast;
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

		public ImportForecastsFileCommandHandler(IMessagePopulatingServiceBusSender busSender,
			ICurrentUnitOfWorkFactory unitOfWorkFactory,
			IJobResultRepository jobResultRepository,
			IPostHttpRequest postHttpRequest, IToggleManager toggleManager, IJsonSerializer jsonSerializer,
			ICurrentBusinessUnit currentBusinessUnit)
		{
			_busSender = busSender;
			_unitOfWorkFactory = unitOfWorkFactory;
			_jobResultRepository = jobResultRepository;
			_postHttpRequest = postHttpRequest;
			_toggleManager = toggleManager;
			_jsonSerializer = jsonSerializer;
			_currentBusinessUnit = currentBusinessUnit;
		}

		public void Handle(ImportForecastsFileCommandDto command)
		{
			if (command == null)
				throw new FaultException("Command is null.");
			if (!PrincipalAuthorization.Instance().IsPermitted(DefinedRaptorApplicationFunctionPaths.ImportForecastFromFile))
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

				var message = new ImportForecastsFileToSkill
				{
					JobId = jobResultId,
					UploadedFileId = command.UploadedFileId,
					TargetSkillId = command.TargetSkillId,
					OwnerPersonId = person.Id.GetValueOrDefault(Guid.Empty),
					ImportMode = (ImportForecastsMode)((int)command.ImportForecastsMode),
					LogOnDatasource = _unitOfWorkFactory.Current().Name,
					LogOnBusinessUnitId = _currentBusinessUnit.Current().Id.GetValueOrDefault()

				};
				if (_toggleManager.IsEnabled(Toggles.Wfm_ForecastFileImportOnStardust_37047))
				{
					var ser = _jsonSerializer.SerializeObject(message);
					var jobModel = new JobRequestModel
					{
						Name = "Import forecast from file",
						Serialized = ser,
						Type = typeof(ImportForecastsFileToSkill).ToString(),
						UserName = person.Name.FirstName
					};
					var mess = _jsonSerializer.SerializeObject(jobModel);
					_postHttpRequest.Send<Guid>(ConfigurationManager.AppSettings["ManagerLocation"] + "job", mess);
				}
				else
					_busSender.Send(message, true);
			}
			command.Result = new CommandResultDto { AffectedId = jobResultId, AffectedItems = 1 };
		}
	}
	internal class JobRequestModel
	{
		public string Name { get; set; }
		public string Serialized { get; set; }
		public string Type { get; set; }
		public string UserName { get; set; }
	}
}
