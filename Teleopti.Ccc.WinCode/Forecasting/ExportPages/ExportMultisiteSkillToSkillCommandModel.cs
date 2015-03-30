using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Forecasting.Export;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;
using Teleopti.Ccc.WinCode.Forecasting.QuickForecastPages;
using Teleopti.Interfaces.Messages.General;

namespace Teleopti.Ccc.WinCode.Forecasting.ExportPages
{
	public class ExportSkillModel : IDisposable
	{
		public ExportSkillModel(bool directExportPermitted, bool fileExportPermitted)
		{
			DirectExportPermitted = directExportPermitted;
			FileExportPermitted = fileExportPermitted;
			ExportMultisiteSkillToSkillCommandModel = new ExportMultisiteSkillToSkillCommandModel();
			ExportSkillToFileCommandModel = new ExportSkillToFileCommandModel();

			ExportToFile = fileExportPermitted;
		}

		public bool DirectExportPermitted { get; set; }
		public bool FileExportPermitted { get; set; }

		public ExportMultisiteSkillToSkillCommandModel ExportMultisiteSkillToSkillCommandModel { get; set; }
		public ExportSkillToFileCommandModel ExportSkillToFileCommandModel { get; set; }

		public void ChangeExportType(bool exportToFile)
		{
			ExportToFile = exportToFile;
		}

		public bool ExportToFile { get; private set; }

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				ExportMultisiteSkillToSkillCommandModel = null;
				ExportSkillToFileCommandModel = null;
			}
		}
	}

	public class ExportMultisiteSkillToSkillCommandModel
	{
		public ICollection<MultisiteSkillSelectionModel> MultisiteSkillSelectionModels { get; private set; }
		public DateOnlyPeriodDto Period { get; set; }

		public ExportMultisiteSkillToSkillCommandModel()
		{
			MultisiteSkillSelectionModels = new List<MultisiteSkillSelectionModel>();
			Period = new DateOnlyPeriodDto
				{
					StartDate = new DateOnlyDto { DateTime = DateTime.Today },
					EndDate = new DateOnlyDto { DateTime = DateTime.Today.AddDays(30) }
				};
		}

		public bool HasChildSkillMappings
		{
			get
			{
				return MultisiteSkillSelectionModels.Select(r => r.ChildSkillMappingModels.Count > 0).FirstOrDefault();
			}
		}

		public ExportMultisiteSkillToSkillCommandDto TransformToDto()
		{
			var exportMultisiteSkillToSkillCommandDto = new ExportMultisiteSkillToSkillCommandDto();
			exportMultisiteSkillToSkillCommandDto.Period = Period;

			foreach (var multisiteSkillSelectionModel in MultisiteSkillSelectionModels)
			{
				var multisiteSkillSelectionDto = new MultisiteSkillSelectionDto();
				multisiteSkillSelectionDto.MultisiteSkill = multisiteSkillSelectionModel.MultisiteSkillModel.SkillDto;

				foreach (var childSkillMappingModel in multisiteSkillSelectionModel.ChildSkillMappingModels)
				{
					var childSkillMappingDto = new ChildSkillMappingDto
					{
						SourceSkill = childSkillMappingModel.SourceSkill,
						TargetSkill = childSkillMappingModel.TargetSkill
					};
					multisiteSkillSelectionDto.ChildSkillMapping.Add(childSkillMappingDto);
				}
				exportMultisiteSkillToSkillCommandDto.MultisiteSkillSelection.Add(multisiteSkillSelectionDto);
			}

			return exportMultisiteSkillToSkillCommandDto;
		}

		public ExportMultisiteSkillsToSkill TransformToServiceBusMessage()
		{
			var command = TransformToDto();
			var message = new ExportMultisiteSkillsToSkill
			{
				OwnerPersonId =
					 ((IUnsafePerson)TeleoptiPrincipal.CurrentPrincipal).Person.Id.GetValueOrDefault(
						  Guid.Empty),
				Period = command.Period.ToDateOnlyPeriod()
			};

			foreach (var multisiteSkillSelection in command.MultisiteSkillSelection)
			{
				var selection = new MultisiteSkillSelection();
				selection.MultisiteSkillId = multisiteSkillSelection.MultisiteSkill.Id.GetValueOrDefault();

				foreach (var childSkillMappingDto in multisiteSkillSelection.ChildSkillMapping)
				{
					var childSkillMapping = new ChildSkillSelection
					{
						SourceSkillId = childSkillMappingDto.SourceSkill.Id.GetValueOrDefault(),
						TargetSkillId = childSkillMappingDto.TargetSkill.Id.GetValueOrDefault()
					};
					selection.ChildSkillSelections.Add(childSkillMapping);
				}
				message.MultisiteSkillSelections.Add(selection);
			}
			return message;
		}
	}
}