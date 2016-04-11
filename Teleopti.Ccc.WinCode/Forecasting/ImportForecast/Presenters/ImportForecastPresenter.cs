﻿using System;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
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
		private readonly IEventPublisher _eventPublisher;

		public ImportForecastPresenter(IImportForecastView view, ImportForecastModel model,
			ISaveImportForecastFileCommand saveImportForecastFileCommand,
			IValidateImportForecastFileCommand validateImportForecastFileCommand, IUnitOfWorkFactory unitOfWorkFactory,
			IJobResultRepository jobResultRepository,
			IEventPublisher eventPublisher)
		{
			_view = view;
			_model = model;
			_saveImportForecastFileCommand = saveImportForecastFileCommand;
			_validateImportForecastFileCommand = validateImportForecastFileCommand;
			_unitOfWorkFactory = unitOfWorkFactory;
			_jobResultRepository = jobResultRepository;
			_eventPublisher = eventPublisher;
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
					LogOnDatasource = _unitOfWorkFactory.Name,
					LogOnBusinessUnitId = _model.SelectedSkill.BusinessUnit.Id.GetValueOrDefault(),
					JobName = "Import forecast from file",
					InitiatorId = person.Id.GetValueOrDefault(),
					Type = typeof(ImportForecastsFileToSkill).ToString()
				};
				_eventPublisher.Publish(message);
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
