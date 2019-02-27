using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.PropertyPageAndWizard;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Forecasting.ExportPages;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Forecasting.Forms.ExportPages
{
	public partial class SelectDestination : BaseUserControl, IPropertyPageNoRoot<ExportSkillModel>
	{
		private readonly ICollection<string> _errorMessages = new List<string>();

		public SelectDestination()
		{
			InitializeComponent();
			setColors();
		}

		public void Populate(ExportSkillModel stateObj)
		{
			var skills = new List<DestinationSkillModel>();
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				foreach (var selection in stateObj.ExportMultisiteSkillToSkillCommandModel.MultisiteSkillSelectionModels)
				{
					var skill = MultisiteSkillRepository.DONT_USE_CTOR(uow).Get(selection.MultisiteSkillModel.Id);
					if (((IDeleteTag)skill).IsDeleted) continue;

					foreach (var childSkill in skill.ChildSkills)
					{
						skills.Add(new DestinationSkillModel(childSkill, selection.ChildSkillMappingModels));
					}
				}
			}

			initializeGrid();
			gridControlDestination.DataSource = skills;
		}

		protected override void OnDockChanged(System.EventArgs e)
		{
			base.OnDockChanged(e);

			resizeColumns();
		}

		private void gridCellButtonClicked(object sender, GridCellButtonClickedEventArgs e)
		{
			var model = ((IList<DestinationSkillModel>) gridControlDestination.DataSource)[e.RowIndex - 1]; //Syncfusion ?!?

			using(var mapDestinationBuSkill = new MapDestinationBuSkill(model))
			{
				if (mapDestinationBuSkill.ShowDialog(this) == DialogResult.OK)
				{
					var selectedSkill = mapDestinationBuSkill.SelectedSkill();
					if (selectedSkill == null)
					{
						var childSkillMappingModel= model.ChildSkillMapping.Where(m => m.SourceSkill.Equals(model.Skill.Id)).FirstOrDefault();
						if (childSkillMappingModel != null)
						{
							model.ChildSkillMapping.Remove(childSkillMappingModel);
							model.TargetBu = string.Empty;
							model.TargetSkill = string.Empty;
						}
					}
					else
					{
						//If exists in list, dont do anything
						var models = model.ChildSkillMapping.Where(s => s.SourceSkill.Equals(model.Skill.Id)).ToList();

						if (!models.IsEmpty())
							model.ChildSkillMapping.Remove(models.First());

						if (model.ChildSkillMapping.Any(m => m.TargetSkill.Equals(selectedSkill.Id))) return;

						var mappingModel = new ChildSkillMappingModel(model.Skill.Id.GetValueOrDefault(),
																	  selectedSkill.Id.GetValueOrDefault(),
																	  selectedSkill.BusinessUnit.Name,
																	  selectedSkill.Name);

						model.ChildSkillMapping.Add(mappingModel);
						model.TargetBu = selectedSkill.BusinessUnit.Name;
						model.TargetSkill = selectedSkill.Name;
					}
					gridControlDestination.Refresh();
				}
			}
		}

		public bool Depopulate(ExportSkillModel stateObj)
		{
			validate(stateObj.ExportMultisiteSkillToSkillCommandModel);
			
			gridControlDestination.CellButtonClicked -= gridCellButtonClicked;
			return true;
		}

		private void validate(ExportMultisiteSkillToSkillCommandModel stateObj)
		{
			var errorMessge = Resources.YouHaveToMapAtLeastOneSubSkill;
			if (stateObj.HasChildSkillMappings)
			{
				_errorMessages.Remove(errorMessge);
			}
			else
			{
				if (!_errorMessages.Contains(errorMessge))
					_errorMessages.Add(errorMessge);
			}
		}

		public void SetEditMode()
		{
		}

		public string PageName
		{
			get { return Resources.SelectDestination; }
		}

		public ICollection<string> ErrorMessages
		{
			get { return _errorMessages; }
		}

		private void setColors()
		{
			BackColor = ColorHelper.WizardBackgroundColor();
			gridControlDestination.Properties.BackgroundColor = ColorHelper.WizardPanelBackgroundColor();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		//Disposar dem i controlens dispose
		private void initializeGrid()
		{
			gridControlDestination.Model.Clear(true);
			gridControlDestination.Model.ReadOnly = true;
			gridControlDestination.GridBoundColumns.Clear();
			gridControlDestination.GridBoundColumns.Add(new GridBoundColumn() { MappingName = "ParentSkill", HeaderText = Resources.MultisiteSkill });
			gridControlDestination.GridBoundColumns.Add(new GridBoundColumn() { MappingName = "ChildSkill", HeaderText = Resources.SubSkill });
			gridControlDestination.GridBoundColumns.Add(new GridBoundColumn() { MappingName = "TargetBu", HeaderText =  Resources.TargetBusinessUnit});
			gridControlDestination.GridBoundColumns.Add(new GridBoundColumn() { MappingName = "TargetSkill", HeaderText = Resources.TargetSkill });
			
			var button = new GridBoundColumn
			{
				MappingName = "Map",
				HeaderText = " ",
				StyleInfo = {CellType = "PushButton", Description = "...", HorizontalAlignment = GridHorizontalAlignment.Right}
			};
			gridControlDestination.CellButtonClicked += gridCellButtonClicked;

			gridControlDestination.GridBoundColumns.Add(button);
			
			gridControlDestination.Properties.RowHeaders = false;
		}

		private void resizeColumns()
		{
			//Argh syncfusion
			var colWidth = (gridControlDestination.Width-34) / (gridControlDestination.Model.ColCount - 1); //first and map
			for (var i = 1; i < gridControlDestination.Model.ColCount; i++)
			{
				gridControlDestination.Model.ColWidths.SetSize(i, colWidth);
			}
			gridControlDestination.Model.ColWidths.SetSize(5, 22);
			gridControlDestination.Model.Refresh();
		}
	}
}
