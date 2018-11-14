using System.Data;
using System.Drawing;
using System.Globalization;
using Syncfusion.Drawing;
using Syncfusion.Windows.Forms.Grid;
using Syncfusion.Windows.Forms.Grid.Grouping;
using Teleopti.Ccc.Domain.ResourcePlanner.Hints;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling
{
	public partial class AgentValidationResult : BaseDialogForm
	{
		private readonly HintResult _hintResult;

		public AgentValidationResult(HintResult hintResult)
		{
			_hintResult = hintResult;
			InitializeComponent();
			if (!DesignMode) SetTexts();
		}

		private void buttonAdvCloseClick(object sender, System.EventArgs e)
		{
			Close();
		}

		private void agentValidationResultLoad(object sender, System.EventArgs e)
		{
			setDataSource();
			masterGrid.TableControl.PrepareViewStyleInfo += tableControlPrepareViewStyleInfo;
			masterGrid.TableControl.CurrentCellStartEditing += tableControlCurrentCellStartEditing;
			masterGrid.TableOptions.ListBoxSelectionColorOptions = GridListBoxSelectionColorOptions.ApplySelectionColor;
			masterGrid.TableOptions.GridVisualStyles = Syncfusion.Windows.Forms.GridVisualStyles.Metro;
			masterGrid.Office2007ScrollBars = false;
			masterGrid.TableOptions.AllowDragColumns = false;
			masterGrid.TableDescriptor.Columns[0].HeaderText = UserTexts.Resources.Name;
			masterGrid.TableDescriptor.Columns[1].HeaderText = UserTexts.Resources.Message;
			masterGrid.TableModel.ColWidths.ResizeToFit(GridRangeInfo.Table(), GridResizeToFitOptions.IncludeHeaders);
		}

		private void setDataSource()
		{
			var dataTable = new DataTable("ValidationErrors") { Locale = CultureInfo.CurrentCulture };
			dataTable.Columns.Add(new DataColumn("Name"));
			dataTable.Columns.Add(new DataColumn("Message"));

			foreach (var validationResultInvalidResource in _hintResult.InvalidResources)
			{
				foreach (var validationError in validationResultInvalidResource.ValidationErrors)
				{
					var dataRow = dataTable.NewRow();
					dataRow[0] = validationResultInvalidResource.ResourceName;
					try
					{
						dataRow[1] = HintsHelper.BuildErrorMessage(validationError, UserTimeZone.Make());
					}
					catch (System.Exception)
					{
						dataRow[1] = UserTexts.Resources.ResourceManager.GetString(validationError.ErrorResource);
					}
					dataTable.Rows.Add(dataRow);
				}			
			}

			masterGrid.DataSource = dataTable;
		}

		private void tableControlCurrentCellStartEditing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			e.Cancel = true;
		}

		private void tableControlPrepareViewStyleInfo(object sender, GridPrepareViewStyleInfoEventArgs e)
		{
			var currentCell = masterGrid.TableControl.CurrentCell;
			var grid = masterGrid.TableControl.CurrentCell.Grid;
			if (e.RowIndex <= grid.Model.Rows.HeaderCount || e.ColIndex <= grid.Model.Cols.HeaderCount || !currentCell.HasCurrentCellAt(e.RowIndex)) return;
			e.Style.Interior = new BrushInfo(SystemColors.Highlight);
			e.Style.TextColor = SystemColors.HighlightText;
		}

		private void agentValidationResultFormClosing(object sender, System.Windows.Forms.FormClosingEventArgs e)
		{
			masterGrid.TableControl.PrepareViewStyleInfo -= tableControlPrepareViewStyleInfo;
			masterGrid.TableControl.CurrentCellStartEditing -= tableControlCurrentCellStartEditing;
		}
	}
}
