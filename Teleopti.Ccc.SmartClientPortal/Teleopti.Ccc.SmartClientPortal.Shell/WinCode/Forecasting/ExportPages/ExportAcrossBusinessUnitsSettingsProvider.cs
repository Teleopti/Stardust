using System.Collections.Generic;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Forecasting.ExportPages
{
	public class ExportAcrossBusinessUnitsSettingsProvider : IExportAcrossBusinessUnitsSettingsProvider
	{
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IRepositoryFactory _repositoryFactory;
		private IExportAcrossBusinessUnitsSettings _settings;

		public ExportAcrossBusinessUnitsSettingsProvider(IUnitOfWorkFactory unitOfWorkFactory,
																		 IRepositoryFactory repositoryFactory)
		{
			_unitOfWorkFactory = unitOfWorkFactory;
			_repositoryFactory = repositoryFactory;
		}

		public IExportAcrossBusinessUnitsSettings ExportAcrossBusinessUnitsSettings
		{
			get
			{
				if (_settings == null)
				{
					using (var uow = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
					{
						_settings = _repositoryFactory.CreateGlobalSettingDataRepository(uow).FindValueByKey
							 <IExportAcrossBusinessUnitsSettings>(
								  "ExportAcrossBusinessUnitsSettings", new ExportAcrossBusinessUnitsSettings());
					}
				}
				return _settings;
			}
		}

		public void Save()
		{
			using (var uow = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
			{
				_repositoryFactory.CreateGlobalSettingDataRepository(uow).PersistSettingValue(ExportAcrossBusinessUnitsSettings);
				uow.PersistAll();
			}
		}

		public IEnumerable<MultisiteSkillSelectionModel> TransformSerializableToSelectionModels()
		{
			var multisiteSkillSelectionModels = new List<MultisiteSkillSelectionModel>();
			foreach (var keyValuePair in ExportAcrossBusinessUnitsSettings.MultisiteSkillSelections)
			{
				var model = new MultisiteSkillSelectionModel
				{
					MultisiteSkillModel = new MultisiteSkillModel(keyValuePair.Key)
				};

				keyValuePair.Value.ForEach(m =>
				{
					var cModel = new ChildSkillMappingModel(
							  m.SourceSkillId,
							  m.TargetSkillId,
							  m.TargetBuName,
							  m.TargetSkillName);
					model.ChildSkillMappingModels.Add(cModel);
				});
				multisiteSkillSelectionModels.Add(model);
			}
			return multisiteSkillSelectionModels;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public void TransformToSerializableModel(IEnumerable<MultisiteSkillSelectionModel> multisiteSkillSelectionModels)
		{
			ExportAcrossBusinessUnitsSettings.MultisiteSkillSelections.Clear();
			foreach (var multisiteSkillSelectionModel in multisiteSkillSelectionModels)
			{
				var key = multisiteSkillSelectionModel.MultisiteSkillModel.Id;
				var value = new List<ChildSkillSelectionMapping>();
				multisiteSkillSelectionModel.ChildSkillMappingModels.ForEach(cm =>
				{
					var csMapping = new ChildSkillSelectionMapping
						 {
							 SourceSkillId = cm.SourceSkill,
							 TargetSkillId = cm.TargetSkill,
							 TargetBuName = cm.TargetBuName,
							 TargetSkillName = cm.TargetSkillName
						 };
					value.Add(csMapping);
				});
				ExportAcrossBusinessUnitsSettings.MultisiteSkillSelections.Add(key, value);
			}
		}
	}
}