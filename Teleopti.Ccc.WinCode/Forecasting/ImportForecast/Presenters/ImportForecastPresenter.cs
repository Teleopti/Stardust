using System;
using System.Configuration;
using Newtonsoft.Json;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Client;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.WinCode.Forecasting.ImportForecast.Models;
using Teleopti.Ccc.WinCode.Forecasting.ImportForecast.Views;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCode.Forecasting.ImportForecast.Presenters
{
	public class ImportForecastPresenter
	{
		private readonly IImportForecastView _view;
		private readonly ImportForecastModel _model;
		private readonly ISaveImportForecastFileCommand _saveImportForecastFileCommand;
		private readonly IValidateImportForecastFileCommand _validateImportForecastFileCommand;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IJobResultRepository _jobResultRepository;
		private readonly IMessagePopulatingServiceBusSender _messageSender;
		private readonly IPostHttpRequest _postHttpRequest;
		private readonly IToggleManager _toggleManager;

		public ImportForecastPresenter(IImportForecastView view, ImportForecastModel model,
			ISaveImportForecastFileCommand saveImportForecastFileCommand,
			IValidateImportForecastFileCommand validateImportForecastFileCommand, IUnitOfWorkFactory unitOfWorkFactory,
			IJobResultRepository jobResultRepository, IMessagePopulatingServiceBusSender messageSender,
			IPostHttpRequest postHttpRequest, IToggleManager toggleManager)
		{
			_view = view;
			_model = model;
			_saveImportForecastFileCommand = saveImportForecastFileCommand;
			_validateImportForecastFileCommand = validateImportForecastFileCommand;
			_unitOfWorkFactory = unitOfWorkFactory;
			_jobResultRepository = jobResultRepository;
			_messageSender = messageSender;
			_postHttpRequest = postHttpRequest;
			_toggleManager = toggleManager;
		}

		public void Initialize()
		{
			var skill = _model.SelectedSkill;
			_view.SetSkillName(skill.Name);

			var workload = _model.SelectedWorkload();
			if (workload == null)
				throw new InvalidOperationException("Workload should not be null.");
			_view.SetWorkloadName(workload.Name);
			_view.SetVisibility(skill.SkillType);
		}

		public void SetImportType(ImportForecastsMode importMode)
		{
			_model.ImportMode = importMode;
		}

		public void StartImport(string fileName)
		{
			_validateImportForecastFileCommand.Execute(fileName);
			if (_model.HasValidationError)
			{
				_view.ShowValidationException(_model.ValidationMessage);
				return;
			}

			_saveImportForecastFileCommand.Execute(fileName);

			if (_model.FileId == Guid.Empty)
			{
				_view.ShowError("Error occured when trying to import file.");
				return;
			}

			Guid jobResultId;
			var person = ((IUnsafePerson)TeleoptiPrincipal.CurrentPrincipal).Person;
			using (var unitOfWork = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
			{
				var jobResult = new JobResult(JobCategory.ForecastsImport, new DateOnlyPeriod(DateOnly.Today, DateOnly.Today),
														person, DateTime.UtcNow);
				_jobResultRepository.Add(jobResult);
				jobResultId = jobResult.Id.GetValueOrDefault();
				unitOfWork.PersistAll();

				var message = new ImportForecastsFileToSkill
				{
					JobId = jobResultId,
					UploadedFileId = _model.FileId,
					TargetSkillId = _model.SelectedSkill.Id.GetValueOrDefault(),
					OwnerPersonId = person.Id.GetValueOrDefault(Guid.Empty),
					ImportMode = _model.ImportMode,
					LogOnDatasource = _unitOfWorkFactory.Name
				};
				if (_toggleManager.IsEnabled(Toggles.Wfm_UseManagersAndNodes))
				{
					var ser = JsonConvert.SerializeObject(message);
					var jobModel = new JobRequestModel
					{
						Name = "Import forecast from file",
						Serialized = ser,
						Type = typeof(ImportForecastsFileToSkill).ToString(),
						//maybe remove the username?? not needed or have something more unique to identify "MY" jobs when viewing on page
						UserName = person.Name.FirstName
					};
					var mess = JsonConvert.SerializeObject(jobModel);
					_postHttpRequest.Send<Guid>(ConfigurationManager.AppSettings["ManagerLocation"] + "job", mess);
				}
				else
					_messageSender.Send(message, true);
			}

			_view.ShowStatusDialog(jobResultId);
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
