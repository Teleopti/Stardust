﻿using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Forecasting.Export;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;
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
		public DateOnlyPeriod Period { get; set; }

		public ExportMultisiteSkillToSkillCommandModel()
		{
			MultisiteSkillSelectionModels = new List<MultisiteSkillSelectionModel>();
			Period = new DateOnlyPeriod
				(
					new DateOnly( DateTime.Today.Date),
					new DateOnly(DateTime.Today.AddDays(30))
				);
		}

		public bool HasChildSkillMappings
		{
			get
			{
				return MultisiteSkillSelectionModels.Select(r => r.ChildSkillMappingModels.Count > 0).FirstOrDefault();
			}
		}

		public ExportMultisiteSkillsToSkill TransformToServiceBusMessage()
		{
			var message = new ExportMultisiteSkillsToSkill
			{
				OwnerPersonId =
					 ((IUnsafePerson)TeleoptiPrincipal.CurrentPrincipal).Person.Id.GetValueOrDefault(
						  Guid.Empty),
				PeriodStart = Period.StartDate.Date,
				PeriodEnd = Period.EndDate.Date
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