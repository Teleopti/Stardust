using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Forecasting.Export;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Messages.General;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Forecasting.ExportPages
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
		public DateOnlyPeriod Period { get; set; }

		public ExportMultisiteSkillToSkillCommandModel()
		{
			MultisiteSkillSelectionModels = new List<MultisiteSkillSelectionModel>();
			Period = new DateOnlyPeriod
				(
					DateOnly.Today,
					DateOnly.Today.AddDays(30)
				);
		}

		public bool HasChildSkillMappings
		{
			get
			{
				return MultisiteSkillSelectionModels.Any(r => r.ChildSkillMappingModels.Count > 0);
			}
		}

		public ExportMultisiteSkillsToSkillEvent TransformToServiceBusMessage()
		{
			var personId = ((ITeleoptiPrincipalForLegacy)TeleoptiPrincipal.CurrentPrincipal).UnsafePerson.Id.GetValueOrDefault(
				Guid.Empty);
			var message = new ExportMultisiteSkillsToSkillEvent
			{
				OwnerPersonId =
					 personId,
				PeriodStart = Period.StartDate.Date,
				PeriodEnd = Period.EndDate.Date,
				InitiatorId = personId
			};

			foreach (var multisiteSkillSelection in MultisiteSkillSelectionModels)
			{
				var selection = new MultisiteSkillSelection();
				selection.MultisiteSkillId = multisiteSkillSelection.MultisiteSkillModel.Id;

				foreach (var childSkillMappingModel in multisiteSkillSelection.ChildSkillMappingModels)
				{
					var childSkillMapping = new ChildSkillSelection
					{
						SourceSkillId = childSkillMappingModel.SourceSkill,
						TargetSkillId = childSkillMappingModel.TargetSkill
					};
					selection.ChildSkillSelections.Add(childSkillMapping);
				}
				message.MultisiteSkillSelections.Add(selection);
			}
			return message;
		}
	}

}